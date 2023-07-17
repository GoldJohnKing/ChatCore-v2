using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using ChatCore.Interfaces;
using ChatCore.Models;
using ChatCore.Models.Bilibili;
using ChatCore.Models.Twitch;
using ChatCore.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Timer = System.Timers.Timer;

namespace ChatCore.Services.Bilibili
{
	public class BilibiliService : ChatServiceBase, IChatService
	{
		private readonly ConcurrentDictionary<Assembly, Action<IChatService, string>> _rawMessageReceivedCallbacks;
		private readonly ConcurrentDictionary<string, IChatChannel> _channels;
		private static HttpClient httpClient = new HttpClient() { Timeout = TimeSpan.FromSeconds(5) };
		private static readonly string BilibiliChannelInfoApi = "https://api.live.bilibili.com/xlive/web-room/v1/index/getInfoByRoom?room_id=";
		private static readonly string BilibiliGiftRoomInfoApi = "https://api.live.bilibili.com/xlive/web-room/v1/giftPanel/giftConfig?platform=pc&room_id=";
		private static readonly string BilibiliWealthApi = "https://api.live.bilibili.com/xlive/general-interface/v1/content/get?key=wealth";

		private readonly ILogger _logger;
		private readonly IWebSocketService _websocketService;
		private readonly IUserAuthProvider _authManager;
		private readonly MainSettingsProvider _settings;

		private readonly object _messageReceivedLock;
		private readonly object _initLock;

		private bool _isStarted;
		private bool _enable;

		private int _currentMessageCount;
		private DateTime _lastResetTime = DateTime.UtcNow;
		private readonly ConcurrentQueue<KeyValuePair<Assembly, string>> _textMessageQueue = new ConcurrentQueue<KeyValuePair<Assembly, string>>();

		public BilibiliChatUser? LoggedInUser { get; internal set; }
		private int RoomID => _authManager.Credentials.Bilibili_room_id;
		private string IdentityCode => string.IsNullOrEmpty(_authManager.Credentials.Bilibili_identity_code) ? string.Empty : _authManager.Credentials.Bilibili_identity_code;
		public static int _roomID { get; internal set; } = 0;
		public static int _userID { get; internal set; } = 0;

		private int _randomUid = 0;

		private readonly System.Timers.Timer packetTimer;

		public ReadOnlyDictionary<string, IChatChannel> Channels { get; }

		public static Dictionary<string, dynamic> bilibiliGiftInfo { get; internal set; } = new Dictionary<string, dynamic>();
		public static Dictionary<string, dynamic> bilibiliGiftCoinType { get; internal set; } = new Dictionary<string, dynamic>();
		public static Dictionary<string, dynamic> bilibiliGiftPrice { get; internal set; } = new Dictionary<string, dynamic>();
		public static Dictionary<string, dynamic> bilibiliGiftName { get; internal set; } = new Dictionary<string, dynamic>();
		public static Dictionary<string, dynamic> bilibiliuserInfo { get; set; } = new Dictionary<string, dynamic>();
		public static Dictionary<string, string> bilibiliWealth { get; set; } = new Dictionary<string, string>();

		private static Dictionary<string, BilibiliGiftTimer> giftTimerDict = new Dictionary<string, BilibiliGiftTimer>();

		private static readonly string[] danmuku = { "danmuku", "danmuku_motion" };
		private static readonly string[] sc = { "super_chat", "super_chat_japanese" };
		private static readonly string[] gift = { "gift" };
		private static readonly string[] gift_star = { "gift_star" };
		private static readonly string[] gift_combo = { "combo_end", "combo_send" };
		private static readonly string[] welcome = { "welcome" };
		private static readonly string[] share = { "share" };
		private static readonly string[] follow = { "follow" };
		private static readonly string[] special_follow = { "special_follow" };
		private static readonly string[] matual_follow = { "matual_follow" };
		private static readonly string[] welcome_guard = { "welcome_guard" };
		private static readonly string[] effect = { "effect" };
		private static readonly string[] anchor = { "anchor_lot_start", "anchor_lot_checkstatus", "anchor_lot_end", "anchor_lot" };
		private static readonly string[] raffle = { "raffle_start" };
		private static readonly string[] new_guard = { "new_guard" };
		private static readonly string[] new_guard_msg = { "new_guard_msg" };
		private static readonly string[] guard_msg = { "guard_msg" };
		private static readonly string[] guard_lottery_msg = { "guard_lottery_msg" };
		private static readonly string[] blocklist = { "blocklist" };
		private static readonly string[] room_change = { "room_change" };
		private static readonly string[] room_perparing = { "room_perparing" };
		private static readonly string[] room_live = { "room_live" };
		private static readonly string[] like = { "like_info" };
		private static readonly string[] global = { "global" };
		private static readonly string[] junk = { "junk", "unkown", "banned" };
		private static readonly string[] system = { "warning", "cut_off", "plugin_message" };
		private static readonly string[] pk = { "pk_pre", "pk_start", "pk_end", "common_notice" };
		private static readonly string[] red_packet = { "red_pocket_start", "red_pocket_new", "red_pocket_result" };

