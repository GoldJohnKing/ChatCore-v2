using System;
using System.Reflection;
using WebSocket4Net;

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

        void Connect(string uri, bool forceReconnect = false);
        void Disconnect();
        void SendMessage(string message);
		void SendMessage(byte[] bytes);
	}
}
