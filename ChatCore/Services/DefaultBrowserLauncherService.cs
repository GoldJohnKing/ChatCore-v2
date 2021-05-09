using System.Diagnostics;
using ChatCore.Interfaces;

namespace ChatCore.Services
{
	public class ProcessDotStartBrowserLauncherService : IDefaultBrowserLauncherService
	{
		public void Launch(string uri)
		{
			Process.Start(uri);
		}
	}
}
