using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ChatCore.Interfaces;
using ChatCore.Utilities;
using Microsoft.Extensions.Logging;

namespace ChatCore.Services
{
	public class WebLoginProvider : IWebLoginProvider
	{
		private readonly ILogger _logger;
		private readonly IUserAuthProvider _authManager;
		private readonly MainSettingsProvider _settings;

		private HttpListener? _listener;
		private CancellationTokenSource? _cancellationToken;
		private static string? _pageData;

		private readonly SemaphoreSlim _requestLock = new SemaphoreSlim(1, 1);

		public WebLoginProvider(ILogger<WebLoginProvider> logger, IUserAuthProvider authManager, MainSettingsProvider settings)
		{
			_logger = logger;
			_authManager = authManager;
			_settings = settings;
		}

		public void Start()
		{
			if (_pageData == null)
			{
				using var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("ChatCore.Resources.Web.index.html")!);
				_pageData = reader.ReadToEnd();
			}

			if (_listener != null)
			{
				return;
			}

			_cancellationToken = new CancellationTokenSource();
			_listener = new HttpListener { Prefixes = { $"http://localhost:{MainSettingsProvider.WEB_APP_PORT}/" } };
			_listener.Start();

			_logger.Log(LogLevel.Information, $"Listening on {string.Join(", ", _listener.Prefixes)}");

			Task.Run(async () =>
			{
				while (true)
				{
					try
					{
						_logger.LogInformation("Waiting for incoming request...");
						var httpListenerContext = await _listener.GetContextAsync().ConfigureAwait(false);
						_logger.LogWarning("Request received");
						await OnContext(httpListenerContext).ConfigureAwait(false);
					}
					catch (Exception e)
					{
						_logger.LogError(e, "WebLoginProvider errored.");
					}
				}

				// ReSharper disable once FunctionNeverReturns
			}).ConfigureAwait(false);
		}

		private async Task OnContext(HttpListenerContext ctx)
		{
			string[] resource_file_list = { "/Statics/Css/default.css", "/Statics/Css/Material+Icons.css", "/Statics/Css/materialize.min.css", "/Statics/Fonts/flUhRq6tzZclQEJ-Vdg-IuiaDsNc.woff2", "/Statics/Js/default.js", "/Statics/Js/materialize.min.js", "/Statics/Js/jquery-3.5.1.min.js", "/Statics/Lang/en.json", "/Statics/Lang/zh.json", "/Statics/Lang/ja.json" };
			await _requestLock.WaitAsync();
			try
			{
				var request = ctx.Request;
				var response = ctx.Response;

				if (request.HttpMethod == "POST" && request.Url.AbsolutePath == "/submit")
				{
					await Submit(request, response).ConfigureAwait(false);
				} else if (request.HttpMethod == "GET" && Array.IndexOf(resource_file_list, request.Url.AbsolutePath) > -1)
				{
					// Load resources
					response.StatusCode = 200;
					var Ext = Path.GetExtension(request.Url.AbsolutePath);
					if (Ext == ".html")
					{
						response.ContentType = "text/html";
					}
					else if (Ext == ".css")
					{
						response.ContentType = "text/css";
					}
					else if (Ext == ".js")
					{
						response.ContentType = "application/javascript";
					}
					else if (Ext == ".json")
					{
						response.ContentType = "application/json";
					}
					else if (Ext == ".woff2")
					{
						response.ContentType = "font/woff2";
					}

					Console.WriteLine("Trying to get resource: " + "ChatCore.Resources.Web" + request.Url.AbsolutePath.Replace("/", "."));
					var buffer = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("ChatCore.Resources.Web" + request.Url.AbsolutePath.Replace("/", "."))!);
					buffer.BaseStream.CopyTo(response.OutputStream);
				}
				else
				{
					var settingsJson = _settings.GetSettingsAsJson();
					settingsJson["twitch_oauth_token"] = new JSONString(_authManager.Credentials.Twitch_OAuthToken);
					settingsJson["twitch_channels"] = new JSONArray(_authManager.Credentials.Twitch_Channels);

					var pageBuilder = new StringBuilder(_pageData);
					pageBuilder.Replace("{libVersion}", ChatCoreInstance.Version.ToString(3));
					pageBuilder.Replace("var data = {};", $"var data = {settingsJson};");

					var data = Encoding.UTF8.GetBytes(pageBuilder.ToString());
					response.ContentType = "text/html";
					response.ContentEncoding = Encoding.UTF8;
					response.ContentLength64 = data.LongLength;
					await response.OutputStream.WriteAsync(data, 0, data.Length);
				}

				response.Close();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Exception occurred during webapp request.");
			}
			finally
			{
				_requestLock.Release();
			}
		}

		private async Task Submit(HttpListenerRequest request, HttpListenerResponse response)
		{
			try
			{
				using var reader = new StreamReader(request.InputStream, request.ContentEncoding);
				var postStr = await reader.ReadToEndAsync().ConfigureAwait(false);

				var responseJson = JSON.Parse(postStr) as JSONObject;
				if (responseJson == null)
				{
					return;
				}

				var authChanged = false;
				if (responseJson.HasKey("twitch_oauth_token"))
				{
					var token = responseJson["twitch_oauth_token"].Value;
					if (!token.StartsWith("oauth:"))
					{
						token = !string.IsNullOrWhiteSpace(token) ? $"oauth:{token}" : string.Empty;
					}

					if (_authManager.Credentials.Twitch_OAuthToken != token)
					{
						_authManager.Credentials.Twitch_OAuthToken = token;
						authChanged = true;
					}

					responseJson.Remove("twitch_oauth_token");
				}

				if (responseJson.HasKey("twitch_channels"))
				{
					var channelsFromResponse = responseJson["twitch_channels"].AsArray?.Children.Select(channelName => channelName.Value).ToList();
					if (channelsFromResponse != null &&
						(channelsFromResponse.Count != _authManager.Credentials.Twitch_Channels.Count ||
						channelsFromResponse.Any(name => !_authManager.Credentials.Twitch_Channels.Contains(name))))
					{
						_authManager.Credentials.Twitch_Channels.Clear();
						_authManager.Credentials.Twitch_Channels.AddRange(channelsFromResponse);

						authChanged = true;
					}

					responseJson.Remove("twitch_channels");
				}

				if (authChanged)
				{
					_authManager.Save();
				}
				_settings.SetFromDictionary(responseJson);
				_settings.Save();

				response.StatusCode = 204;
			}
			catch
			{
				response.StatusCode = 500;
			}
		}

		public void Stop()
		{
			if (_cancellationToken is null)
			{
				return;
			}

			_listener?.Stop();
			_cancellationToken.Cancel();
			_logger.LogInformation("Stopped");
		}
	}
}