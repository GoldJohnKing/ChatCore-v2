using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ChatCore.Interfaces;
using ChatCore.Utilities;
using Microsoft.Extensions.Logging;
using SuperSocket.ClientEngine;
using WebSocket4Net;

namespace ChatCore.Services
{
	public class WebSocket4NetServiceProvider : IWebSocketService
	{
		private readonly ILogger _logger;
		private readonly object _lock;
		private readonly SemaphoreSlim _reconnectLock;

		private WebSocket? _client;
		private string _uri = string.Empty;
		private string _userAgent = string.Empty;
		private string _origin = string.Empty;
		private List<KeyValuePair<string, string>>? _cookies = null;
		private CancellationTokenSource? _cancellationToken;
		private DateTime _startTime;

		public bool IsConnected => !(_client is null) && (_client?.State == WebSocketState.Open || _client?.State == WebSocketState.Connecting);
		public bool AutoReconnect { get; set; } = true;
		public int ReconnectDelay { get; set; } = 500;

		public event Action? OnOpen;
		public event Action? OnClose;
		public event Action? OnError;
		public event Action<Assembly, string>? OnMessageReceived;
		public event Action<Assembly, byte[]>? OnDataRecevied;

		public WebSocket4NetServiceProvider(ILogger<WebSocket4NetServiceProvider> logger)
		{
			_logger = logger;

			_lock = new object();
			_reconnectLock = new SemaphoreSlim(1, 1);
		}

		public void Connect(string uri, bool forceReconnect = false, string userAgent = "", string origin = "", List<KeyValuePair<string, string>>? cookies = null)
		{
			lock (_lock)
			{
				try
				{
					if (forceReconnect)
					{
						Dispose();
					}

					if (_client != null)
					{
						return;
					}

					_logger.LogDebug($"[WebSocket4NetServiceProvider] | [Connect] | Connecting to {uri}");
					_uri = uri;
					_userAgent = userAgent;
					_origin = origin;
					_cookies = cookies;

					_cancellationToken = new CancellationTokenSource();
					Task.Run(async () =>
					{
						try
						{
							_client = new WebSocket(uri, "", cookies, null, userAgent, origin);
							_client.Opened += _client_Opened;
							_client.Closed += _client_Closed;
							//_client.Error += _client_Error;
							_client.MessageReceived += _client_MessageReceived;
							_client.DataReceived += _client_DataReceived;
							_startTime = DateTime.UtcNow;

							await _client.OpenAsync();
						}
						catch (TaskCanceledException)
						{
							_logger.LogInformation("[WebSocket4NetServiceProvider] | [Connect] | WebSocket client task was cancelled");
						}
						catch (Exception ex)
						{
							_logger.LogError(ex, $"[WebSocket4NetServiceProvider] | [Connect] | An exception occurred in WebSocket while connecting to {_uri}");
							OnError?.Invoke();
							TryHandleReconnect();
						}
					}, _cancellationToken.Token);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, $"[WebSocket4NetServiceProvider] | [Connect] | An exception occurred while trying to connect");
				}
			}
		}

		private void _client_DataReceived(object sender, DataReceivedEventArgs e)
		{
			_logger.LogDebug($"[WebSocket4NetServiceProvider] | [_client_DataReceived] | Message received from {_uri}");
			OnDataRecevied?.Invoke(null!, e.Data);
		}

		private void _client_MessageReceived(object sender, MessageReceivedEventArgs e)
		{
			_logger.LogDebug($"[WebSocket4NetServiceProvider] | [_client_MessageReceived] | Message received from {_uri}: {e.Message}");

			OnMessageReceived?.Invoke(null!, e.Message);
		}

		private void _client_Opened(object sender, EventArgs e)
		{
			_logger.LogDebug($"[WebSocket4NetServiceProvider] | [_client_Opened] | Connection to {_uri} opened successfully!");
			OnOpen?.Invoke();
		}

		private void _client_Error(object sender, ErrorEventArgs e)
		{
			_logger.LogError(e.Exception, $"[WebSocket4NetServiceProvider] | [_client_Error] | An error occurred in WebSocket while connected to {_uri}");
			OnError?.Invoke();
			TryHandleReconnect();
		}

