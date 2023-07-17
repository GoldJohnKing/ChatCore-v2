using System;
using System.Collections.Generic;
using System.Text;

namespace ChatCore.Models.Bilibili
{
	public interface BilibiliChatMessageExtra
	{
	}

	public class BilibiliChatMessageExtraDanmuku : BilibiliChatMessageExtra
	{
		public BilibiliChatMessageExtraDanmuku() { }
		public string raw_msg { get; set; } = "";
	}

	public class BilibiliChatMessageExtraEmotionDanmuku : BilibiliChatMessageExtra
	{
		public BilibiliChatMessageExtraEmotionDanmuku() { }
		public string raw_msg { get; set; } = "";
		public string emoticon_id { get; set; } = "";
		public string emoticon_name { get; set; } = "";
		public string emoticon_img { get; set; } = "";
	}

	public class BilibiliChatMessageExtraGift : BilibiliChatMessageExtra
	{
		public BilibiliChatMessageExtraGift() { }
		public string gift_id { get; set; } = "";
		public string gift_action { get; set; } = "";
		public int gift_num { get; set; } = 0;
		public string gift_name { get; set; } = "";
		public string origin_gift { get; set; } = "";
		public string gift_type { get; set; } = "";
		public double gift_price { get; set; } = 0.0;
		public string gift_img { get; set; } = "";
	}

	public class BilibiliChatMessageExtraSuperChat : BilibiliChatMessageExtra
	{
		public BilibiliChatMessageExtraSuperChat() { }
		public string sc_price { get; set; } = "";
		public string sc_time { get; set; } = "";
	}

	public class BilibiliChatMessageExtraRedPacket : BilibiliChatMessageExtra
	{
		public BilibiliChatMessageExtraRedPacket() { }
		public string gift_id = "13000";
		public string gift_action = "赠送";
		public int gift_num = 1;
		public string gift_name = "红包";
		public string origin_gift = "";
		public string gift_type = "gold";
		public double gift_price { get; set; } = 0.0;
		public string gift_img { get; set; } = "";
	}

	public class BilibiliChatMessageExtraNewGuard : BilibiliChatMessageExtra
	{
		public BilibiliChatMessageExtraNewGuard() { }
		public string gift_name { get; set; } = "";
		public int gift_num = 1;
		public string gift_img { get; set; } = "";
		public double gift_price { get; set; } = 0.0;
	}

	public class BilibiliChatMessageExtraNewGuardMsg : BilibiliChatMessageExtra
	{
		public BilibiliChatMessageExtraNewGuardMsg() { }
		public string role_name { get; set; } = "";
		public string num { get; set; } = "";
		public string unit { get; set; } = "";
		public string gift_img { get; set; } = "";
		public double gift_price { get; set; } = 0.0;
	}

	public class BilibiliChatMessageExtraPK : BilibiliChatMessageExtra
	{
		public BilibiliChatMessageExtraPK() { }
		public int pk_timer { get; set; } = 0;
		public string pk_uname { get; set; } = "";
	}
}
