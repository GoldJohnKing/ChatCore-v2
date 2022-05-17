using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ChatCore.Interfaces;
using ChatCore.Models;
using ChatCore.Models.BiliBili;
using ChatCore.Utilities;
using Microsoft.Extensions.Logging;

namespace ChatCore.Services.BiliBili
{
	public class BiliBiliService : ChatServiceBase, IChatService
	{
		private readonly ConcurrentDictionary<Assembly, Action<IChatService, string>> _rawMessageReceivedCallbacks;
		private readonly ConcurrentDictionary<string, IChatChannel> _channels;
		private static HttpClient httpClient = new HttpClient() { Timeout = TimeSpan.FromSeconds(5) };
		private static readonly string BilibiliChannelInfoApi = "https://api.live.bilibili.com/xlive/web-room/v1/index/getInfoByRoom?room_id=";
		private static readonly string BilibiliGiftRoomInfoApi = "https://api.live.bilibili.com/xlive/web-room/v1/giftPanel/giftConfig?platform=pc&room_id=";

		private readonly ILogger _logger;
		//private readonly TwitchMessageParser _messageParser;
		//private readonly TwitchDataProvider _dataProvider;
		private readonly IWebSocketService _websocketService;
		private readonly MainSettingsProvider _settings;

		private readonly object _messageReceivedLock;
		private readonly object _initLock;

		private bool _isStarted;

		private int _currentMessageCount;
		private DateTime _lastResetTime = DateTime.UtcNow;
		private readonly ConcurrentQueue<KeyValuePair<Assembly, string>> _textMessageQueue = new ConcurrentQueue<KeyValuePair<Assembly, string>>();

		public BiliBiliChatUser? LoggedInUser { get; internal set; }
		private int _roomID = 0;
		private int _userID = 0;
		private int _randomUid = 0;

		private readonly System.Timers.Timer packetTimer;

		public ReadOnlyDictionary<string, IChatChannel> Channels { get; }

		public static Dictionary<string, dynamic> bilibiliGiftInfo { get; internal set; } = new Dictionary<string, dynamic>();
		public static Dictionary<string, dynamic> bilibiliGiftCoinType { get; internal set; } = new Dictionary<string, dynamic>();
		public static Dictionary<string, dynamic> bilibiliGiftPrice { get; internal set; } = new Dictionary<string, dynamic>();
		public static Dictionary<string, dynamic> bilibiliGiftName { get; internal set; } = new Dictionary<string, dynamic>();
		public static Dictionary<string, dynamic> bilibiliuserInfo { get; set; } = new Dictionary<string, dynamic>();

		public string DisplayName { get; } = "BiliBili Live";

		public event Action<IChatService, string> OnRawMessageReceived
		{
			add => _rawMessageReceivedCallbacks.AddAction(Assembly.GetCallingAssembly(), value);
			remove => _rawMessageReceivedCallbacks.RemoveAction(Assembly.GetCallingAssembly(), value);
		}

		public BiliBiliService(ILogger<BiliBiliService> logger, IWebSocketService websocketService, MainSettingsProvider settings, Random rand)
		{
			_logger = logger;
			_settings = settings;
			_websocketService = websocketService;

			_rawMessageReceivedCallbacks = new ConcurrentDictionary<Assembly, Action<IChatService, string>>();
			_channels = new ConcurrentDictionary<string, IChatChannel>();
			_messageReceivedLock = new object();
			_initLock = new object();
			_roomID = _settings.bilibili_room_id;

			_randomUid = rand.Next(10000, 1000000);

			Channels = new ReadOnlyDictionary<string, IChatChannel>(_channels);

			_websocketService.OnOpen += _websocketService_OnOpen;
			_websocketService.OnClose += _websocketService_OnClose;
			_websocketService.OnError += _websocketService_OnError;
			_websocketService.OnMessageReceived += _websocketService_OnMessageReceived;
			_websocketService.OnDataRecevied += _websocketService_OnDataRecevied;

			packetTimer = new System.Timers.Timer(1000 * 30);
			packetTimer.Elapsed += PacketTimer_Elapsed;
		}

