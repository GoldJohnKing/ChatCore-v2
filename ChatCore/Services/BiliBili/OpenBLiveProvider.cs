using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using ChatCore.Interfaces;
using ChatCore.Models;
using ChatCore.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenBLive.Client;
using OpenBLive.Client.Data;
using OpenBLive.Runtime;
using OpenBLive.Runtime.Data;
using OpenBLive.Runtime.Utilities;

namespace ChatCore.Services.Bilibili
{
	public class OpenBLiveProvider : IOpenBLiveProvider
	{
		private readonly ILogger _logger;
		private readonly IUserAuthProvider _authManager;
		private static OpenBLiveProvider? _openBLiveService;

		private IBApiClient? _bApiClient;
		private AppStartInfo? _appStartInfo;
		private IConfigurationRoot _config;
		private string? _gameId;
		private string? _appId;
		private InteractivePlayHeartBeat? _heatbeat;
		private CancellationTokenSource? _cancellationToken;
		private CancellationTokenSource _reconnectCancellationToken;
		private string? _status;
		private Task? _deamon;
		private bool _enable = false;

		public OpenBLiveProvider(ILogger<WebLoginProvider> logger, IUserAuthProvider authManager)
		{
			if (_openBLiveService == null)
			{
				_openBLiveService = this;
			}
			_status = "init";
			_logger = logger;
			_authManager = authManager;
			_config = new ConfigurationBuilder().AddUserSecrets(Assembly.GetExecutingAssembly(), true).Build();
			_authManager.OnBilibiliCredentialsUpdated += _authManager_OnCredentialsUpdated;
			_reconnectCancellationToken = new CancellationTokenSource();
		}

		private void _authManager_OnCredentialsUpdated(LoginCredentials credentials)
		{
			if (_bApiClient != null)
			{
				Stop(true);
				Task.Run(async () =>
				{
					while (true)
					{
						if (_bApiClient != null)
						{
							await Task.Delay(500);
						}
						else
						{
							await Task.Delay(5000);
							Start();
							break;
						}
					}
				});
			}
		}

		public void Start(bool reconnect = false)
		{
			if (_bApiClient != null)
			{
				_status = "Instance already exists";
				return;
			}

			if (string.IsNullOrEmpty(_authManager.Credentials.Bilibili_identity_code) || string.IsNullOrEmpty(_config["bilibili_live_app_id"]) || string.IsNullOrEmpty(_config["bilibili_live_access_key_id"]) || string.IsNullOrEmpty(_config["bilibili_live_access_key_secret"]))
			{
				_status = "Lack of parameters";
				return;
			}

			if (!_enable)
			{
				_status = "Service not Enabled!";
				Stop();
				return;
			}

			if (_deamon == null)
			{
				_deamon = Task.Run(async () =>
				{
					while (true)
					{
						if (_reconnectCancellationToken.IsCancellationRequested)
						{
							break;
						}
						if (reconnect)
						{
							await Task.Delay(5000);
							Start(true);
						}
						else
						{
							Start();
						}
						await Task.Delay(15000);
					}
				}, _reconnectCancellationToken.Token);
				return;
			}

			_logger.LogInformation($"[OpenBLiveProvider] | [Start] | Start");
			_reconnectCancellationToken = new CancellationTokenSource();
			_bApiClient = new BApiClient();
			_appStartInfo = new AppStartInfo();
			_appId = _config["bilibili_live_app_id"]!;
			_status = "Starting";

			Task.Run(async () =>
			{
				BApi.isTestEnv = false;
				SignUtility.accessKeyId = _config["bilibili_live_access_key_id"]!;
				SignUtility.accessKeySecret = _config["bilibili_live_access_key_secret"]!;

				_appStartInfo = await _bApiClient!.StartInteractivePlay(_authManager.Credentials.Bilibili_identity_code, _appId);

				if (_appStartInfo?.Code != 0)
				{
					_logger.LogCritical($"[OpenBLiveProvider] | [Start] | Connecting Error: {_appStartInfo?.Code}, _appStartInfo?.Message");
					_status = $"Connecting Error: {_appStartInfo?.Code}, _appStartInfo?.Message";
					Stop();
					return;
				}

				_gameId = _appStartInfo?.Data?.GameInfo?.GameId;
				if (_gameId != null)
				{
					_logger.LogDebug("[OpenBLiveProvider] | [Start] | 成功开启，开始心跳，游戏ID: " + _gameId);
					_status = $"Success";
					_cancellationToken = new CancellationTokenSource();
					_heatbeat = new InteractivePlayHeartBeat(_gameId, 20000, _cancellationToken);
					_heatbeat.HeartBeatError += M_PlayHeartBeat_HeartBeatError;
					_heatbeat.HeartBeatSucceed += M_PlayHeartBeat_HeartBeatSucceed;
					_heatbeat.Start();
				}
				else
				{
					_logger.LogCritical("[OpenBLiveProvider] | [Start] | 开启游戏错误: " + _appStartInfo!.ToString());
					_status = $"Connecting Error: {_appStartInfo!.ToString()}";
					Stop();
					return;
				}
			});

			_cancellationToken = new CancellationTokenSource();
		}