		public static bool typeInList(string msgType, string type)
		{
			string[] target = { };
			switch (type)
			{
				case "danmuku":
					target = danmuku;
					break;
				case "sc":
					target = sc;
					break;
				case "gift":
					target = gift;
					break;
				case "gift_star":
					target = gift_star;
					break;
				case "gift_combo":
					target = gift_combo;
					break;
				case "welcome":
					target = welcome;
					break;
				case "share":
					target = share;
					break;
				case "follow":
					target = follow;
					break;
				case "special_follow":
					target = special_follow;
					break;
				case "matual_follow":
					target = matual_follow;
					break;
				case "welcome_guard":
					target = welcome_guard;
					break;
				case "effect":
					target = effect;
					break;
				case "anchor":
					target = anchor;
					break;
				case "raffle":
					target = raffle;
					break;
				case "new_guard":
					target = new_guard;
					break;
				case "new_guard_msg":
					target = new_guard_msg;
					break;
				case "guard_msg":
					target = guard_msg;
					break;
				case "guard_lottery_msg":
					target = guard_lottery_msg;
					break;
				case "blocklist":
					target = blocklist;
					break;
				case "room_change":
					target = room_change;
					break;
				case "room_perparing":
					target = room_perparing;
					break;
				case "room_live":
					target = room_live;
					break;
				case "like":
					target = like;
					break;
				case "global":
					target = global;
					break;
				case "junk":
					target = junk;
					break;
				case "system":
					target = system;
					break;
				case "pk":
					target = pk;
					break;
				case "red_packet":
					target = red_packet;
					break;
			}

			return target.ToList().Contains(msgType);
		}

		public delegate void GiftTimerHandler(Assembly arg1, BilibiliChatMessage bmessage);
		//public GiftTimerHandler GiftDelegate;

		public string BilibiliLiveAppSecretChecker()
		{
#if (DEBUG)
			var config = new ConfigurationBuilder().AddUserSecrets(Assembly.GetExecutingAssembly(), true).Build();
			return $"bilibili_live_app_id: {config["bilibili_live_app_id"]}\nbilibili_live_access_key_id: {config["bilibili_live_access_key_id"]}\nbilibili_live_access_key_secret: {config["bilibili_live_access_key_secret"]}";
#elif (RELEASE)
            return $"bilibili_live_app_id: HIDDEN\nbilibili_live_access_key_id: HIDDEN\nbilibili_live_access_key_secret: HIDDEN";
#endif
		}

		public string DisplayName { get; } = "BilibiliLive";

		public event Action<IChatService, string> OnRawMessageReceived
		{
			add => _rawMessageReceivedCallbacks.AddAction(Assembly.GetCallingAssembly(), value);
			remove => _rawMessageReceivedCallbacks.RemoveAction(Assembly.GetCallingAssembly(), value);
		}