		private void _client_Closed(object sender, EventArgs e)
		{
			_logger.LogDebug($"[WebSocket4NetServiceProvider] | [_client_Closed] | WebSocket connection to {_uri} was closed");
			OnClose?.Invoke();
			TryHandleReconnect();
		}

		private async void TryHandleReconnect()
		{
			try
			{
				_logger.LogInformation($"[WebSocket4NetServiceProvider] | [TryHandleReconnect] | Connection was closed after {(DateTime.UtcNow - _startTime).ToShortString()}.");
				if (!await _reconnectLock.WaitAsync(0))
				{
					//_logger.LogInformation("[WebSocket4NetServiceProvider] | [TryHandleReconnect] | Not trying to reconnect, connectLock already locked.");
					return;
				}

				if (_client != null)
				{
					_client.Opened -= _client_Opened;
					_client.Closed -= _client_Closed;
					_client.Error -= _client_Error;
					_client.MessageReceived -= _client_MessageReceived;
					_client.DataReceived -= _client_DataReceived;
					_client.Dispose();
					_client = null;
				}

				if (AutoReconnect && (!_cancellationToken!.IsCancellationRequested))
				{
					_logger.LogInformation($"[WebSocket4NetServiceProvider] | [TryHandleReconnect] | Trying to reconnect to {_uri} in {(int)TimeSpan.FromMilliseconds(ReconnectDelay).TotalSeconds} sec");
					try
					{
						await Task.Delay(ReconnectDelay, _cancellationToken.Token);
						Connect(_uri, false, _userAgent, _origin, _cookies);
						ReconnectDelay *= 2;
						if (ReconnectDelay > 60000)
						{
							ReconnectDelay = 60000;
						}
					}
					catch (TaskCanceledException) { }
				}
				_reconnectLock.Release();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"[WebSocket4NetServiceProvider] | [TryHandleReconnect] | An exception occurred while trying to handle reconnect");
			}
		}

		public void Disconnect()
		{
			lock (_lock)
			{
				try
				{
					_logger.LogInformation("[WebSocket4NetServiceProvider] | [Disconnect] | Disconnecting");
					_cancellationToken?.Cancel();
					Dispose();
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, $"[WebSocket4NetServiceProvider] | [Disconnect] | An exception occurred while trying to disconnect");
				}
			}
		}

		public void SendMessage(string message)
		{
			try
			{
				if (IsConnected)
				{
#if DEBUG
					_logger.LogDebug($"[WebSocket4NetServiceProvider] | [SendMessage] | Sending {message}"); // Only log this in debug builds, since it can potentially contain sensitive auth data
#endif
					_client!.Send(message);
				}
				else
				{
					_logger.LogDebug("[WebSocket4NetServiceProvider] | [SendMessage] | WebSocket not connected, can't send message!");
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"[WebSocket4NetServiceProvider] | [SendMessage] | An exception occurred while trying to send message to {_uri}");
			}
		}

		public void SendMessage(byte[] bytes)
		{
			try
			{
				if (IsConnected)
				{
#if DEBUG
					_logger.LogDebug($"[WebSocket4NetServiceProvider] | [SendMessage] | Sending bytes"); // Only log this in debug builds, since it can potentially contain sensitive auth data
#endif
					_client!.Send(bytes, 0, bytes.Length);
				}
				else
				{
					_logger.LogDebug("[WebSocket4NetServiceProvider] | [SendMessage] | WebSocket not connected, can't send bytes!");
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"[WebSocket4NetServiceProvider] | [SendMessage] | An exception occurred while trying to send message to {_uri}");
			}
		}

		public void Dispose()
		{
			try
			{
				if (_client == null)
				{
					return;
				}

				if (IsConnected)
				{
					_cancellationToken?.Cancel();
					_client.Close();
				}

				if (_client == null)
				{
					return;
				}

				_client.Dispose();
				_client = null;
			} catch (ObjectDisposedException ex)
			{
				_logger.LogError("[WebSocket4NetServiceProvider] | [Dispose] | Caught: {0}", ex.Message);
			}
		}
	}
}
