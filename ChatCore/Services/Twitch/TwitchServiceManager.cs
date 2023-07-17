using System;
using System.Collections.Generic;
using System.Reflection;
using ChatCore.Interfaces;
using Microsoft.Extensions.Logging;

namespace ChatCore.Services.Twitch
{
	public class TwitchServiceManager : IChatServiceManager, IDisposable
	{
		private readonly ILogger _logger;
		private readonly TwitchService _twitchService;
		private readonly object _lock = new object();

		public bool IsRunning { get; private set; }
		public HashSet<Assembly> RegisteredAssemblies => new HashSet<Assembly>();

		public TwitchServiceManager(ILogger<TwitchServiceManager> logger, TwitchService twitchService)
		{
			_logger = logger;
			_twitchService = twitchService;
		}

		public void Start(Assembly assembly)
		{
			lock (_lock)
			{
				RegisteredAssemblies.Add(assembly);
				if (IsRunning)
				{
					return;
				}

				_twitchService.Start();
				IsRunning = true;

				_logger.LogInformation("[TwitchServiceManager] | [Start] | Started");
			}
		}

		public void Stop(Assembly assembly)
		{
			lock (_lock)
			{
				if (!IsRunning)
				{
					return;
				}

				if (assembly != null)
				{
					RegisteredAssemblies.Remove(assembly);
					if (RegisteredAssemblies.Count > 0)
					{
						return;
					}
				}

				_twitchService.Stop();
				IsRunning = false;

				_logger.LogInformation("[TwitchServiceManager] | [Stop] | Stopped");
			}
		}

		public void Dispose()
		{
			if (IsRunning)
			{
				Stop(null!);
			}

			_logger.LogInformation("[TwitchServiceManager] | [Dispose] | Disposed");
		}

		public IChatService GetService()
		{
			// ReSharper disable once InconsistentlySynchronizedField
			return _twitchService;
		}
	}
}
