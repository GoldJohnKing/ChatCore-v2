using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using ChatCore.Interfaces;
using ChatCore.Services.Bilibili;
using ChatCore.Utilities;

namespace ChatCore.Models.Bilibili
{
	public class BilibiliGiftTimer : System.Timers.Timer
	{
		public string Name { get; internal set; }
		public string Action { get; internal set; }
		public string GiftId { get; internal set; }
		public string GiftName { get; internal set; }
		public int GiftNumber { get; internal set; }
		public double GiftPrice { get; internal set; }
		public string GiftUri { get; internal set; }
		public string GiftType { get; internal set; }
		public BilibiliChatUser Sender { get; internal set; }
		public Assembly Arg1 { get; internal set; }
		private int Counter { get; set; } = 1;
		public BilibiliChatMessage bmessage { get; internal set; }
		public BilibiliGiftTimer(string Name, string Action, string GiftId, string GiftName, int GiftNumber, double GiftPrice, string GiftUri, string GiftType, BilibiliChatUser Sender, Assembly Arg1, BilibiliChatMessage bmessage)
		{
			Interval = 5000;
			this.Name = Name;
			this.Action = Action;
			this.GiftId = GiftId;
			this.GiftName = GiftName;
			this.GiftNumber = GiftNumber;
			this.GiftPrice = GiftPrice;
			this.GiftUri = GiftUri;
			this.GiftType = GiftType;
			this.Sender = Sender;
			this.Arg1 = Arg1;
			this.bmessage = bmessage;
		}

		public void AddNumber(int number) {
			GiftNumber += number;
			Counter++;
		}

		public void UpdateSender(BilibiliChatUser Sender) {
			this.Sender = Sender;
		}

		public void UpdateArg1(Assembly Arg1)
		{
			this.Arg1 = Arg1;
		}

		public void GetMessage() {
			var GiftPlacholder = $"%GIFT_{GiftId}%";

			bmessage.Sender = Sender;
			var extra = new BilibiliChatMessageExtraGift();
			extra.gift_id = GiftId;
			extra.gift_action = Action;
			extra.gift_num = GiftNumber;
			extra.gift_name = GiftName;
			extra.origin_gift = "";
			extra.gift_type = GiftType;
			extra.gift_price = Math.Round(GiftPrice * GiftNumber, 1);
			extra.gift_img = GiftUri;
			bmessage.extra = extra;
			bmessage.Message = $"{Action}{GiftNumber}个{GiftName}{GiftPlacholder}({(GiftType == "silver" ? "免￥" : "￥")}{string.Format("{0:0.0}", Math.Round(GiftPrice * GiftNumber, 1))}" + (Counter == 1 ? "" : $" 已合并{Counter}次") + ")";

			var emote_list = new List<IChatEmote>();
			var target = new Regex(GiftPlacholder, RegexOptions.Compiled);
			Match match = target.Match(bmessage.Message);
			while (match.Success)
			{
				emote_list.Add(new BilibiliChatEmote(GiftPlacholder, GiftPlacholder, GiftUri, true, match.Index));
				match = match.NextMatch();
			}
			bmessage.Emotes = emote_list.ToArray();
		}

		public void ResetTimer() {
			Stop();
			Start();
		}
	}
}
