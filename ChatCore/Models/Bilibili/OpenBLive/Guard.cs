using System;
using System.Text.Json.Serialization;

namespace OpenBLive.Runtime.Data
{
	/// <summary>
	/// 大航海数据 https://open-live.bilibili.com/doc/2/2/2
	/// </summary>
    [Serializable]
    public struct Guard
    {
		/// <summary>
		/// 大航海等级
		/// </summary>r
		[JsonPropertyName("guard_level")]
		public long guardLevel { get; set; }

		/// <summary>
		/// 大航海数量
		/// </summary>
		[JsonPropertyName("guard_num")]
		public long guardNum { get; set; }

		/// <summary>
		/// 大航海单位 "月"
		/// </summary>
		[JsonPropertyName("guard_unit")]
		public string guardUnit { get; set; }

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
		/// 赠送大航海的用户数据
		/// </summary>
		[JsonPropertyName("user_info")]
		public UserInfo userInfo { get; set; }

		/// <summary>
		/// 房间号
		/// </summary>
		[JsonPropertyName("room_id")]
		public long roomID { get; set; }
	}
}