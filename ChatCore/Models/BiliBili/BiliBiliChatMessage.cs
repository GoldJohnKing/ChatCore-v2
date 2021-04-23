using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ChatCore.Interfaces;
using ChatCore.Utilities;

namespace ChatCore.Models.BiliBili
{
	public class BiliBiliChatMessage : IChatMessage
	{
		public string Id { get; internal set; } = "";
		public bool IsSystemMessage { get; internal set; }
		public bool IsActionMessage { get; internal set; }
		public bool IsHighlighted { get; internal set; }
		public bool IsPing { get; internal set; }
		public string Message { get; internal set; } = "";
		public IChatUser Sender { get; internal set; } = new BiliBiliChatUser();
		public IChatChannel Channel { get; internal set; } = new BiliBiliChatChannel();
		public IChatEmote[] Emotes { get; internal set; } = Array.Empty<IChatEmote>();
		public ReadOnlyDictionary<string, string> Metadata { get; internal set; } = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>());
		public string MessageType { get; private set; }
		private static readonly Dictionary<string, Action<BiliBiliChatMessage, JSONNode>> comands = new Dictionary<string, Action<BiliBiliChatMessage, JSONNode>>();

		static BiliBiliChatMessage()
		{
			CreateCommands();
		}
		public BiliBiliChatMessage(string json)
		{
			var obj = JSON.Parse(json);
			if (obj == null)
			{
				return;
			}
			Id = Guid.NewGuid().ToString();
			IsSystemMessage = false;
			IsActionMessage = false;
			IsHighlighted = false;
			IsPing = false;
			
			CreateMessage(JSON.Parse(json));
			
			
			
			//if (obj.TryGetKey(nameof(Emotes), out var emotes))
			//{
			//	var emoteList = new List<IChatEmote>();
			//	if (emotes.AsArray is not null)
			//	{
			//		foreach (var emote in emotes.AsArray)
			//		{
			//			if (emote.Value.TryGetKey(nameof(IChatEmote.Id), out var emoteNode))
			//			{
			//				emoteList.Add(new UnknownChatEmote(emoteNode.Value));
			//			}
			//		}
			//	}

			//	Emotes = emoteList.ToArray();
			//}
		}
		public JSONObject ToJson()
		{
			var obj = new JSONObject();
			obj.Add(nameof(Id), new JSONString(Id));
			obj.Add(nameof(IsSystemMessage), new JSONBool(IsSystemMessage));
			obj.Add(nameof(IsActionMessage), new JSONBool(IsActionMessage));
			obj.Add(nameof(IsActionMessage), new JSONBool(IsActionMessage));
			obj.Add(nameof(IsHighlighted), new JSONBool(IsHighlighted));
			obj.Add(nameof(IsPing), new JSONBool(IsPing));
			obj.Add(nameof(Message), new JSONString(Message));
			obj.Add(nameof(Sender), Sender?.ToJson());
			obj.Add(nameof(Channel), Channel?.ToJson());
			var emotes = new JSONArray();
			foreach (var emote in Emotes)
			{
				emotes.Add(emote.ToJson());
			}
			obj.Add(nameof(Emotes), emotes);
			return obj;
		}