		public BilibiliService(ILogger<BilibiliService> logger, IWebSocketService websocketService, MainSettingsProvider settings, IUserAuthProvider authManager, Random rand)
		{
			_logger = logger;
			_settings = settings;
			_websocketService = websocketService;
			_authManager = authManager;

			_rawMessageReceivedCallbacks = new ConcurrentDictionary<Assembly, Action<IChatService, string>>();
			_channels = new ConcurrentDictionary<string, IChatChannel>();
			_messageReceivedLock = new object();
			_initLock = new object();
			_roomID = _authManager.Credentials.Bilibili_room_id;

			_randomUid = rand.Next(10000, 1000000);

			Channels = new ReadOnlyDictionary<string, IChatChannel>(_channels);

			_websocketService.OnOpen += _websocketService_OnOpen;
			_websocketService.OnClose += _websocketService_OnClose;
			_websocketService.OnError += _websocketService_OnError;
			_websocketService.OnMessageReceived += _websocketService_OnMessageReceived;
			_websocketService.OnDataRecevied += _websocketService_OnDataRecevied;
			_authManager.OnBilibiliCredentialsUpdated += _authManager_OnCredentialsUpdated;

			packetTimer = new System.Timers.Timer(1000 * 30);
			packetTimer.Elapsed += PacketTimer_Elapsed;
		}

		private void _authManager_OnCredentialsUpdated(LoginCredentials credentials)
		{
			if (_isStarted && _enable)
			{
				_roomID = _authManager.Credentials.Bilibili_room_id;
				// Console.WriteLine($"Connecting to {_roomID}");
				Start(true);
			}
		}

		private void _websocketService_OnDataRecevied(Assembly arg1, byte[] arg2)
		{
			/*_logger.LogInformation("Get bytes packet!");*/
			//var buffer = new byte[arg2.Length];
			// Receive the greeting ack notify, then a HeartBeat timer should be setup.
			foreach (var message in DanmakuMessage.ParsePackets(arg2))
			{
				/*_logger.LogInformation("Operation: " + message.Operation.ToString());*/
				if (message.Operation == BilibiliPacket.DanmakuOperation.GreetingAck)
				{
					_logger.LogInformation("[BilibiliService] | [ws_OnDataRecevied] | Bilibili Connected");
					if (!_channels.ContainsKey($"{RoomID}"))
					{
						_channels[$"{RoomID}"] = new BilibiliChatChannel();
						_logger.LogInformation($"[BilibiliService] | [ws_OnDataRecevied] | Added channel {RoomID} to the channel list.");
						JoinRoomCallbacks?.InvokeAll(arg1, this, _channels[$"{RoomID}"], _logger);
					}
					StartHeartBeat();
				} else if (message.Operation == BilibiliPacket.DanmakuOperation.HeartBeatAck)
				{
					/*Console.WriteLine($"Popularity: {message.Body}");*/
					/*_logger.LogInformation($"Popularity: {message.Body}");*/
				} else if (message.Operation == BilibiliPacket.DanmakuOperation.ChatMessage) {
					/*Console.WriteLine($"Body: {message.Body}");*/
					/*_logger.LogInformation($"Body: {message.Body}");*/
					try
					{
						_rawMessageReceivedCallbacks?.InvokeAll(arg1, this, message.Body);
						DanmukuProcessor(arg1, message.Body);
					}
					catch (Exception r)
					{
						_logger.LogError($"[BilibiliService] | [ws_OnDataRecevied] | {r}");
					}
				} else if (message.Operation == BilibiliPacket.DanmakuOperation.StopRoom) {

				}  else
				{
					_logger.LogInformation($"[BilibiliService] | [ws_OnDataRecevied] | Unknown Msg(Body: {message.Body})");
				}
			}
		}

		public void DanmukuProcessor(Assembly arg1, string body)
		{
			var bmessage = new BilibiliChatMessage(body);
			BanDetection(bmessage);
			if (bmessage.MessageType != "banned" && ShowDanmuku(bmessage.MessageType) && (!string.IsNullOrEmpty(bmessage.Message) || !int.TryParse(bmessage.Message, out _)))
			{
				// Gift Delay
				if (_settings.danmuku_gift_combine && (Array.Exists(gift, el => el == bmessage.MessageType) || Array.Exists(gift_combo, el => el == bmessage.MessageType)))
				{
					enqueueGiftDanmuku(arg1, bmessage);
				}
				else
				{
					TextMessageReceivedCallbacks?.InvokeAll(arg1, this, bmessage);
				}
			}
		}

		private void StartHeartBeat()
		{
			packetTimer.Start();
		}

