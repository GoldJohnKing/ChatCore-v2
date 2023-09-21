using System;
using System.Text.Json.Serialization;

namespace OpenBLive.Runtime.Data
{
	/// <summary>
	/// 礼物数据中的主播数据 https://open-live.bilibili.com/doc/2/2/1
	/// </summary>
    [Serializable]
    public struct AnchorInfo
    {
		/// <summary>
		/// 收礼主播UID
		/// </summary>
		[JsonPropertyName("uid")]
		public long uid { get; set; }

		/// <summary>
		/// 收礼主播昵称
		/// </summary>
		[JsonPropertyName("uname")]
		public string userName { get; set; }

		/// <summary>
		/// 收礼主播头像
		/// </summary>
		[JsonPropertyName("uface")]
		public string userFace { get; set; }
	}
}