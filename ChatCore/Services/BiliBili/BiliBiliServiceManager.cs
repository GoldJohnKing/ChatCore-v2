using System;
using System.Collections.Generic;
using System.Reflection;
using ChatCore.Interfaces;
using Microsoft.Extensions.Logging;

namespace ChatCore.Services.BiliBili
{
	public class BiliBiliServiceManager : IChatServiceManager, IDisposable
	{
		private readonly ILogger _logger;
		private readonly BiliBiliService _bilibiliService;
		private static readonly object _lock = new object();

		public bool IsRunning { get; private set; }
		public HashSet<Assembly> RegisteredAssemblies => new HashSet<Assembly>();

		public BiliBiliServiceManager(ILogger<BiliBiliServiceManager> logger, BiliBiliService bilibiliService)
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

				_logger.LogInformation("Started");
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

				_logger.LogInformation("Stopped");
			}
		}

		public void Dispose()
		{
			if (IsRunning)
			{
				Stop(null!);
			}

			_logger.LogInformation("Disposed");
		}

		public IChatService GetService()
		{
			// ReSharper disable once InconsistentlySynchronizedField
			return _bilibiliService;
		}
	}
}