		private async void GetChannelConfigAsync(int roomID) {
			try
			{
				httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/107.0.0.0 Safari/537.36");
				var NewChannelInfo = JSONNode.Parse(await httpClient.GetStringAsync(BilibiliChannelInfoApi + roomID));
				if (NewChannelInfo["data"]["room_info"]["room_id"] != string.Empty)
				{
					_userID = int.Parse(NewChannelInfo["data"]["room_info"]["uid"]);
					_roomID = int.Parse(NewChannelInfo["data"]["room_info"]["room_id"]);
					_authManager.Credentials.Bilibili_room_id = _roomID;
					LoggedInUser = new BilibiliChatUser();
					LoggedInUser.Id = _userID.ToString();
					LoggedInUser.UserName = NewChannelInfo["data"]["anchor_info"]["base_info"]["uname"]!;
					LoggedInUser.DisplayName = LoggedInUser.UserName;
					LoggedInUser.Color = "#FF0000";
					LoggedInUser.IsBroadcaster = true;
					LoggedInUser.IsModerator = true;
					LoggedInUser.IsFan = true;
				}
			}
			catch { }
		}

		private async void GetChannelGiftRoomInfoAsync(int roomID)
		{
			try
			{
				httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/107.0.0.0 Safari/537.36");
				var NewGiftInfo = JSONNode.Parse(await httpClient.GetStringAsync(BilibiliGiftRoomInfoApi + roomID));
				if (NewGiftInfo["code"].AsInt == 0)
				{
					var giftList = NewGiftInfo["data"]["list"].AsArray!;
					foreach (JSONObject gift in giftList)
					{
						var gift_id = gift["id"].ToString();
						bilibiliGiftCoinType[gift_id] = gift["coin_type"];
						bilibiliGiftInfo[gift_id] = (gift["gif"].IsNull) ? gift["img_basic"] : gift["gif"];
						bilibiliGiftPrice[gift_id] = Math.Round(gift["price"].AsInt / 1000.0f, 1);
						bilibiliGiftName[gift_id] = gift["name"];
					}
				}
			}
			catch { }
		}

		private async void GetWealthAsync()
		{
			try
			{
				httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/107.0.0.0 Safari/537.36");
				var NewWealthInfo = JSONNode.Parse(await httpClient.GetStringAsync(BilibiliWealthApi));
				if (NewWealthInfo["code"].AsInt == 0)
				{
					var wealthList = JSONNode.Parse(NewWealthInfo["data"]["content"]!)["wealth_level_medal"].AsArray!;
					foreach (JSONObject wealth in wealthList)
					{
						var wealth_id = wealth["id"].ToString();
						bilibiliWealth[wealth_id] = wealth["url"]!;
					}
				}
			}
			catch { }
		}

		private void PacketTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			SendHeartBeatPacket();
		}

		internal void Start(bool forceReconnect = false)
		{
			if (forceReconnect)
			{
				Stop();
			}
			lock (_initLock)
			{
				if (!_isStarted && _enable)
				{
					_logger.LogInformation($"[BilibiliService] | [ws_OnDataRecevied] | [Bilibili] | Start");
					GetChannelConfigAsync(_roomID);
					GetChannelGiftRoomInfoAsync(_roomID);
					GetWealthAsync();

					_isStarted = true;
					_websocketService.Connect("wss://broadcastlv.chat.bilibili.com:443/sub", forceReconnect);
					Task.Run(ProcessQueuedMessages);

					/*Task.Run(async () =>
					{
						await Task.Delay(1000);
						_isStarted = true;
						_websocketService.Connect("wss://broadcastlv.chat.bilibili.com:443/sub", forceReconnect);
						await ProcessQueuedMessages().ConfigureAwait(false);
					}).ConfigureAwait(false);*/
				}
			}
		}

		internal void Stop()
		{
			lock (_initLock)
			{
				if (!_isStarted)
				{
					return;
				}
				packetTimer?.Stop();
				_isStarted = false;
				_channels?.Clear();
				LoggedInUser = null;
				_websocketService?.Disconnect(); 
			}
		}

