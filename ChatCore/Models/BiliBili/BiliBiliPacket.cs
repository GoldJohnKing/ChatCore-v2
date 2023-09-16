using System;
using System.Linq;
using System.Text;
using ChatCore.Utilities;

namespace ChatCore.Models.Bilibili
{
	public class BilibiliPacket
	{
		public const int HEADERLENGTH = 16;
		public const int PACKETOFFSET = 0;
		public const int HEADEROFFSET = 4;
		public const int VERSIONOFFSET = 6;
		public const int OPERATIONOFFSET = 8;
		public const int SEQUENCEOFFSET = 12;

		public byte[] PacketBuffer { get; private set; }

		private byte[] Encoder(string value, DanmakuOperation operation)
		{
			var data = Encoding.UTF8.GetBytes(value);
			var packetLen = 16 + data.Length;
			var header = new byte[] { 0, 0, 0, 0, 0, 16, 0, 1, 0, 0, 0, (byte)operation, 0, 0, 0, 1 };
			WriteInt(header, 0, 4, packetLen);
			return header.Concat(data).ToArray();
		}

		private byte[] Decoder(string value, DanmakuOperation operation)
		{
			return new byte[0];
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
		/// <param name="operation"></param>
		/// <param name="json"></param>
		private BilibiliPacket(DanmakuOperation operation, JSONObject json)
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
			PacketBuffer = Encoder(json.ToString(), operation);
		}

		private BilibiliPacket(DanmakuOperation operation, string json)
		{
			PacketBuffer = Encoder(json, operation);
		}

		/// <summary>
		/// Create greeting packet.
		/// </summary>
		/// <param name="uid"></param>
		/// <param name="roomId"></param>
		/// <returns></returns>
		public static BilibiliPacket CreateGreetingPacket(int uid, int roomId)
		{
			var json = new JSONObject();
			//json["clientver"] = "1.6.3";
			//json["platform"] = "web";
			//json["protover"] = new JSONNumber(3);
			json["roomid"] = new JSONNumber(roomId);
			json["uid"] = new JSONNumber(uid);
			//json["type"] = new JSONNumber(2);

			return new BilibiliPacket(DanmakuOperation.GreetingReq, json);
		}

		/// <summary>
		/// Create greeting packet.
		/// </summary>
		/// <param name="uid"></param>
		/// <param name="roomId"></param>
		/// <param name="token"></param>
		/// <param name="buvid"></param>
		/// <returns></returns>
		public static BilibiliPacket CreateGreetingPacket(int uid, int roomId, string token, string buvid)
		{
			var json = new JSONObject();
			//json["clientver"] = "1.6.3";
			json["platform"] = "web";
			json["protover"] = new JSONNumber(3);
			json["roomid"] = new JSONNumber(roomId);
			json["uid"] = new JSONNumber(uid);
			json["key"] = new JSONString(token);
			json["buvid"] = new JSONString(buvid);
			json["type"] = new JSONNumber(2);

			return new BilibiliPacket(DanmakuOperation.GreetingReq, json);
		}

		public static BilibiliPacket CreateAuthPacket(string authBody)
		{
			return new BilibiliPacket(DanmakuOperation.GreetingReq, authBody);
		}

		/// <summary>
		/// Create HeartBeat Packet..
		/// </summary>
		/// <returns></returns>
		public static BilibiliPacket CreateHeartBeatPacket()
		{
			return new BilibiliPacket(DanmakuOperation.HeartBeatReq, "");
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
			GreetingAck = 8,

			// Room ids stops live message from Server
			StopRoom = 1398034256
		}

		public static string ByteArrayToString(byte[] ba)
		{
			var hex = new StringBuilder(ba.Length * 2);
			foreach (var b in ba)
			{
				hex.AppendFormat("{0:x2}", b);
			}

			return hex.ToString();
		}
	}
}
