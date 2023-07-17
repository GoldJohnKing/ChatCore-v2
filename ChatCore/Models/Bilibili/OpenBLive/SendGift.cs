using System;
using System.Text.Json.Serialization;

namespace OpenBLive.Runtime.Data
{
	/// <summary>
	/// 礼物数据 https://open-live.bilibili.com/doc/2/2/1
	/// </summary>
    [Serializable]
    public struct SendGift
    {
		/// <summary>
		/// 房间号
		/// </summary>
		[JsonPropertyName("room_id")]
		public long roomId { get; set; }

		/// <summary>
		/// 送礼用户UID
		/// </summary>
		[JsonPropertyName("uid")]
		public long uid { get; set; }

		/// <summary>
		/// 送礼用户昵称
		/// </summary>
		[JsonPropertyName("uname")]
		public string userName { get; set; }

		/// <summary>
		/// 送礼用户头像
		/// </summary>
		[JsonPropertyName("uface")]
		public string userFace { get; set; }

		/// <summary>
		/// 道具id(盲盒:爆出道具id)
		/// </summary>
		[JsonPropertyName("gift_id")]
		public long giftId { get; set; }

		/// <summary>
		/// 道具名(盲盒:爆出道具名)
		/// </summary>
		[JsonPropertyName("gift_name")]
		public string giftName { get; set; }

		/// <summary>
		/// 赠送道具数量
		/// </summary>
		[JsonPropertyName("gift_num")]
		public long giftNum { get; set; }

		/// <summary>
		/// 支付金额(1000 = 1元 = 10电池),盲盒:爆出道具的价值
		/// </summary>
		[JsonPropertyName("price")]
		public long price { get; set; }

		/// <summary>
		/// 是否真的花钱(电池道具)
		/// </summary>
		[JsonPropertyName("paid")]
		public bool paid { get; set; }

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
		/// 收礼时间秒级时间戳
		/// </summary>
		[JsonPropertyName("timestamp")]
		public long timestamp { get; set; }

		/// <summary>
		/// 主播信息
		/// </summary>
		[JsonPropertyName("anchor_info")]
		public AnchorInfo anchorInfo { get; set; }
	}
}