		private void _websocketService_OnMessageReceived(Assembly assembly, string rawMessage)
		{
			lock (_messageReceivedLock)
			{
				//_logger.LogInformation("RawMessage: " + rawMessage);

				_rawMessageReceivedCallbacks?.InvokeAll(assembly, this, rawMessage);
				TextMessageReceivedCallbacks?.InvokeAll(assembly, this, new BilibiliChatMessage(rawMessage), _logger);
			}
		}

		private void _websocketService_OnClose()
		{
			if (_channels.TryRemove($"{RoomID}", out var channel))
			{
				_logger.LogInformation($"[BilibiliService] | [ws_OnClose] | Removed channel {RoomID} from the channel list.");
				//LeaveRoomCallbacks?.InvokeAll(arg1, this, channel, _logger);
			}
			_logger.LogInformation("[BilibiliService] | [ws_OnClose] | Bilibili live connection closed");
		}

		private void _websocketService_OnError()
		{
			_logger.LogError("[BilibiliService] | [ws_OnError] | An error occurred in Bilibili connection");
		}

		private void _websocketService_OnOpen()
		{
			_logger.LogInformation("[BilibiliService] | [ws_OnOpen] | Bilibili live connection opened");
			SendGreetingPacket();
		}

		private void SendRawMessage(Assembly assembly, string rawMessage, bool forwardToSharedClients = false)
		{
			if (_websocketService.IsConnected)
			{
				//_websocketService.SendMessage(rawMessage);
				if (forwardToSharedClients)
				{
					_websocketService_OnMessageReceived(assembly, rawMessage);
				}
			}
			else
			{
				_logger.LogWarning("[BilibiliService] | [ws_send] | WebSocket service is not connected!");
			}
		}

		private async Task ProcessQueuedMessages()
		{
			while (_isStarted)
			{
				if (_currentMessageCount >= 20)
				{
					var remainingMilliseconds = (float)(30000 - (DateTime.UtcNow - _lastResetTime).TotalMilliseconds);
					if (remainingMilliseconds > 0)
					{
						await Task.Delay((int)remainingMilliseconds);
					}
				}
				if ((DateTime.UtcNow - _lastResetTime).TotalSeconds >= 30)
				{
					_currentMessageCount = 0;
					_lastResetTime = DateTime.UtcNow;
				}

				if (_textMessageQueue.TryDequeue(out var msg))
				{
					SendRawMessage(msg.Key, msg.Value, true);
					_currentMessageCount++;
				}
				await Task.Delay(10);
			}
		}

		/// <summary>
		/// Sends a raw message to the Twitch server
		/// </summary>
		/// <param name="rawMessage">The raw message to send.</param>
		/// <param name="forwardToSharedClients">
		/// Whether or not the message should also be sent to other clients in the assembly that implement StreamCore, or only to the Twitch server.<br/>
		/// This should only be set to true if the Twitch server would rebroadcast this message to other external clients as a response to the message.
		/// </param>
		public void SendRawMessage(string rawMessage, bool forwardToSharedClients = false)
		{
			// SendRawMessage(Assembly.GetCallingAssembly(), rawMessage, forwardToSharedClients);
		}

		internal void SendTextMessage(Assembly assembly, string message, string channel)
		{
			// Fake
			var bilibiliChatMessage = new BilibiliChatMessage("{\"cmd\": \"plugin_message\"}");
			bilibiliChatMessage.Message = message;

			TextMessageReceivedCallbacks?.InvokeAll(assembly, this, bilibiliChatMessage);
			//_textMessageQueue.Enqueue(new KeyValuePair<Assembly, string>(assembly, $"@id={Guid.NewGuid().ToString()} PRIVMSG #{channel} :{message}"));
		}

		/*public void SendTextMessage(string message, string channel)
		{
			SendTextMessage(Assembly.GetCallingAssembly(), message, channel);
		}*/

		public void SendTextMessage(string message, IChatChannel channel)
		{
			/*if (channel == null)
			{
				channel = _channels[0];
			}
			if (channel is BilibiliChatChannel)
			{
				SendTextMessage(Assembly.GetCallingAssembly(), message, channel.Id);
			}*/
			SendTextMessage(Assembly.GetCallingAssembly(), message, channel.Id);
		}

