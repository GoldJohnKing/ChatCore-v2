using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Reflection;
using System.Runtime;
using ChatCore.Config;
using ChatCore.Exceptions;
using ChatCore.Interfaces;
using ChatCore.Logging;
using ChatCore.Services;
using ChatCore.Services.Bilibili;
using ChatCore.Services.Twitch;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ChatCore
{
	public class ChatCoreInstance
	{
		private static readonly object CreateLock = new object();

		private readonly object _runLock = new object();

		private static ChatCoreInstance? _instance;
		private static ServiceProvider? _serviceProvider;
		private static Version? _version;
		private static ChatServiceMultiplexer? _chatServiceMultiplexer;
		private static OpenBLiveProvider? _openBLiveProvider;
		private static MainSettingsProvider? _settings;
		private static ILogger? _logger;

		private ChatCoreInstance() { }

		internal static Version Version => _version ??= typeof(ChatCoreInstance).Assembly.GetName().Version;

		public event Action<CustomLogLevel, string, string>? OnLogReceived;


		internal void OnLogReceivedInternal(CustomLogLevel level, string category, string message)
		{
			OnLogReceived?.Invoke(level, category, message);
		}

		// Method is kept to keep binary compatibility
		public static ChatCoreInstance Create()
		{
			return Create(null);
		}

		// ReSharper disable once MethodOverloadWithOptionalParameter
		public static ChatCoreInstance Create(Action<CustomLogLevel, string, string>? logReceiver = null!)
		{
			lock (CreateLock)
			{
				if (_instance != null)
				{
					return _instance;
				}

				_instance = new ChatCoreInstance();
				var serviceCollection = new ServiceCollection();
				serviceCollection
					.AddLogging(builder =>
					{
#if DEBUG
						builder.AddConsole();
#endif
						builder.AddProvider(new CustomSinkProvider(_instance));
					})
					.AddSingleton<Random>()
					.AddSingleton<HttpClient>()
					.AddSingleton<ObjectSerializer>()
					.AddSingleton<MainSettingsProvider>()
					.AddSingleton<BilibiliService>()
					.AddSingleton<BilibiliServiceManager>()
					.AddSingleton<IPathProvider, PathProvider>()
					.AddSingleton<IUserAuthProvider, UserAuthProvider>()
					.AddSingleton<IWebLoginProvider, WebLoginProvider>()
					.AddSingleton<IEmojiParser, FrwTwemojiParser>()
					.AddSingleton<IDefaultBrowserLauncherService, ProcessDotStartBrowserLauncherService>()
					.AddTransient<IWebSocketService, WebSocket4NetServiceProvider>()
					.AddSingleton<IWebSocketServerService, WebSocketServerProvider>()
					.AddSingleton<IOpenBLiveProvider, OpenBLiveProvider>()
					.AddSingleton<TwitchService>()
					.AddSingleton<TwitchServiceManager>()
					.AddSingleton<TwitchMessageParser>()
					.AddSingleton<TwitchDataProvider>()
					.AddSingleton<TwitchCheermoteProvider>()
					.AddSingleton<TwitchBadgeProvider>()
					.AddSingleton<BTTVDataProvider>()
					.AddSingleton<FFZDataProvider>()
					.AddSingleton<IChatService>(x =>
						new ChatServiceMultiplexer(
							x.GetService<ILogger<ChatServiceMultiplexer>>(),
							new List<IChatService>
							{
								x.GetService<TwitchService>(),
								x.GetService<BilibiliService>()
							},
							x.GetService<IOpenBLiveProvider>(),
							false, false
						)
					)
					.AddSingleton<IChatServiceManager>(x =>
					new ChatServiceManager(
						x.GetService<ILogger<ChatServiceManager>>(),
						x.GetService<IChatService>(),
						new List<IChatServiceManager>
						{
							x.GetService<TwitchServiceManager>(),
							x.GetService<BilibiliServiceManager>()
						}
					)
					);

				if (logReceiver != null)
				{
					_instance.OnLogReceived += logReceiver;
				}

				_serviceProvider = serviceCollection.BuildServiceProvider();

				_logger = _serviceProvider.GetService<ILogger<ChatCoreInstance>>();
				_settings = _serviceProvider.GetService<MainSettingsProvider>();
				_settings.onTwitchUpdate += onEnableTwitch;
				_settings.onBilibiliUpdate += onEnableBilibili;
				if (_settings.DisableWebApp)
				{
					_logger.Log(LogLevel.Information, "[ChatCoreInstance] | [Create(...)] | WebLoginProvider disabled...");
					return _instance;
				}

				_logger.Log(LogLevel.Information, "[ChatCoreInstance] | [Create(...)] | Attempting to start WebLoginProvider");
				_serviceProvider.GetService<IWebLoginProvider>().Start();
				_logger.Log(LogLevel.Information, "[ChatCoreInstance] | [Create(...)] | Attempting to start WebSocketServerService");
				_serviceProvider.GetService<IWebSocketServerService>().Start(System.Net.IPAddress.Any, MainSettingsProvider.WEB_APP_PORT == 65535 ? 65534 : (MainSettingsProvider.WEB_APP_PORT + 1));
				_logger.Log(LogLevel.Information, "[ChatCoreInstance] | [Create(...)] | Supposedly started WebLoginProvider");
				if (_settings.LaunchWebAppOnStartup)
				{
					_serviceProvider.GetService<IDefaultBrowserLauncherService>().Launch($"http://localhost:{MainSettingsProvider.WEB_APP_PORT}");
				}

				return _instance;
			}
		}

		/// <summary>
		/// Starts all services if they haven't been already.
		/// </summary>
		/// <returns>A reference to the generic service multiplexer</returns>
		public ChatServiceMultiplexer RunAllServices()
		{
			lock (_runLock)
			{
				if (_serviceProvider == null)
				{
					throw new ChatCoreNotInitializedException("Make sure to call ChatCoreInstance.Create() to initialize ChatCore!");
				}

				var services = _serviceProvider.GetService<IChatServiceManager>();
				services.Start(Assembly.GetCallingAssembly());
				_chatServiceMultiplexer = (ChatServiceMultiplexer)services.GetService();
				if (_settings!.EnableTwitch)
				{
					_logger.Log(LogLevel.Information, "[ChatCoreInstance] | [RunAllServices] | Twitch Enabled");
					onEnableTwitch(true);
				}

				if (_settings!.EnableBilibili)
				{
					_logger.Log(LogLevel.Information, "[ChatCoreInstance] | [RunAllServices] | Bilibili Enabled");
					onEnableBilibili(true);
				}
				return _chatServiceMultiplexer;
			}
		}

		/// <summary>
		/// Stops all services as long as no references remain. Make sure to unregister any callbacks first!
		/// </summary>
		public void StopAllServices()
		{
			lock (_runLock)
			{
				lock (_runLock)
				{
					try
					{
						var x = _serviceProvider.GetService<IChatServiceManager>().GetService();
						_logger.Log(LogLevel.Information, $"[ChatCoreInstance] | [StopAllServices] | {x.DisplayName} stopped.");
						_serviceProvider.GetService<IChatServiceManager>().Stop(Assembly.GetCallingAssembly());
					} catch (Exception ex)
					{
						_logger.Log(LogLevel.Information, $"[ChatCoreInstance] | [StopAllServices] | Exception: {ex}");
					}
				}
			}
		}

		/// <summary>
		/// Starts the Twitch services if they haven't been already.
		/// </summary>
		/// <returns>A reference to the Twitch service</returns>
		public TwitchService RunTwitchServices()
		{
			lock (_runLock)
			{
				if (_serviceProvider == null)
				{
					throw new ChatCoreNotInitializedException("Make sure to call ChatCoreInstance.Create() to initialize ChatCore!");
				}

				var twitch = _serviceProvider.GetService<TwitchServiceManager>();
				twitch.Start(Assembly.GetCallingAssembly());
				return (TwitchService)twitch.GetService();
			}
		}

		/// <summary>
		/// Stops the Twitch services as long as no references remain. Make sure to unregister any callbacks first!
		/// </summary>
		public void StopTwitchServices()
		{
			lock (_runLock)
			{
				_serviceProvider.GetService<TwitchServiceManager>().Stop(Assembly.GetCallingAssembly());
			}
		}

		/// <summary>
		/// Starts the Twitch services if they haven't been already.
		/// </summary>
		/// <returns>A reference to the Twitch service</returns>
		public BilibiliService RunBilibiliServices()
		{
			lock (_runLock)
			{
				if (_serviceProvider == null)
				{
					throw new ChatCoreNotInitializedException("Make sure to call ChatCoreInstance.Create() to initialize ChatCore!");
				}

				var bilibili = _serviceProvider.GetService<BilibiliServiceManager>();
				bilibili.Start(Assembly.GetCallingAssembly());
				return (BilibiliService)bilibili.GetService();
			}
		}

		public OpenBLiveProvider RunBLiveServices()
		{
			lock (_runLock)
			{
				if (_serviceProvider == null)
				{
					throw new ChatCoreNotInitializedException("Make sure to call ChatCoreInstance.Create() to initialize ChatCore!");
				}

				_openBLiveProvider = (OpenBLiveProvider)_serviceProvider.GetService<IOpenBLiveProvider>();
				_openBLiveProvider.Start();
				return _openBLiveProvider;
			}
		}

		public void StopBLiveServices()
		{
			lock (_runLock)
			{
				if (_openBLiveProvider != null)
				{
					_openBLiveProvider.Stop();
				}
			}
		}

		/// <summary>
		/// Stops the Twitch services as long as no references remain. Make sure to unregister any callbacks first!
		/// </summary>
		public void StopBilibiliServices()
		{
			lock (_runLock)
			{
				_serviceProvider.GetService<BilibiliServiceManager>().Stop(Assembly.GetCallingAssembly());
			}
		}

		/// <summary>
		/// Launches the settings WebApp in the users default browser.
		/// </summary>
		public void LaunchWebApp()
		{
			lock (_runLock)
			{
				if (_serviceProvider == null)
				{
					throw new ChatCoreNotInitializedException("Make sure to call ChatCoreInstance.Create() to initialize ChatCore!");
				}

				_serviceProvider.GetService<IDefaultBrowserLauncherService>().Launch($"http://localhost:{MainSettingsProvider.WEB_APP_PORT}");
			}
		}

		public void LaunchWebSocketServer()
		{
			lock (_runLock)
			{
				if (_serviceProvider == null)
				{
					throw new ChatCoreNotInitializedException("Make sure to call ChatCoreInstance.Create() to initialize ChatCore!");
				}

				_serviceProvider.GetService<IWebSocketServerService>().Start(System.Net.IPAddress.Any, MainSettingsProvider.WEB_APP_PORT == 65535 ? 65534 : (MainSettingsProvider.WEB_APP_PORT + 1));
			}
		}

		public static Dictionary<string, bool> BilibiliDisplaySettings()
		{
			var result = new Dictionary<string, bool>();
			if (_serviceProvider != null)
			{
				var _settings = _serviceProvider.GetService<MainSettingsProvider>();
				result.Add("showAvatar", _settings.danmuku_avatar);
				result.Add("showGuard", _settings.danmuku_guard_prefix);
				result.Add("showGuardText", _settings.danmuku_guard_prefix_type);
				result.Add("showBadge", _settings.danmuku_badge_prefix);
				result.Add("showBadgeText", _settings.danmuku_badge_prefix_type);
				result.Add("showHonorBadge", _settings.danmuku_honor_badge_prefix);
				result.Add("showHonorBadgeText", _settings.danmuku_honor_badge_prefix_type);
				result.Add("showBroadcaster", _settings.danmuku_broadcaster_prefix);
				result.Add("showBroadcasterText", _settings.danmuku_broadcaster_prefix_type);
				result.Add("showModerator", _settings.danmuku_moderator_prefix);
				result.Add("showModeratorText", _settings.danmuku_moderator_prefix_type);
			}

			return result;
		}

		private static void onEnableTwitch(bool enable)
		{
			_logger.LogInformation($"[ChatCoreInstance] | [onEnableTwitch] | {enable.ToString()}");
			if (_chatServiceMultiplexer == null)
			{
				throw new ChatCoreNotInitializedException("Make sure to call ChatCoreInstance.RunAllServices() to initialize Services!");
			}

			if (enable)
			{
				_chatServiceMultiplexer.EnableTwitchService();
			}
			else
			{
				_chatServiceMultiplexer.DisableTwitchService();
			}
		}

		private static void onEnableBilibili(bool enable)
		{
			_logger.LogInformation($"[ChatCoreInstance] | [onEnableBilibili] | {enable.ToString()}");
			if (_chatServiceMultiplexer == null)
			{
				throw new ChatCoreNotInitializedException("Make sure to call ChatCoreInstance.RunAllServices() to initialize Services!");
			}

			if (enable)
			{
				_chatServiceMultiplexer.EnableBilibiliService();
				((OpenBLiveProvider)_serviceProvider.GetService<IOpenBLiveProvider>()).Enable();
			}
			else
			{
				_chatServiceMultiplexer.DisableBilibiliService();
				((OpenBLiveProvider)_serviceProvider.GetService<IOpenBLiveProvider>()).Disable();
			}
		}
	}
}
