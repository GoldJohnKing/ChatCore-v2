using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace ChatCore.Interfaces
{
	public interface IWebSocketServerService
	{
		void Start(IPAddress address, int port);

		void Stop();

		void SendMessage(string message, List<string>? channels = null);
		void SendMessage(byte[] bytes, List<string>? channels = null);
	}
}
