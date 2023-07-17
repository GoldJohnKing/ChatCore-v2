using System;
using System.Text.Json.Serialization;

namespace OpenBLive.Runtime.Data
{
	/// <summary>
	/// 弹幕数据 https://open-live.bilibili.com/doc/2/2/0
	/// </summary>
    [Serializable]
    public struct Dm
    {
		/// <summary>
		/// 用户UID
		/// </summary>
		[JsonPropertyName("uid")]
		public long uid { get; set; }

		/// <summary>
		/// 用户昵称
		/// </summary>
		[JsonPropertyName("uname")]
		public string userName { get; set; }

		/// <summary>
		/// 用户头像
		/// </summary>
		[JsonPropertyName("uface")]
		public string userFace { get; set; }

		/// <summary>
		/// 弹幕发送时间秒级时间戳
		/// </summary>
		[JsonPropertyName("timestamp")]
		public long timestamp { get; set; }


		/// <summary>
		/// 弹幕内容
		/// </summary>
		[JsonPropertyName("msg")]
		public string msg { get; set; }

		/// <summary>
		/// 粉丝勋章等级
		/// </summary>
		[JsonPropertyName("fans_medal_level")]
		public long fansMedalLevel { get; set; }

		/// <summary>
		/// 粉丝勋章名
		/// </summary>
		[JsonPropertyName("fans_medal_name")]
		public string fansMedalName { get; set; }

		/// <summary>
		/// 佩戴的粉丝勋章佩戴状态
		/// </summary>
		[JsonPropertyName("fans_medal_wearing_status")]
		public bool fansMedalWearingStatus { get; set; }

		/// <summary>
		/// 大航海等级
		/// </summary>
		[JsonPropertyName("guard_level")]
		public long guardLevel { get; set; }

		/// <summary>
		/// 弹幕接收的直播间
		/// </summary>
		[JsonPropertyName("room_id")]
		public long roomId { get; set; }
	}
}