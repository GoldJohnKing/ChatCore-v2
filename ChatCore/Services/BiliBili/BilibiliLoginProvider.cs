using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ChatCore.Services.Twitch;
using ChatCore.Utilities;
using Microsoft.Extensions.Logging;
using System.Timers;

namespace ChatCore.Services.Bilibili
{
	public class BilibiliLoginProvider
	{
		private readonly ILogger _logger;
		private readonly MainSettingsProvider _mainSettingsProvider;
		private readonly SemaphoreSlim _loginLock;

		private readonly string BilibiliRequestQRCodeApi = @"https://passport.bilibili.com/x/passport-login/web/qrcode/generate";
		private readonly string BilibiliQRCodeStatusApi = @"https://passport.bilibili.com/x/passport-login/web/qrcode/poll?qrcode_key=";

		private string qr_secret = string.Empty;
		public string qr_url = string.Empty;
		public string status = "ready";
		public string cookie = string.Empty;
		private int retry = 36;

		private System.Timers.Timer heatBeatTimer = new System.Timers.Timer();

		public BilibiliLoginProvider(ILogger<TwitchDataProvider> logger, MainSettingsProvider mainSettingsProvider)
		{
			_logger = logger;
			_mainSettingsProvider = mainSettingsProvider;
			_loginLock = new SemaphoreSlim(1, 1);
		}

		public void Login()
		{
			Task.Run(async () =>
			{
				await _loginLock.WaitAsync();
				try
				{
					await GetQRCodeAsync();
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, $"[BilibiliLoginProvider] | [Login] | An exception occurred while trying to login Bilibili.");
				}
				finally
				{
					_loginLock.Release();
				}
			}).Wait();
		}

		private async Task<bool> GetQRCodeAsync()
		{
			qr_secret = string.Empty;
			qr_url = string.Empty;
			status = "qr_fetch_busy";
			cookie = string.Empty;
			retry = 36;

			setTimer();
			try
			{
				var apiResult = await (new HttpClientUtils()).HttpClient(BilibiliRequestQRCodeApi, HttpMethod.Get, null, null);
				if (apiResult != null && apiResult[0] == "OK")
				{
					var NewQRInfo = JSONNode.Parse(apiResult[1]);
					if (NewQRInfo["code"].AsInt == 0)
					{
						qr_url = NewQRInfo["data"]["url"].Value;
						qr_secret = NewQRInfo["data"]["qrcode_key"].Value;
						status = "qr_fetch_done";
						setTimer();
						return true;
					}
				}
				else
				{
					status = "qr_fetch_failed_api_error";
					_logger.LogInformation($"[BilibiliLoginProvider] | [GetQRCodeAsync] | Get QR Code Info failed. ({(apiResult == null ? "connection failed" : apiResult[0])})");
				}

			}
			catch
			{
				status = "qr_fetch_failed_http_client_error";
				_logger.LogInformation($"[BilibiliLoginProvider] | [GetQRCodeAsync] | Get QR Code Info failed. (Exception)");
			}
			status = "qr_fetch_failed";
			return false;
		}

		private async void GetQRCodeLoginStatusAsync()
		{
			try
			{
				if (qr_secret == string.Empty)
				{
					return;
				}

				try
				{
					var apiResult = await (new HttpClientUtils()).HttpClient($"{BilibiliQRCodeStatusApi}{qr_secret}", HttpMethod.Get, null, null);
					if (apiResult != null && apiResult[0] == "OK")
					{
						var NewLoginInfo = JSONNode.Parse(apiResult[1]);
						if (NewLoginInfo["code"].AsInt == 0 && NewLoginInfo["data"] != null)
						{
							var bilibiliData = NewLoginInfo["data"];
							var code = bilibiliData["code"].AsInt;

							switch (code)
							{
								case 0:
									qr_secret = string.Empty;

									if (apiResult.Count == 3)
									{
										cookie = apiResult[2];
										status = "login_done";
									}
									else
									{
										status = "login_failed_internal_error";
									}
									disposeTimer();
									break;
								case 86038:
									status = "login_failed_expired";
									break;
								case 86090:
									status = "qr_scan_done";
									break;
								case 86101:
									status = "qr_scan_busy";
									break;
							}
						}
					}
					else
					{
						status = "qr_status_failed_api_error";
						_logger.LogInformation($"[BilibiliLoginProvider] | [GetQRCodeLoginStatusAsync] | Get QR code status failed. ({(apiResult == null ? "connection failed" : apiResult[0])})");
					}

				}
				catch
				{
					status = "qr_status_failed_http_client_error";
					_logger.LogInformation($"[BilibiliLoginProvider] | [GetQRCodeLoginStatusAsync] | Get QR code status failed. (Exception)");
				}
			}
			catch (Exception ex)
			{
				status = "status_failed" + ex.Message;
			}
		}

		private void setTimer()
		{
			disposeTimer();
			heatBeatTimer = new System.Timers.Timer();
			heatBeatTimer.Interval = 5000;
			heatBeatTimer.AutoReset = true;
			heatBeatTimer.Elapsed += (sender, e) =>
			{
				if (retry > 0)
				{
					retry--;
					GetQRCodeLoginStatusAsync();
				}
				else
				{
					heatBeatTimer.AutoReset = false;
					disposeTimer();
				}
			};
			heatBeatTimer.Start();
		}

		private void disposeTimer()
		{
			heatBeatTimer?.Stop();
			heatBeatTimer?.Dispose();
		}
	}
}
