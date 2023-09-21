using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using WebSocketSharp;
using WebSocketSharp.Server;
using System.Timers;
using ChatCore.Models.Bilibili;
using ChatCore.Utilities;
using System.Security.Cryptography;

namespace ChatCore.Models
{
	public class WebSocketServerBehavior : WebSocketBehavior
	{
		public readonly List<string> avaliable_channels = new List<string>() { "twitch", "twitch_raw", "bilibili", "bilibili_raw" };
		private List<string> _subscribe_channels = new List<string>();
		private Timer initTimer = new Timer();
		private Timer heartBeatTimer = new Timer();
		public Action? RemoveAllListener;
		public WebSocketServerBehavior()
		{
			setTimer();
		}

		public WebSocketServerBehavior(Action<string, List<string>>? OnMessageReceived, Action<byte[], List<string>>? OnDataRecevied)
		{
			OnMessageReceived += SendMessage;
			OnDataRecevied += SendMessage;
			setTimer();
		}

		private void setTimer()
		{
			initTimer.Interval = 5000;
			initTimer.AutoReset = false;
			heartBeatTimer.Interval = 60000;
			heartBeatTimer.AutoReset = false;
			initTimer.Elapsed += (sender, e) =>
			{
				if (_subscribe_channels.Count == 0)
				{
					Console.WriteLine("STOP by initTimer");
					Stop();
				}
				else
				{
					initTimer.Dispose();
				}
			};
			heartBeatTimer.Elapsed += heartBeatTimeout;
		}

		public bool containsChannel(string channel_name)
		{
			return _subscribe_channels.Contains(channel_name);
		}

		protected override void OnMessage(MessageEventArgs e)
		{
			try
			{
				var data = JSON.Parse(e.Data);
				var json = new JSONObject();
				if (data.HasKey("cmd"))
				{
					switch (data["cmd"].Value.ToString())
					{
						case "sub":
							if (data.HasKey("channels"))
							{
								var request_channels = data["channels"].AsArray;
								if (request_channels != null)
								{
									for (var i = 0; i < request_channels.Count; i++)
									{
										var channel_name = request_channels[i].Value;
										if (avaliable_channels.Contains(channel_name) && !_subscribe_channels.Contains(channel_name))
										{
											_subscribe_channels.Add(channel_name);
										}
									}
								}
							}
							json["cmd"] = new JSONString("sub");
							json["channels"] = new JSONArray(_subscribe_channels.ToArray());
							Send(json.ToString());
							if (initTimer != null && _subscribe_channels.Count > 0)
							{
								initTimer.Dispose();
								heartBeatTimer.Start();
							}
							break;
						case "unsub":
							if (data.HasKey("channels"))
							{
								var request_channels = data["channels"].AsArray;
								if (request_channels != null)
								{
									for (var i = 0; i < request_channels.Count; i++)
									{
										var channel_name = request_channels[i].Value;
										if (avaliable_channels.Contains(channel_name) && _subscribe_channels.Contains(channel_name))
										{
											_subscribe_channels.Remove(channel_name);
										}
									}
								}
							}
							json["cmd"] = new JSONString("unsub");
							json["channels"] = new JSONArray(_subscribe_channels.ToArray());
							Send(json.ToString());
							break;
						case "ping":
							restartTimer();
							json["cmd"] = new JSONString("pong");
							Send(json.ToString());
							break;
						case "disconnect":
							json["cmd"] = new JSONString("disconnected");
							Send(json.ToString());
							Console.WriteLine("STOP by client");
							Stop();
							break;
					}
				}
			}
			catch { }
		}

		protected override void OnOpen()
		{
			initTimer.Start();
		}

		protected override void OnClose(CloseEventArgs e)
		{
			Stop();
		}

		private void restartTimer()
		{
			if (heartBeatTimer != null && heartBeatTimer.Enabled)
			{
				heartBeatTimer.Stop();
				heartBeatTimer.Elapsed -= heartBeatTimeout;
				heartBeatTimer.Enabled = false;
				heartBeatTimer.Dispose();
				//GC.Collect();
				heartBeatTimer = new Timer();
				heartBeatTimer.Interval = 60000;
				heartBeatTimer.AutoReset = false;
				heartBeatTimer.Elapsed += heartBeatTimeout;
				heartBeatTimer.Start();
			}
		}

		private void heartBeatTimeout(object sender, ElapsedEventArgs e) {
			if (sender.GetHashCode() == heartBeatTimer.GetHashCode())
			{
				Console.WriteLine($"STOP by heartBeatTimer");
			}
			Stop();
		}

		private void Stop()
		{
			if (initTimer != null)
			{
				initTimer.Stop();
				initTimer.Dispose();
			}

			if (heartBeatTimer != null)
			{
				heartBeatTimer.Stop();
				heartBeatTimer.Elapsed -= heartBeatTimeout;
				heartBeatTimer.Enabled = false;
				heartBeatTimer.Dispose();
			}
			RemoveAllListener?.Invoke();
			Sessions.CloseSession(ID);
		}

		public void SendMessage(string message, List<string> channels)
		{
			if (_subscribe_channels.Count > 0)
			{
				foreach (var channel_name in channels)
				{
					if (_subscribe_channels.Contains(channel_name))
					{
						var json = new JSONObject();
						json["cmd"] = new JSONString("data");
						json["channel"] = new JSONString(channel_name);
						json["data"] = new JSONString(message);
						Send(json.ToString());
					}
				}
			}
		}

		public void SendMessage(byte[] bytes, List<string> channels)
		{
			if (_subscribe_channels.Count > 0)
			{
				foreach (var channel_name in channels)
				{
					if (_subscribe_channels.Contains(channel_name))
					{
						Send(bytes);
					}
				}
			}
		}

		protected override void OnError(ErrorEventArgs e)
		{
			
		}
	}
}
