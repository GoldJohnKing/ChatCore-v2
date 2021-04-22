using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ChatCore.Utilities;

namespace ChatCore.Models.BiliBili
{
	public class BiliBiliPacket
	{
		public const int HeaderLength = 16;
		public const int PacketOffset = 0;
		public const int HeaderOffset = 4;
		public const int VersionOffset = 6;
		public const int OperationOffset = 8;
		public const int SequenceOffset = 12;

		public byte[] PacketBuffer { get; private set; }

		private byte[] Encorde(string value, DanmakuOperation operation)
		{
			var data = Encoding.UTF8.GetBytes(value);
			var packetLen = 16 + data.Length;
			var header = new byte[] { 0, 0, 0, 0, 0, 16, 0, 1, 0, 0, 0, (byte)operation, 0, 0, 0, 1 };
			this.WriteInt(header, 0, 4, packetLen);
			return header.Concat(data).ToArray();
		}

		private byte[] Decorde(string value, DanmakuOperation operation)
		{
			return null;
		}

		private void WriteInt(byte[] buffer, int start, int length, int value)
		{
			for (var i = 0; i + start < length; i++)
			{
				buffer[start + i] = (byte)(value / Math.Pow(256, length - i - 1));
			}
		}

		/// <summary>
		/// Constructor of DanmakuPacket.
		/// </summary>
		/// <param name="version"></param>
		/// <param name="operation"></param>
		/// <param name="sequence"></param>
		/// <param name="body"></param>
		private BiliBiliPacket(DanmakuOperation operation, JSONObject json)
		{
			//var headerBytes = new byte[HeaderLength];
			//var bodyBuffer = Encoding.UTF8.GetBytes(json.ToString());
			//DataView.SetInt32(headerBytes, PacketOffset, HeaderLength + bodyBuffer.Length);
			//DataView.SetInt16(headerBytes, HeaderOffset, HeaderLength);
			//DataView.SetInt16(headerBytes, VersionOffset, version);
			//DataView.SetInt32(headerBytes, OperationOffset, (int)operation);
			//DataView.SetInt32(headerBytes, SequenceOffset, sequence);

			//var packetBuffer = DataView.MergeBytes(new List<byte[]> {
			//	headerBytes, bodyBuffer
			//});
			this.PacketBuffer = this.Encorde(json.ToString(), operation);
		}

		private BiliBiliPacket(DanmakuOperation operation, string json)
		{
			this.PacketBuffer = this.Encorde(json, operation);
		}

		/// <summary>
		/// Create greeting packet.
		/// </summary>
		/// <param name="uid"></param>
		/// <param name="roomId"></param>
		/// <returns></returns>
		public static BiliBiliPacket CreateGreetingPacket(ulong uid, ulong roomId)
		{
			var json = new JSONObject();
			//json["clientver"] = "1.6.3";
			//json["platform"] = "web";
			//json["protover"] = new JSONNumber(1);
			json["roomid"] = new JSONNumber(roomId);
			//json["uid"] = new JSONNumber(uid);
			//json["type"] = new JSONNumber(2);

			return new BiliBiliPacket(DanmakuOperation.GreetingReq, json);
		}

		/// <summary>
		/// Create HeartBeat Packet..
		/// </summary>
		/// <param name="uid"></param>
		/// <param name="roomId"></param>
		/// <returns></returns>
		public static BiliBiliPacket CreateHeartBeatPacket(int uid, int roomId)
		{
			return new BiliBiliPacket(DanmakuOperation.HeartBeatReq, "");
		}

		public enum DanmakuOperation
		{
			// Send HeartBeat packet to server.
			HeartBeatReq = 2,

			// Server has got the HeartBeat packet successfully.
			HeartBeatAck = 3,

			// Chat message from server.
			ChatMessage = 5,

			// Send greeting request to server.
			GreetingReq = 7,

			// Server has got the Greeting packet successfully.
			GreetingAck = 8
		}
	}
}
