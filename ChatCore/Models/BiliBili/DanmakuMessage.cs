using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using static ChatCore.Models.BiliBili.BiliBiliPacket;

namespace ChatCore.Models.BiliBili
{
	public class DanmakuMessage
	{
		public int PacketLength { get; private set; }
		public int HeaderLength { get; private set; }
		public int Version { get; private set; }
		public DanmakuOperation Operation { get; private set; }
		public int Sequence { get; private set; }
		public string Body { get; private set; }
		public byte[] Buffer { get; private set; }

		public static IEnumerable<DanmakuMessage> ParsePackets(byte[] buffer)
		{
			var packetLength = DataView.GetInt32(buffer);
			var headerLength = DataView.GetInt16(buffer, HeaderOffset);
			var version = DataView.GetInt16(buffer, VersionOffset);
			var operation = DataView.GetInt32(buffer, OperationOffset);
			var sequence = DataView.GetInt32(buffer, SequenceOffset);
			var offset = 0;

			if (operation == 5)
			{
				if (version == 2)
				{

					DataView.ByteSlice(ref buffer, headerLength, packetLength);
					byte[] decomp;
					using (var dest = new MemoryStream())
					{
						using (var ds = new DeflateStream(new MemoryStream(buffer, 2, packetLength - headerLength - 2), CompressionMode.Decompress, true))
						{
							ds.CopyTo(dest);
						}
						decomp = dest.ToArray();
					}
					while (offset < decomp.Length)
					{
						var packetLength1 = DataView.GetInt32(decomp, offset);
						var headerLength1 = DataView.GetInt16(decomp, HeaderOffset + offset);
						var version1 = DataView.GetInt16(decomp, VersionOffset + offset);
						var operation1 = DataView.GetInt32(decomp, OperationOffset + offset);
						var sequence1 = DataView.GetInt32(decomp, SequenceOffset + offset);

						var newData = (byte[])decomp.Clone();
						DataView.ByteSlice(ref newData, offset + headerLength1, offset + packetLength1);
						yield return new DanmakuMessage()
						{
							PacketLength = packetLength1,
							HeaderLength = headerLength1,
							Version = version1,
							Operation = (DanmakuOperation)operation1,
							Sequence = sequence1,
							Body = Encoding.UTF8.GetString(newData, 0, newData.Length),
							Buffer = decomp
						};
						offset += packetLength1;
						if (packetLength1 <= 0)
						{
							break;
						}
					}
				}
				else
				{
					while (offset < buffer.Length)
					{
						var packetLength1 = DataView.GetInt32(buffer, offset);
						var headerLength1 = DataView.GetInt16(buffer, HeaderOffset + offset);
						var version1 = DataView.GetInt16(buffer, VersionOffset + offset);
						var operation1 = DataView.GetInt32(buffer, OperationOffset + offset);
						var sequence1 = DataView.GetInt32(buffer, SequenceOffset + offset);

						var data = (byte[])buffer.Clone();
						DataView.ByteSlice(ref data, offset + headerLength1, offset + packetLength1);
						yield return new DanmakuMessage()
						{
							PacketLength = packetLength1,
							HeaderLength = headerLength1,
							Version = version1,
							Operation = (DanmakuOperation)operation1,
							Sequence = sequence1,
							Body = Encoding.UTF8.GetString(data, 0, data.Length),
							Buffer = buffer
						};
						offset += packetLength1;
						if (packetLength1 <= 0)
						{
							break;
						}
					}
				}
			}
			else
			{
				yield return new DanmakuMessage()
				{
					PacketLength = packetLength,
					HeaderLength = headerLength,
					Version = version,
					Operation = (DanmakuOperation)operation,
					Sequence = sequence,
					Body = DataView.GetInt32(buffer, BiliBiliPacket.HeaderLength).ToString(),
					Buffer = buffer
				};
			}
		}
	}
}
