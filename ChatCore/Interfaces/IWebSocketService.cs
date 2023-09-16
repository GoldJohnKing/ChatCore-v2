using System;
using System.Collections.Generic;
using System.Reflection;

namespace ChatCore.Interfaces
{
	public interface IWebSocketService : IDisposable
	{
		bool IsConnected { get; }
		bool AutoReconnect { get; set; }
		int ReconnectDelay { get; set; }

		event Action OnOpen;
		event Action OnClose;
		event Action OnError;
		event Action<Assembly, string> OnMessageReceived;
		event Action<Assembly, byte[]> OnDataRecevied;

		void Connect(string uri, bool forceReconnect = false, string userAgent = "", string origin = "", List<KeyValuePair<string, string>>? cookies = null);
		void Disconnect();
		void SendMessage(string message);
		void SendMessage(byte[] bytes);
	}
}