		private static void CreateCommands()
		{
			comands.Add("DANMU_MSG", (b, danmuku) => {
				var info = danmuku["info"].AsArray;
				b.MessageType = "damuku";
				b.Message = info[1].Value;
				b.Sender = new BiliBiliChatUser(info);
				b.Channel = new BiliBiliChatChannel(danmuku);
			});
			comands.Add("DANMU_MSG:4:0:2:2:2:0", (b, danmuku) => {
				var info = danmuku["info"].AsArray;
				b.MessageType = "damuku";
				b.Message = info[1].Value;
				b.Sender = new BiliBiliChatUser(info);
				b.Channel = new BiliBiliChatChannel(danmuku);
				/*this.Message = "【弹幕】" + info[2][1].Value + ": " + info[1].Value;*/
				//if (BanListDetect(info[2][0].Value.ToString(), "uid") || BanListDetect(info[2][1].Value.ToString(), "username") || BanListDetect(info[1].Value.ToString(), "content"))
				//	this.MessageType = "banned";

			});
			comands.Add("SEND_GIFT", (b, danmuku) => {
				b.MessageType = "gift";
				var data = danmuku["data"].AsObject;
				if (string.IsNullOrEmpty(data["combo_num"].Value))
				{
					b.Message = "【礼物】" + data["uname"].Value + data["action"].Value + data["num"].Value + "个" + data["giftName"].Value;
				}
				else
				{
					b.Message = "【礼物】" + data["uname"].Value + data["action"].Value + data["num"].Value + "个" + data["giftName"].Value + " x" + data["combo_num"].Value;
				}
				b.IsHighlighted = true;
				b.IsSystemMessage = true;
				//if (BanListDetect(data["uid"].Value.ToString(), "uid") || BanListDetect(data["uname"].Value.ToString(), "username"))
				//	this.MessageType = "banned";
			});
			comands.Add("COMBO_END", (b, danmuku) => { });
			comands.Add("COMBO_SEND", (b, danmuku) => { });
			comands.Add("SUPER_CHAT_MESSAGE", (b, danmuku) => { });
			comands.Add("SUPER_CHAT_MESSAGE_JPN", (b, danmuku) => { });
			comands.Add("WELCOME", (b, danmuku) => { });
			comands.Add("INTERACT_WORD", (b, danmuku) => {
				b.Message = $"{danmuku["data"].AsObject["uname"].Value} 进入直播间";
				b.IsSystemMessage = true;
			});
			comands.Add("WELCOME_GUARD", (b, danmuku) => { });
			comands.Add("ENTRY_EFFECT", (b, danmuku) => { });
			comands.Add("ACTIVITY_BANNER_UPDATE_V2", (b, danmuku) => { });
			comands.Add("ROOM_REAL_TIME_MESSAGE_UPDATE", (b, danmuku) => { });
			comands.Add("NOTICE_MSG", (b, danmuku) => { });
			comands.Add("ANCHOR_LOT_START", (b, danmuku) => { });
			comands.Add("ANCHOR_LOT_CHECKSTATUS", (b, danmuku) => { });
			comands.Add("ANCHOR_LOT_END", (b, danmuku) => { });
			comands.Add("ANCHOR_LOT_AWARD", (b, danmuku) => { });
			comands.Add("RAFFLE_START", (b, danmuku) => { });
			comands.Add("ROOM_BLOCK_MSG", (b, danmuku) => { });
			comands.Add("GUARD_BUY", (b, danmuku) => { });
			comands.Add("USER_TOAST_MSG", (b, danmuku) => { });
			comands.Add("GUARD_MSG", (b, danmuku) => { });
			comands.Add("GUARD_LOTTERY_START", (b, danmuku) => { });
			comands.Add("ROOM_CHANGE", (b, danmuku) => { });
			comands.Add("PREPARING", (b, danmuku) => { });
			comands.Add("LIVE", (b, danmuku) => { });
		}

