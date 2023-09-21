using System;
using System.Text.Json.Serialization;

namespace OpenBLive.Runtime.Data
{
	/// <summary>
	/// 赠送大航海的用户数据 https://open-live.bilibili.com/doc/2/2/2
	/// </summary>
    [Serializable]
    public struct UserInfo
    {
		/// <summary>
		/// 购买大航海的用户UID
		/// </summary>
		[JsonPropertyName("uid")]
		public long uid { get; set; }

		/// <summary>
		/// 购买大航海的用户昵称
		/// </summary>
		[JsonPropertyName("uname")]
		public string userName { get; set; }

		/// <summary>
		/// 购买大航海的用户头像
		/// </summary>
		[JsonPropertyName("uface")]
		public string userFace { get; set; }
	}
}