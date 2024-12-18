using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ChatCore.Interfaces;
using ChatCore.Models.Twitch;
using ChatCore.Services.Bilibili;
using ChatCore.Services.Twitch;
using ChatCore.Utilities;
using Microsoft.Extensions.Logging;

namespace ChatCore.Services
{
	/// <summary>
	/// A multiplexer for all the supported streaming services.
	/// </summary>
	public class ChatServiceMultiplexer : ChatServiceBase, IChatService
	{
		private readonly ILogger _logger;
		private readonly IList<IChatService> _streamingServices;
		private static TwitchService? _twitchService;
		private static BilibiliService? _bilibiliService;
		private readonly object _invokeLock = new object();
		private readonly object _initLock = new object();
		public bool twitchEnable { get; private set; }
		public bool bilibiliEnable { get; private set; }

		public string DisplayName { get; internal set; }

		public ChatServiceMultiplexer(ILogger<ChatServiceMultiplexer> logger, IList<IChatService> streamingServices, IOpenBLiveProvider openBLiveServices, bool twitchEnable, bool bilibiliEnable)
		{
			_logger = logger;
			_streamingServices = streamingServices;
			_twitchService = (TwitchService)streamingServices.First(s => s is TwitchService);
			_bilibiliService = (BilibiliService)streamingServices.First(s => s is BilibiliService);

			var displayNameBuilder = new StringBuilder();
			foreach (var service in _streamingServices)
			{
				if ((service.DisplayName == "Twitch" && twitchEnable) || (service.DisplayName == "Bilibili Live" && bilibiliEnable))
				{
					service.OnTextMessageReceived += Service_OnTextMessageReceived;
					service.OnJoinChannel += Service_OnJoinChannel;
					service.OnRoomStateUpdated += Service_OnRoomStateUpdated;
					service.OnLeaveChannel += Service_OnLeaveChannel;
					service.OnLogin += Service_OnLogin;
					service.OnChatCleared += Service_OnChatCleared;
					service.OnMessageCleared += Service_OnMessageCleared;
					service.OnChannelResourceDataCached += Service_OnChannelResourceDataCached;

					if (displayNameBuilder.Length > 0)
					{
						displayNameBuilder.Append(", ");
					}

					displayNameBuilder.Append(service.DisplayName);
				}
			}

			DisplayName = displayNameBuilder.Length > 0 ? displayNameBuilder.ToString() : "Generic";
		}

		private void Service_OnChannelResourceDataCached(IChatService svc, IChatChannel channel, Dictionary<string, IChatResourceData> resources)
		{
			lock (_invokeLock)
			{
				ChannelResourceDataCached.InvokeAll(Assembly.GetCallingAssembly(), svc, channel, resources, _logger);
			}
		}

		private void Service_OnMessageCleared(IChatService svc, string messageId)
		{
			lock (_invokeLock)
			{
				MessageClearedCallbacks.InvokeAll(Assembly.GetCallingAssembly(), svc, messageId, _logger);
			}
		}

		private void Service_OnChatCleared(IChatService svc, string userId)
		{
			lock (_invokeLock)
			{
				ChatClearedCallbacks.InvokeAll(Assembly.GetCallingAssembly(), svc, userId, _logger);
			}
		}

		private void Service_OnLogin(IChatService svc)
		{
			lock (_invokeLock)
			{
				LoginCallbacks.InvokeAll(Assembly.GetCallingAssembly(), svc, _logger);
			}
		}

		private void Service_OnLeaveChannel(IChatService svc, IChatChannel channel)
		{
			lock (_invokeLock)
			{
				LeaveRoomCallbacks.InvokeAll(Assembly.GetCallingAssembly(), svc, channel, _logger);
			}
		}

		private void Service_OnRoomStateUpdated(IChatService svc, IChatChannel channel)
		{
			lock (_invokeLock)
			{
				RoomStateUpdatedCallbacks.InvokeAll(Assembly.GetCallingAssembly(), svc, channel, _logger);
			}
		}