		private void CreateMessage(JSONNode danmuku)
		{
			if (comands.TryGetValue(danmuku["cmd"].Value, out var commandAction))
			{
				commandAction?.Invoke(this, danmuku);
			}
			else
			{
				MessageType = "unkown";
				Message = "【暂不支持该消息】";
				IsSystemMessage = true;
			}
			//switch (danmuku["cmd"].Value)
			//{
			//	case "DANMU_MSG":
			//		if (danmuku["info"][2][2].Value == "1")// || danmuku["info"][2][0].Value == BilibiliChannelMaster)
			//		{
			//			if (danmuku["info"][1].Value == "!clr")
			//			{
			//				MessageType = "StreamCoreCMD_ClearMsg";
			//				if (danmuku["info"][2][2].Value == "1")
			//				{
			//					Message = $"房管 {danmuku["info"][2][1].Value} 清除了弹幕";
			//				}
			//				else
			//				{
			//					Message = $"主播 {danmuku["info"][2][1].Value} 清除了弹幕";
			//				}
			//			}
			//			else if (danmuku["info"][1].Value.StartsWith("!ban_usr "))
			//			{
			//				MessageType = "StreamCoreCMD_DeleteMsgByUser";
			//				Message = danmuku["info"][1].Value.Substring(9);

			//			}
			//			else if (danmuku["info"][1].Value.StartsWith("!ban_word "))
			//			{
			//				MessageType = "StreamCoreCMD_DeleteMsgByWord";
			//				Message = danmuku["info"][1].Value.Substring(10);

			//			}
			//			else
			//			{
			//				MessageType = "damuku";
			//				Message = danmuku["info"][1].Value;
			//			}
			//		}
			//		else
			//		{
			//			MessageType = "damuku";
			//			Message = danmuku["info"][1].Value;
			//			/*this.Message = "【弹幕】" + danmuku["info"][2][1].Value + ": " + danmuku["info"][1].Value;*/
			//			//if (BanListDetect(danmuku["info"][2][0].Value.ToString(), "uid") || BanListDetect(danmuku["info"][2][1].Value.ToString(), "username") || BanListDetect(danmuku["info"][1].Value.ToString(), "content"))
			//			//{
			//			//	this.MessageType = "banned";
			//			//}
			//		}
			//		break;
			//	case "DANMU_MSG:4:0:2:2:2:0":
			//		if ((danmuku["info"][1].Value == "!clr") && (danmuku["info"][2][2].Value == "1"))// || danmuku["info"][2][0].Value == BilibiliChannelMaster))
			//		{
			//			MessageType = "StreamCoreCMD_ClearMsg";
			//			Message = danmuku["info"][2][1].Value;
			//		}
			//		else
			//		{
			//			MessageType = "damuku";
			//			Message = danmuku["info"][2][1].Value + ": " + danmuku["info"][1].Value;
			//			/*this.Message = "【弹幕】" + danmuku["info"][2][1].Value + ": " + danmuku["info"][1].Value;*/
			//			//if (BanListDetect(danmuku["info"][2][0].Value.ToString(), "uid") || BanListDetect(danmuku["info"][2][1].Value.ToString(), "username") || BanListDetect(danmuku["info"][1].Value.ToString(), "content"))
			//			//	this.MessageType = "banned";
			//		}

			//		break;
			//	case "SEND_GIFT":
			//		MessageType = "gift";
			//		if (danmuku["data"]["combo_num"].Value == "")
			//		{
			//			Message = "【礼物】" + danmuku["data"]["uname"].Value + danmuku["data"]["action"].Value + danmuku["data"]["num"].Value + "个" + danmuku["data"]["giftName"].Value;
			//		}
			//		else
			//		{
			//			Message = "【礼物】" + danmuku["data"]["uname"].Value + danmuku["data"]["action"].Value + danmuku["data"]["num"].Value + "个" + danmuku["data"]["giftName"].Value + " x" + danmuku["data"]["combo_num"].Value;
			//		}
			//		//if (BanListDetect(danmuku["data"]["uid"].Value.ToString(), "uid") || BanListDetect(danmuku["data"]["uname"].Value.ToString(), "username"))
			//		//	this.MessageType = "banned";
			//		break;
			//	case "COMBO_END":
			//		MessageType = "combo_end";
			//		Message = "【连击】" + danmuku["data"]["uname"].Value + danmuku["data"]["action"].Value + danmuku["data"]["gift_num"].Value + "个" + danmuku["data"]["gift_name"].Value + " x" + danmuku["data"]["combo_num"].Value;
			//		//if (BanListDetect(danmuku["data"]["uid"].Value.ToString(), "uid") || BanListDetect(danmuku["data"]["uname"].Value.ToString(), "username"))
			//		//	this.MessageType = "banned";
			//		break;
			//	case "COMBO_SEND":
			//		MessageType = "Combo_send";
			//		Message = "【连击】" + danmuku["data"]["uname"].Value + danmuku["data"]["action"].Value + danmuku["data"]["gift_num"].Value + "个" + danmuku["data"]["gift_name"].Value + " x" + danmuku["data"]["combo_num"].Value;
			//		//if (BanListDetect(danmuku["data"]["uid"].Value.ToString(), "uid") || BanListDetect(danmuku["data"]["uname"].Value.ToString(), "username"))
			//		//	this.MessageType = "banned";
			//		break;
			//	case "SUPER_CHAT_MESSAGE":
			//		MessageType = "super_chat";
			//		Message = "【醒目留言】(￥" + danmuku["data"]["price"].Value + ") " + danmuku["data"]["user_info"]["uname"].Value + " 留言说: " + danmuku["data"]["Message"].Value;
			//		//if (BanListDetect(danmuku["data"]["user_info"]["uid"].Value.ToString(), "uid") || BanListDetect(danmuku["data"]["user_info"]["uname"].Value.ToString(), "username") || BanListDetect(danmuku["data"]["Message"].Value.ToString(), "content"))
			//		//	this.MessageType = "banned";
			//		break;
			//	case "SUPER_CHAT_MESSAGE_JPN":
			//		MessageType = "super_chat_japanese";
			//		Message = "【スーパーチャット】(CNY￥" + danmuku["data"]["price"].Value + ") " + danmuku["data"]["user_info"]["uname"].Value + " は言う: " + danmuku["data"]["message_jpn"].Value;
			//		//if (BanListDetect(danmuku["data"]["user_info"]["uid"].Value.ToString(), "uid") || BanListDetect(danmuku["data"]["user_info"]["uname"].Value.ToString(), "username") || BanListDetect(danmuku["data"]["Message"].Value.ToString(), "content"))
			//		//	this.MessageType = "banned";
			//		break;
			//	case "WELCOME":
			//		MessageType = "welcome";
			//		Message = "【入场】" + "欢迎老爷" + danmuku["data"]["uname"].Value + "进入直播间";
			//		//if (BanListDetect(danmuku["data"]["uid"].Value.ToString(), "uid") || BanListDetect(danmuku["data"]["uname"].Value.ToString(), "username"))
			//		//	this.MessageType = "banned";
			//		break;
			//	case "INTERACT_WORD":
			//		MessageType = "welcome";
			//		Message = "【入场】" + "欢迎" + danmuku["data"]["uname"].Value + "进入直播间";
			//		//if (BanListDetect(danmuku["data"]["uid"].Value.ToString(), "uid") || BanListDetect(danmuku["data"]["uname"].Value.ToString(), "username"))
			//		//	this.MessageType = "banned";
			//		break;
			//	case "WELCOME_GUARD":
			//		MessageType = "welcome_guard";
			//		Message = "【舰队】" + "欢迎舰长" + danmuku["data"]["username"].Value + "进入直播间";
			//		//if (BanListDetect(danmuku["data"]["uid"].Value.ToString(), "uid") || BanListDetect(danmuku["data"]["uname"].Value.ToString(), "username"))
			//		//	this.MessageType = "banned";
			//		break;
			//	case "ENTRY_EFFECT":
			//		MessageType = "effect";
			//		Message = "【特效】" + danmuku["data"]["copy_writing"].Value.Replace("<%", "").Replace("%>", "");
			//		//if (BanListDetect(danmuku["data"]["copy_writing"].Value.ToString(), "username"))
			//		//	this.MessageType = "banned";
			//		break;
			//	case "ROOM_RANK":
			//		MessageType = "global";
			//		Message = "【打榜】" + danmuku["data"]["rank_desc"].Value;
			//		break;
			//	case "ACTIVITY_BANNER_UPDATE_V2":
			//		MessageType = "global";
			//		Message = "【横幅】" + "当前分区排名" + danmuku["data"]["title"].Value;
			//		break;
			//	case "ROOM_REAL_TIME_MESSAGE_UPDATE":
			//		MessageType = "global";
			//		Message = "【关注】" + "粉丝数:" + danmuku["data"]["fans"].Value;
			//		break;
			//	case "NOTICE_MSG":
			//		MessageType = "junk";
			//		Message = "【喇叭】" + danmuku["data"]["msg_common"].Value;
			//		break;
			//	case "ANCHOR_LOT_START":
			//		MessageType = "anchor_lot_start";
			//		Message = "【天选】" + "天选之子活动开始啦";
			//		break;
			//	case "ANCHOR_LOT_CHECKSTATUS":
			//		MessageType = "anchor_lot_checkstatus";
			//		Message = "【天选】" + "天选之子活动开始啦";
			//		break;
			//	case "ANCHOR_LOT_END":
			//		MessageType = "anchor_lot_end";
			//		Message = "【天选】" + "天选之子活动结束啦，奖品是" + danmuku["data"]["award_name"].Value;
			//		break;
			//	case "ANCHOR_LOT_AWARD":
			//		MessageType = "anchor_lot";
			//		var list = danmuku["data"]["award_users"].AsArray;
			//		var usernameList = "";
			//		//for (int i = 0; i < list.Count; i++)
			//		//{
			//		//	usernameList += (BanListDetect(list[i]["uname"].Value.ToString(), "username") || BanListDetect(list[i]["uid"].Value.ToString(), "uid")) ? "【该用户已被过滤】" : list[i]["uname"].Value.ToString();
			//		//}
			//		Message = "【天选】" + "恭喜" + usernameList + "获得" + danmuku["data"]["award_name"].Value;
			//		break;
			//	case "RAFFLE_START":
			//		MessageType = "raffle_start";
			//		Message = "【抽奖】" + danmuku["data"]["title"].Value + "开始啦!";
			//		break;
			//	case "ROOM_BLOCK_MSG":
			//		MessageType = "blacklist";
			//		Message = "【封禁】" + danmuku["data"]["uname"].Value + "(UID: " + danmuku["data"]["uid"].Value + ")";
			//		break;
			//	case "GUARD_BUY":
			//		MessageType = "new_guard";
			//		Message = "【上舰】" + danmuku["data"]["username"].Value + "成为" + danmuku["data"]["gift_name"].Value + "进入舰队啦";
			//		//if (BanListDetect(danmuku["data"]["uid"].Value.ToString(), "uid") || BanListDetect(danmuku["data"]["username"].Value.ToString(), "username"))
			//		//	this.MessageType = "banned";
			//		break;
			//	case "USER_TOAST_MSG":
			//		MessageType = "new_guard_msg";
			//		Message = "【上舰】" + danmuku["data"]["username"].Value + "开通了" + danmuku["data"]["num"].Value + "个" + danmuku["data"]["unit"].Value + "的" + danmuku["data"]["role_name"].Value + "进入舰队啦";
			//		//if (BanListDetect(danmuku["data"]["uid"].Value.ToString(), "uid") || BanListDetect(danmuku["data"]["username"].Value.ToString(), "username"))
			//		//	this.MessageType = "banned";
			//		break;
			//	case "GUARD_MSG":
			//		if (danmuku["broadcast_type"].Value != "0")
			//		{
			//			MessageType = "guard_msg";
			//			Message = "【上舰】" + danmuku["data"]["msg"].Value.Replace(":?", "");
			//			//if (BanListDetect(danmuku["data"]["msg"].Value.ToString(), "username"))
			//			//	this.MessageType = "banned";
			//		}
			//		else
			//		{
			//			MessageType = "junk";
			//			Message = "【上舰广播】" + danmuku["data"]["msg"].Value.Replace(":?", "");
			//			//if (BanListDetect(danmuku["data"]["msg"].Value.ToString(), "username") || BanListDetect(danmuku["data"]["msg"].Value.ToString(), "content"))
			//			//	this.MessageType = "banned";
			//		}
			//		break;
			//	case "GUARD_LOTTERY_START":
			//		MessageType = "guard_lottery_msg";
			//		Message = "【抽奖】" + "上舰抽奖开始啦";
			//		break;
			//	case "ROOM_CHANGE":
			//		MessageType = "room_change";
			//		Message = "【变更】" + "直播间名称为: " + danmuku["data"]["title"].Value;
			//		break;
			//	case "PREPARING":
			//		MessageType = "room_perparing";
			//		Message = "【下播】" + "直播间准备中";
			//		break;
			//	case "LIVE":
			//		MessageType = "room_live";
			//		Message = "【开播】" + "直播间开播啦";
			//		break;
			//	default:
			//		MessageType = "unkown";
			//		Message = "【暂不支持该消息】";
			//		break;
			//}
		}
	}
}