		/*public void SendCommand(string command, string channel)
		{
			SendRawMessage(Assembly.GetCallingAssembly(), $"PRIVMSG #{channel} :/{command}");
		}

		public void JoinChannel(string channel)
		{
			_logger.LogInformation($"Trying to join channel #{channel}");
			SendRawMessage(Assembly.GetCallingAssembly(), $"JOIN #{channel.ToLower()}");
		}

		public void PartChannel(string channel)
		{
			SendRawMessage(Assembly.GetCallingAssembly(), $"PART #{channel.ToLower()}");
		}*/

		private void SendGreetingPacket()
		{
			_logger.LogInformation($"[BilibiliService] | [SendGreetingPacket] | Send Greeting packet. Connect to room {_roomID}");
			var packet = BilibiliPacket.CreateGreetingPacket(_randomUid, _roomID);
			_websocketService.SendMessage(packet.PacketBuffer);
		}

		private void SendHeartBeatPacket()
		{
			if (!_websocketService.IsConnected)
			{
				return;
			}

			var packet = BilibiliPacket.CreateHeartBeatPacket();
			_websocketService.SendMessage(packet.PacketBuffer);
		}

		private void BanDetection(BilibiliChatMessage msg) {
			if (BanListDetect(msg.Uid.ToString(), "uid") || BanListDetect(msg.Username, "username") || BanListDetect(msg.Content, "content"))
			{
				_logger.Log(LogLevel.Information, $"[BilibiliService] | [BanDetection] | \"{msg.Username}(UID: {msg.Uid}): {msg.Content}\" has been banned!");
				msg.BanMessage();
			}
		}

		private bool BanListDetect(string value, string mode)
		{
			if (string.IsNullOrEmpty(value))
			{
				return false;
			}

			switch (mode)
			{
				case "username":
					JSONArray usernameArray = JSON.Parse(_settings.bilibili_block_list_username).AsArray!;
					foreach (var username in usernameArray)
					{
						if (value.Contains(username.Value.Value))
						{
							return true;
						}
						else if (username.Value.Value.StartsWith("#")) // Match username that represent numbers
						{
							var num = "";
							var target_username = value;
							var rule = username.Value.Value.Substring(1);
							target_username = target_username
								.Replace("一", "1").Replace("壹", "1").Replace("①", "1")
								.Replace("二", "2").Replace("贰", "2").Replace("②", "2")
								.Replace("三", "3").Replace("叁", "3").Replace("③", "3")
								.Replace("四", "4").Replace("肆", "4").Replace("④", "4")
								.Replace("五", "5").Replace("伍", "5").Replace("⑤", "5")
								.Replace("六", "6").Replace("陆", "6").Replace("⑥", "6")
								.Replace("七", "7").Replace("柒", "7").Replace("⑦", "7")
								.Replace("八", "8").Replace("捌", "8").Replace("⑧", "8")
								.Replace("九", "9").Replace("玖", "9").Replace("⑨", "9")
								.Replace("零", "0").Replace("〇", "0")
								;
							foreach (var character in target_username)
							{
								if (int.TryParse(character.ToString(), out _))
								{
									num += character;
								}
							}
							return (num.Equals(rule));
						}
					}
					return false;
				case "uid":
					//Console.WriteLine($"UID: {value}");
					JSONArray uidArray = JSON.Parse(_settings.bilibili_block_list_uid).AsArray!;
					foreach (var uid in uidArray)
					{
						if (value == uid.Value)
						{
							return true;
						}
					}
					return false;
				case "content":
					//Console.WriteLine($"Content: {value}");
					JSONArray keywordArray = JSON.Parse(_settings.bilibili_block_list_keyword).AsArray!;
					foreach (var keywords in keywordArray)
					{
						if (value.Contains(keywords.Value))
						{
							return true;
						}
					}
					return false;
				default:
					return false;
			}
		}