		public void Stop(bool force = false)
		{
			_logger.LogInformation("[OpenBLiveProvider] | [Stop] | Stop");
			if (_cancellationToken is null)
			{
				return;
			}

			if (_bApiClient == null)
			{
				return;
			}

			Task.Run(async () =>
			{
				var ret = await _bApiClient!.EndInteractivePlay(_appId!, _gameId!);
				_logger.LogDebug("[OpenBLiveProvider] | [Stop] | 关闭游戏: " + ret.Message);
				_status = $"Stopped";

				_cancellationToken.Cancel();
				_bApiClient = null;
				_gameId = null;
				_appStartInfo = null;
				_appId = null;
				_heatbeat = null;
				_logger.LogInformation("[OpenBLiveProvider] | [Stop] | Stopped");
				if (force)
				{
					_reconnectCancellationToken.Cancel();
					_deamon = null;
				}
			});
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

		public void Dispose()
		{
			if (_appStartInfo != null)
			{
				Stop();
			}
			_logger.LogInformation("[OpenBLiveProvider] | [Dispose] | Disposed");
		}

		public IOpenBLiveProvider GetBLiveService()
		{
			return _openBLiveService!;
		}

		private void M_PlayHeartBeat_HeartBeatSucceed()
		{
			// Logger.Log("心跳成功");
		}

		private void M_PlayHeartBeat_HeartBeatError(string json)
		{
			var data = JSON.Parse(json);
			_logger.LogCritical("[OpenBLiveProvider] | [M_PlayHeartBeat_HeartBeatError] | 心跳失败" + data["message"]);
			_status = $"Heartbeat failed: {data["message"]}";
			Stop();
		}

		private static void WebSocketBLiveClientOnSuperChat(SuperChat superChat)
		{
			var sb = new StringBuilder("收到SC!");
			sb.AppendLine();
			sb.Append("来自用户：");
			sb.AppendLine(superChat.userName);
			sb.Append("留言内容：");
			sb.AppendLine(superChat.message);
			sb.Append("金额：");
			sb.Append(superChat.rmb);
			sb.Append("元");
			Logger.Log(sb.ToString());
		}

		private static void WebSocketBLiveClientOnGuardBuy(Guard guard)
		{
			var sb = new StringBuilder("收到大航海!");
			sb.AppendLine();
			sb.Append("来自用户：");
			sb.AppendLine(guard.userInfo.userName);
			sb.Append("赠送了");
			sb.Append(guard.guardUnit);
			Logger.Log(sb.ToString());
		}

		private static void WebSocketBLiveClientOnGift(SendGift sendGift)
		{
			var sb = new StringBuilder("收到礼物!");
			sb.AppendLine();
			sb.Append("来自用户：");
			sb.AppendLine(sendGift.userName);
			sb.Append("赠送了");
			sb.Append(sendGift.giftNum);
			sb.Append("个");
			sb.Append(sendGift.giftName);
			Logger.Log(sb.ToString());
		}

		private static void WebSocketBLiveClientOnDanmaku(Dm dm)
		{
			var sb = new StringBuilder("收到弹幕!");
			sb.AppendLine();
			sb.Append("用户：");
			sb.AppendLine(dm.userName);
			sb.Append("弹幕内容：");
			sb.Append(dm.msg);
			Logger.Log(sb.ToString());
		}
	}
}