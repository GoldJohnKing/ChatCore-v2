using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ChatCore.Interfaces;
using ChatCore.Models;
using ChatCore.Utilities;
using Microsoft.Extensions.Logging;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace ChatCore.Services
{
	public class WebSocketServerProvider : IWebSocketServerService
	{
		private readonly ILogger _logger;
		private readonly object _lock;

		private WebSocketServer? _server;
		private CancellationTokenSource? _cancellationToken;

		public event Action<string, List<string>>? OnMessageReceived;
		public event Action<byte[], List<string>>? OnDataRecevied;

		public WebSocketServerProvider(ILogger<WebSocketServerProvider> logger)
		{
			_logger = logger;

			_lock = new object();
		}

		public void Start(IPAddress address, int port)
		{
			if (_server != null)
			{
				return;
			}

			_cancellationToken = new CancellationTokenSource();

			_server = new WebSocketServer(address, port);
			_server.AddWebSocketService<WebSocketServerBehavior>("/", () => createSession());
			_server.Start();

			if (_server.IsListening)
			{
				var services_str = "";
				foreach (var path in _server.WebSocketServices.Paths)
				{
					services_str += $" [{path}] ";
				}
					
				_logger.LogInformation($"[WebSocketServerProvider] | [Start] | Listening on {address}:{port}, and providing WebSocket services:{services_str}");
			}

			WebSocketServerBehavior createSession()
			{
				var _webSocketServerBehavior = new WebSocketServerBehavior();
				OnMessageReceived += _webSocketServerBehavior.SendMessage;
				OnDataRecevied += _webSocketServerBehavior.SendMessage;
				_webSocketServerBehavior.RemoveAllListener += removeListener;

				void removeListener()
				{
					OnMessageReceived -= _webSocketServerBehavior.SendMessage;
					OnDataRecevied -= _webSocketServerBehavior.SendMessage;
				}

				return _webSocketServerBehavior;
			}
		}

		public void Stop()
		{
			if (_cancellationToken is null)
			{
				return;
			}

			_server?.Stop();
			_cancellationToken.Cancel();
			_logger.LogInformation("[WebSocketServerProvider] | [Stop] | Stopped");
		}

		public void SendMessage(string message, List<string>? channels = null)
		{
			if (OnMessageReceived != null && channels != null)
			{
				OnMessageReceived.Invoke(message, channels);
			}
		}

		public void SendMessage(byte[] bytes, List<string>? channels = null)
		{
			if (OnDataRecevied != null && channels != null)
			{
				OnDataRecevied.Invoke(bytes, channels);
			}
		}
	}
}