		private bool ShowDanmuku(string type) {
			var result = false;;
			if (
				(Array.Exists(danmuku, el => el == type) && _settings.danmuku_danmuku) ||
				(Array.Exists(sc, el => el == type) && _settings.danmuku_superchat) ||
				(Array.Exists(gift, el => el == type) && _settings.danmuku_gift) ||
				(Array.Exists(gift_star, el => el == type) && _settings.danmuku_gift_star) ||
				(Array.Exists(gift_combo, el => el == type) && _settings.danmuku_gift_combo) ||
				(Array.Exists(welcome, el => el == type) && _settings.danmuku_interaction_enter) ||
				(Array.Exists(share, el => el == type) && _settings.danmuku_interaction_share) ||
				(Array.Exists(follow, el => el == type) && _settings.danmuku_interaction_follow) ||
				(Array.Exists(special_follow, el => el == type) && _settings.danmuku_interaction_special_follow) ||
				(Array.Exists(matual_follow, el => el == type) && _settings.danmuku_interaction_mutual_follow) ||
				(Array.Exists(welcome_guard, el => el == type) && _settings.danmuku_interaction_guard_enter) ||
				(Array.Exists(effect, el => el == type) && _settings.danmuku_interaction_effect) ||
				(Array.Exists(anchor, el => el == type) && _settings.danmuku_interaction_anchor) ||
				(Array.Exists(raffle, el => el == type) && _settings.danmuku_interaction_raffle) ||
				(Array.Exists(red_packet, el => el == type) && _settings.danmuku_interaction_red_packet) ||
				(Array.Exists(new_guard, el => el == type) && _settings.danmuku_new_guard) ||
				(Array.Exists(new_guard_msg, el => el == type) && _settings.danmuku_new_guard_msg) ||
				(Array.Exists(guard_msg, el => el == type) && _settings.danmuku_guard_msg) ||
				(Array.Exists(guard_lottery_msg, el => el == type) && _settings.danmuku_guard_lottery) ||
				(Array.Exists(blocklist, el => el == type) && _settings.danmuku_notification_block_list) ||
				(Array.Exists(room_change, el => el == type) && _settings.danmuku_notification_room_info_change) ||
				(Array.Exists(room_perparing, el => el == type) && _settings.danmuku_notification_room_prepare) ||
				(Array.Exists(room_live, el => el == type) && _settings.danmuku_notification_room_online) ||
				(Array.Exists(like, el => el == type) && _settings.danmuku_notification_like) ||
				(Array.Exists(global, el => el == type) && _settings.danmuku_notification_boardcast) ||
				(Array.Exists(junk, el => el == type) && _settings.danmuku_notification_junk) ||
				(Array.Exists(pk, el => el == type) && _settings.danmuku_notification_pk) ||
				(Array.Exists(system, el => el == type))
				)
			{
				result = true;
			}

			return result;
		}

		private void enqueueGiftDanmuku(Assembly arg1, BilibiliChatMessage bmessage) {
			var extra = (BilibiliChatMessageExtraGift)(bmessage.extra);
			var TimerName = $"{bmessage.Uid}_{extra.gift_id}";

			if (giftTimerDict.TryGetValue(TimerName, out var GiftTimer))
			{
				giftTimerDict[TimerName].AddNumber(extra.gift_num);
				giftTimerDict[TimerName].UpdateSender(bmessage.Sender as BilibiliChatUser ?? new BilibiliChatUser());
				giftTimerDict[TimerName].UpdateArg1(arg1);
				giftTimerDict[TimerName].ResetTimer();
			}
			else
			{
				giftTimerDict.Add(TimerName, new BilibiliGiftTimer(TimerName, extra.gift_action, extra.gift_id, extra.gift_name, extra.gift_num, bilibiliGiftPrice[extra.gift_id], bilibiliGiftInfo[extra.gift_id], bilibiliGiftCoinType[extra.gift_id], bmessage.Sender as BilibiliChatUser, arg1, bmessage));
				giftTimerDict[TimerName].Elapsed += (sender, e) =>
				{
					((BilibiliGiftTimer)sender).GetMessage();
					TextMessageReceivedCallbacks?.InvokeAll(((BilibiliGiftTimer)sender).Arg1, this, ((BilibiliGiftTimer)sender).bmessage);
					((BilibiliGiftTimer)sender).Close();
					giftTimerDict.Remove(((BilibiliGiftTimer)sender).Name);
				};
				giftTimerDict[TimerName].Start();
			}
		}

		public void connectBilibili() {
			Start(true);
		}

		public void disconnectBilibili()
		{
			Stop();
		}

		public void Enable()
		{
			_enable = true;
			Start();
		}

		public void Disable()
		{
			_enable = false;
			Stop();
		}
	}
}
