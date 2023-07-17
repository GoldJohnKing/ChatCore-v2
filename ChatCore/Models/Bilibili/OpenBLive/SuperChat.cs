using System;
using System.Text.Json.Serialization;

namespace OpenBLive.Runtime.Data
{
	/// <summary>
	/// 付费留言数据 https://open-live.bilibili.com/doc/2/2/3
	/// </summary>
    [Serializable]
    public struct SuperChat
    {
		/// <summary>
		/// 直播间ID
		/// </summary>
		[JsonPropertyName("room_id")]
		public long roomId { get; set; }

		/// <summary>
		/// 购买用户UID
		/// </summary>
		[JsonPropertyName("uid")]
		public long uid { get; set; }

		/// <summary>
		/// 购买的用户昵称
		/// </summary>
		[JsonPropertyName("uname")]
		public string userName { get; set; }

		/// <summary>
		/// 购买用户头像
		/// </summary>
		[JsonPropertyName("uface")]
		public string userFace { get; set; }

		/// <summary>
		/// 留言id(风控场景下撤回留言需要)
		/// </summary>
		[JsonPropertyName("message_id")]
		public long messageId { get; set; }

		/// <summary>
		/// 留言内容
		/// </summary>
		[JsonPropertyName("message")]
		public string message { get; set; }

		/// <summary>
		/// 支付金额(元)
		/// </summary>
		[JsonPropertyName("rmb")]
		public long rmb { get; set; }

		/// <summary>
		/// 赠送时间秒级
		/// </summary>
		[JsonPropertyName("timestamp")]
		public long timeStamp { get; set; }

		/// <summary>
		/// 生效开始时间
		/// </summary>
		[JsonPropertyName("start_time")]
		public long startTime { get; set; }

		/// <summary>
		/// 生效结束时间
		/// </summary>
		[JsonPropertyName("end_time")]
		public long endTime { get; set; }

		/// <summary>
		/// 对应房间大航海等级
		/// </summary>
		[JsonPropertyName("guard_level")]
		public long guardLevel { get; set; }

		/// <summary>
		/// 对应房间勋章信息
		/// </summary>
		[JsonPropertyName("fans_medal_level")]
		public long fansMedalLevel { get; set; }

		/// <summary>
		/// 对应房间勋章名字
		/// </summary>
		[JsonPropertyName("fans_medal_name")]
		public string fansMedalName { get; set; }

		/// <summary>
		/// 当前佩戴的粉丝勋章佩戴状态
		/// </summary>
		[JsonPropertyName("fans_medal_wearing_status")]
		public bool fansMedalWearingStatus { get; set; }
	}
}