		private void Service_OnTextMessageReceived(IChatService svc, IChatMessage message)
		{
			lock (_invokeLock)
			{
				TextMessageReceivedCallbacks.InvokeAll(Assembly.GetCallingAssembly(), svc, message, _logger);
			}
		}

		private void Service_OnJoinChannel(IChatService svc, IChatChannel channel)
		{
			lock (_invokeLock)
			{
				JoinRoomCallbacks.InvokeAll(Assembly.GetCallingAssembly(), svc, channel, _logger);
			}
		}

		public void SendTextMessage(string message, IChatChannel channel)
		{
			if (channel is TwitchChannel && _twitchService != null)
			{
				_twitchService.SendTextMessage(Assembly.GetCallingAssembly(), message, channel.Id);
			}
		}

		public TwitchService? GetTwitchService()
		{
			return _twitchService;
		}

		public BilibiliService? GetBilibiliService()
		{
			return _bilibiliService;
		}

		public void EnableTwitchService()
		{
			lock (_initLock)
			{
				_logger.LogInformation("[ChatServiceMultiplexer] | [EnableTwitchService]");
				//var service = twitchService ?? GetTwitchService();
				if (_twitchService != null && !_twitchService._enable)
				{
					EnableService(_twitchService);
				}
			}
		}

		public void EnableBilibiliService()
		{
			lock (_initLock)
			{
				if (_bilibiliService != null)
				{
					if (_bilibiliService._enable)
					{
						_bilibiliService.reloadWebsocketConnection();
					}
					else
					{
						EnableService(_bilibiliService);
					}
				}
			}
		}

		private void EnableService(IChatService service)
		{
			service.OnTextMessageReceived += Service_OnTextMessageReceived;
			service.OnJoinChannel += Service_OnJoinChannel;
			service.OnRoomStateUpdated += Service_OnRoomStateUpdated;
			service.OnLeaveChannel += Service_OnLeaveChannel;
			service.OnLogin += Service_OnLogin;
			service.OnChatCleared += Service_OnChatCleared;
			service.OnMessageCleared += Service_OnMessageCleared;
			service.OnChannelResourceDataCached += Service_OnChannelResourceDataCached;

			var displayNames = DisplayName.Split(',').ToList();
			displayNames.Remove("Generic");
			displayNames.Add(service.DisplayName);
			DisplayName = string.Join(", ", displayNames.ToArray());
			// DisplayName = DisplayName.Substring(0, DisplayName.Length - 2);
			service.Enable();
		}

		public void DisableTwitchService()
		{
			_logger.LogInformation("[ChatServiceMultiplexer] | [DisableTwitchService]");
			//var service = twitchService ?? GetTwitchService();
			if (_twitchService != null && !twitchEnable)
			{
				DisableService(_twitchService);
				twitchEnable = false;
			}
		}

		public void DisableBilibiliService()
		{
			_logger.LogInformation("[ChatServiceMultiplexer] | [DisableBilibiliService]");
			if (_bilibiliService != null && !bilibiliEnable)
			{
				DisableService(_bilibiliService);
				bilibiliEnable = false;
			}
		}

		private void DisableService(IChatService service)
		{
			service.OnTextMessageReceived -= Service_OnTextMessageReceived;
			service.OnJoinChannel -= Service_OnJoinChannel;
			service.OnRoomStateUpdated -= Service_OnRoomStateUpdated;
			service.OnLeaveChannel -= Service_OnLeaveChannel;
			service.OnLogin -= Service_OnLogin;
			service.OnChatCleared -= Service_OnChatCleared;
			service.OnMessageCleared -= Service_OnMessageCleared;
			service.OnChannelResourceDataCached -= Service_OnChannelResourceDataCached;

			var displayNames = DisplayName.Split(',').ToList();
			displayNames.Remove(service.DisplayName);
			DisplayName = string.Join(", ", displayNames.ToArray());
			// DisplayName = DisplayName.Substring(0, DisplayName.Length - 2);
			if (DisplayName.Length == 0)
			{
				DisplayName = "Generic";
			}
			service.Disable();
		}

		public void Enable()
		{

		}

		public void Disable()
		{

		}
	}
}
