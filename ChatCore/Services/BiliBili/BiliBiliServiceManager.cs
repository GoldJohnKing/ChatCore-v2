using System;
using System.Collections.Generic;
using System.Reflection;
using ChatCore.Interfaces;
using Microsoft.Extensions.Logging;

namespace ChatCore.Services.Bilibili
{
	public class BilibiliServiceManager : IChatServiceManager, IDisposable
	{
		private readonly ILogger _logger;
		private readonly BilibiliService _bilibiliService;
		private static readonly object _lock = new object();

		public bool IsRunning { get; private set; }
		public HashSet<Assembly> RegisteredAssemblies => new HashSet<Assembly>();

		public BilibiliServiceManager(ILogger<BilibiliServiceManager> logger, BilibiliService bilibiliService)
		{
			_logger = logger;
			_bilibiliService = bilibiliService;
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

				_bilibiliService.Start();
				IsRunning = true;

				_logger.LogInformation("[BilibiliServiceManager] | [Start] | Started");
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

				_bilibiliService.Stop();
				IsRunning = false;

				_logger.LogInformation("[BilibiliServiceManager] | [Stop] | Stopped");
			}
		}

		public void Dispose()
		{
			if (IsRunning)
			{
				Stop(null!);
			}

			_logger.LogInformation("[BilibiliServiceManager] | [Dispose] | Disposed");
		}

		public IChatService GetService()
		{
			// ReSharper disable once InconsistentlySynchronizedField
			return _bilibiliService;
		}
	}
}
