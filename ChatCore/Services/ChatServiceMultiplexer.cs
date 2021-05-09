using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ChatCore.Interfaces;
using ChatCore.Models.Twitch;
using ChatCore.Services.BiliBili;
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
		private readonly TwitchService _twitchService;
		private readonly BiliBiliService _bilibiliService;
		private readonly object _invokeLock = new object();

		public string DisplayName { get; }

		public ChatServiceMultiplexer(ILogger<ChatServiceMultiplexer> logger, IList<IChatService> streamingServices)
		{
			_logger = logger;
			_streamingServices = streamingServices;
			_twitchService = (TwitchService)streamingServices.First(s => s is TwitchService);
			_bilibiliService = (BiliBiliService)streamingServices.First(s => s is BiliBiliService);

			var displayNameBuilder = new StringBuilder();
			foreach (var service in _streamingServices)
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
			if (channel is TwitchChannel)
			{
				_twitchService.SendTextMessage(Assembly.GetCallingAssembly(), message, channel.Id);
			}
		}

		public TwitchService GetTwitchService()
		{
			return _twitchService;
		}

		public BiliBiliService GetBiliBiliService()
		{
			return _bilibiliService;
		}
	}
}