		private void _websocketService_OnDataRecevied(Assembly arg1, byte[] arg2)
		{
			/*_logger.LogInformation("Get bytes packet!");*/
			//var buffer = new byte[arg2.Length];
			// Receive the greeting ack notify, then a HeartBeat timer should be setup.
			foreach (var message in DanmakuMessage.ParsePackets(arg2))
			{
				/*_logger.LogInformation("Operation: " + message.Operation.ToString());*/
				if (message.Operation == BiliBiliPacket.DanmakuOperation.GreetingAck)
				{
					/*Console.WriteLine("BiliBili Connected");*/
					_logger.LogInformation("BiliBili Connected");
					StartHeartBeat();
				} else if (message.Operation == BiliBiliPacket.DanmakuOperation.HeartBeatAck)
				{
					/*Console.WriteLine($"Popularity: {message.Body}");*/
					/*_logger.LogInformation($"Popularity: {message.Body}");*/
				} else if (message.Operation == BiliBiliPacket.DanmakuOperation.ChatMessage) {
					/*Console.WriteLine($"Body: {message.Body}");*/
					/*_logger.LogInformation($"Body: {message.Body}");*/
					try
					{
						_rawMessageReceivedCallbacks?.InvokeAll(arg1, this, message.Body);
						var bmessage = new BiliBiliChatMessage(message.Body, _settings.bilibili_room_id);
						BanDetection(bmessage);
						if (bmessage.MessageType != "banned" && ShowDanmuku(bmessage.MessageType) && (!string.IsNullOrEmpty(bmessage.Message) || !int.TryParse(bmessage.Message, out _)))
						{
							TextMessageReceivedCallbacks?.InvokeAll(arg1, this, bmessage);
						}
					}
					catch (Exception r)
					{
						_logger.LogError($"{r}");
					}
				} else if (message.Operation == BiliBiliPacket.DanmakuOperation.StopRoom) {

				}  else
				{
					_logger.LogInformation($"Unknown Msg(Body: {message.Body})");
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
				var NewChannelInfo = JSONNode.Parse(await httpClient.GetStringAsync(BilibiliChannelInfoApi + roomID));
				if (NewChannelInfo["data"]["room_info"]["room_id"] != string.Empty)
				{
					_userID = int.Parse(NewChannelInfo["data"]["room_info"]["uid"]);
					_roomID = int.Parse(NewChannelInfo["data"]["room_info"]["room_id"]);
					_settings.bilibili_room_id = _roomID;
					_settings.Save();
					LoggedInUser = new BiliBiliChatUser();
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
				var NewGiftInfo = JSONNode.Parse(await httpClient.GetStringAsync(BilibiliGiftRoomInfoApi + roomID));
				if (NewGiftInfo["code"].AsInt == 0)
				{
					var giftList = NewGiftInfo["data"]["list"].AsArray!;
					foreach (JSONObject gift in giftList)
					{
						var gift_id = gift["id"].ToString();
						bilibiliGiftCoinType[gift_id] = gift["coin_type"];
						bilibiliGiftInfo[gift_id] = (gift["gif"].IsNull) ? gift["img_basic"] : gift["gif"];
						bilibiliGiftPrice[gift_id] = gift["price"].AsInt / 1000;
						bilibiliGiftName[gift_id] = gift["name"];
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
				if (!_isStarted)
				{
					_logger.LogInformation($"BiliBili Start");
					GetChannelConfigAsync(_roomID);
					GetChannelGiftRoomInfoAsync(_roomID);
					Task.Run(() => {
						Thread.Sleep(1000);
						_isStarted = true;
						_websocketService.Connect("wss://broadcastlv.chat.bilibili.com:443/sub", forceReconnect);
						Task.Run(ProcessQueuedMessages);
						//_logger.LogInformation("BiliBili Connected");
					});
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
				TextMessageReceivedCallbacks?.InvokeAll(assembly, this, new BiliBiliChatMessage(rawMessage, _settings.bilibili_room_id), _logger);
			}
		}

		private void _websocketService_OnClose()
		{
			_logger.LogInformation("BiliBili live connection closed");
		}

		private void _websocketService_OnError()
		{
			_logger.LogError("An error occurred in Bilibili connection");
		}

		private void _websocketService_OnOpen()
		{
			_logger.LogInformation("BiliBili live connection opened");
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
				_logger.LogWarning("WebSocket service is not connected!");
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
				Thread.Sleep(10);
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
		/*public void SendRawMessage(string rawMessage, bool forwardToSharedClients = false)
		{
			// TODO: rate limit sends to Twitch service
			SendRawMessage(Assembly.GetCallingAssembly(), rawMessage, forwardToSharedClients);
		}*/

		internal void SendTextMessage(Assembly assembly, string message, string channel)
		{
			//_textMessageQueue.Enqueue(new KeyValuePair<Assembly, string>(assembly, $"@id={Guid.NewGuid().ToString()} PRIVMSG #{channel} :{message}"));
		}

		/*public void SendTextMessage(string message, string channel)
		{
			SendTextMessage(Assembly.GetCallingAssembly(), message, channel);
		}*/

		public void SendTextMessage(string message, IChatChannel channel)
		{
			if (channel is BiliBiliChatChannel)
			{
				SendTextMessage(Assembly.GetCallingAssembly(), message, channel.Id);
			}
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
			_logger.LogInformation("Send Greeting packet.");
			var packet = BiliBiliPacket.CreateGreetingPacket(_randomUid, _roomID);
			_websocketService.SendMessage(packet.PacketBuffer);
		}

		private void SendHeartBeatPacket()
		{
			if (!_websocketService.IsConnected)
			{
				return;
			}

			var packet = BiliBiliPacket.CreateHeartBeatPacket();
			_websocketService.SendMessage(packet.PacketBuffer);
		}

		private void BanDetection(BiliBiliChatMessage msg) {
			if (BanListDetect(msg.Uid.ToString(), "uid") || BanListDetect(msg.Username, "username") || BanListDetect(msg.Content, "content"))
			{
				Console.WriteLine("Ban!");
				msg.BanMessage();
			}
		}

		private bool BanListDetect(string value, string mode)
		{
			switch (mode)
			{
				case "username":
					//Console.WriteLine($"Username: {value}");
					JSONArray usernameArray = JSON.Parse(_settings.bilibili_block_list_username).AsArray!;
					foreach (var username in usernameArray)
					{
						if (value.Contains(username.Value))
						{
							return true;
						}
					}
					return false;
				case "uid":
					//Console.WriteLine($"UID: {value}");
					JSONArray uidArray = JSON.Parse(_settings.bilibili_block_list_uid).AsArray!;
					foreach (var uid in uidArray)
					{
						if (int.Parse(value) == int.Parse(uid.Value))
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
			var result = false;
			string[] danmuku = { "danmuku", "danmuku_motion" };
			string[] sc = { "super_chat", "super_chat_japanese" };
			string[] gift = { "gift" };
			string[] gift_combo = { "combo_end", "combo_send" };
			string[] welcome = { "welcome" };
			string[] share = { "share" };
			string[] follow = { "follow" };
			string[] special_follow = { "special_follow" };
			string[] matual_follow = { "matual_follow" };
			string[] welcome_guard = { "welcome_guard" };
			string[] effect = { "effect" };
			string[] anchor = { "anchor_lot_start", "anchor_lot_checkstatus", "anchor_lot_end", "anchor_lot" };
			string[] raffle = { "raffle_start" };
			string[] new_guard = { "new_guard" };
			string[] new_guard_msg = { "new_guard_msg" };
			string[] guard_msg = { "guard_msg" };
			string[] guard_lottery_msg = { "guard_lottery_msg" };
			string[] blocklist = { "blocklist" };
			string[] room_change = { "room_change" };
			string[] room_perparing = { "room_perparing" };
			string[] room_live = { "room_live" };
			string[] global = { "global" };
			string[] junk = { "junk", "unkown", "banned" };
			string[] system = { "warning", "cut_off" };
			string[] pk = {"pk_pre", "pk_start", "pk_end", "common_notice"};

			if (
				(Array.Exists(danmuku, el => el == type) && _settings.danmuku_danmuku) ||
				(Array.Exists(sc, el => el == type) && _settings.danmuku_superchat) ||
				(Array.Exists(gift, el => el == type) && _settings.danmuku_gift) ||
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
				(Array.Exists(new_guard, el => el == type) && _settings.danmuku_new_guard) ||
				(Array.Exists(new_guard_msg, el => el == type) && _settings.danmuku_new_guard_msg) ||
				(Array.Exists(guard_msg, el => el == type) && _settings.danmuku_guard_msg) ||
				(Array.Exists(guard_lottery_msg, el => el == type) && _settings.danmuku_guard_lottery) ||
				(Array.Exists(blocklist, el => el == type) && _settings.danmuku_notification_block_list) ||
				(Array.Exists(room_change, el => el == type) && _settings.danmuku_notification_room_info_change) ||
				(Array.Exists(room_perparing, el => el == type) && _settings.danmuku_notification_room_prepare) ||
				(Array.Exists(room_live, el => el == type) && _settings.danmuku_notification_room_online) ||
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
	}
}
