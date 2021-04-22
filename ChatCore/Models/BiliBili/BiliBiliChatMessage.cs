using ChatCore.Interfaces;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ChatCore.Utilities;
using Microsoft.Extensions.Logging;
using System;

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
        public IChatUser Sender { get; internal set; } = null!;
        public IChatChannel Channel { get; internal set; } = null!;
		public IChatEmote[] Emotes { get; internal set; } = Array.Empty<IChatEmote>();
        public ReadOnlyDictionary<string, string> Metadata { get; internal set; } = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>());
		public string MessageType { get; private set; }

		public BiliBiliChatMessage(string json)
        {
			var obj = JSON.Parse(json);
			Id = Guid.NewGuid().ToString();
			if (obj.TryGetKey("info", out var info))
			{
				IsSystemMessage = false;
				IsActionMessage = false;
				IsHighlighted = false;
				IsPing = false;
			}
			if (obj.TryGetKey("info", out var message))
			{
				this.CreateMessage(JSON.Parse(json), message);
			}
			else
			{
				this.Message = "";
			}
			Sender = new BiliBiliChatUser(json);
			
			if (obj.TryGetKey("data", out var channel))
			{
				Channel = new UnknownChatChannel(channel.ToString());
			}
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

		private void CreateMessage(JSONNode danmuku, string action)
		{
			switch (action)
			{
				case "DANMU_MSG":
					if (danmuku["info"][2][2].Value == "1")// || danmuku["info"][2][0].Value == BilibiliChannelMaster)
					{
						if (danmuku["info"][1].Value == "!clr")
						{
							this.MessageType = "StreamCoreCMD_ClearMsg";
							if (danmuku["info"][2][2].Value == "1")
							{
								this.Message = $"房管 {danmuku["info"][2][1].Value} 清除了弹幕";
							}
							else
							{
								this.Message = $"主播 {danmuku["info"][2][1].Value} 清除了弹幕";
							}
						}
						else if (danmuku["info"][1].Value.StartsWith("!ban_usr "))
						{
							this.MessageType = "StreamCoreCMD_DeleteMsgByUser";
							this.Message = danmuku["info"][1].Value.Substring(9);
							
						}
						else if (danmuku["info"][1].Value.StartsWith("!ban_word "))
						{
							this.MessageType = "StreamCoreCMD_DeleteMsgByWord";
							this.Message = danmuku["info"][1].Value.Substring(10);
							
						}
						else
						{
							this.MessageType = "damuku";
							this.Message = danmuku["info"][1].Value;
						}
					}
					else
					{
						this.MessageType = "damuku";
						this.Message = danmuku["info"][1].Value;
						/*this.Message = "【弹幕】" + danmuku["info"][2][1].Value + ": " + danmuku["info"][1].Value;*/
						//if (BanListDetect(danmuku["info"][2][0].Value.ToString(), "uid") || BanListDetect(danmuku["info"][2][1].Value.ToString(), "username") || BanListDetect(danmuku["info"][1].Value.ToString(), "content"))
						//{
						//	this.MessageType = "banned";
						//}
					}
					break;
				case "DANMU_MSG:4:0:2:2:2:0":
					if ((danmuku["info"][1].Value == "!clr") && (danmuku["info"][2][2].Value == "1"))// || danmuku["info"][2][0].Value == BilibiliChannelMaster))
					{
						this.MessageType = "StreamCoreCMD_ClearMsg";
						this.Message = danmuku["info"][2][1].Value;
					}
					else
					{
						this.MessageType = "damuku";
						this.Message = danmuku["info"][2][1].Value + ": " + danmuku["info"][1].Value;
						/*this.Message = "【弹幕】" + danmuku["info"][2][1].Value + ": " + danmuku["info"][1].Value;*/
						//if (BanListDetect(danmuku["info"][2][0].Value.ToString(), "uid") || BanListDetect(danmuku["info"][2][1].Value.ToString(), "username") || BanListDetect(danmuku["info"][1].Value.ToString(), "content"))
						//	this.MessageType = "banned";
					}

					break;
				case "SEND_GIFT":
					this.MessageType = "gift";
					if (danmuku["data"]["combo_num"].Value == "")
					{
						this.Message = "【礼物】" + danmuku["data"]["uname"].Value + danmuku["data"]["action"].Value + danmuku["data"]["num"].Value + "个" + danmuku["data"]["giftName"].Value;
					}
					else
					{
						this.Message = "【礼物】" + danmuku["data"]["uname"].Value + danmuku["data"]["action"].Value + danmuku["data"]["num"].Value + "个" + danmuku["data"]["giftName"].Value + " x" + danmuku["data"]["combo_num"].Value;
					}
					//if (BanListDetect(danmuku["data"]["uid"].Value.ToString(), "uid") || BanListDetect(danmuku["data"]["uname"].Value.ToString(), "username"))
					//	this.MessageType = "banned";
					break;
				case "COMBO_END":
					this.MessageType = "combo_end";
					this.Message = "【连击】" + danmuku["data"]["uname"].Value + danmuku["data"]["action"].Value + danmuku["data"]["gift_num"].Value + "个" + danmuku["data"]["gift_name"].Value + " x" + danmuku["data"]["combo_num"].Value;
					//if (BanListDetect(danmuku["data"]["uid"].Value.ToString(), "uid") || BanListDetect(danmuku["data"]["uname"].Value.ToString(), "username"))
					//	this.MessageType = "banned";
					break;
				case "COMBO_SEND":
					this.MessageType = "Combo_send";
					this.Message = "【连击】" + danmuku["data"]["uname"].Value + danmuku["data"]["action"].Value + danmuku["data"]["gift_num"].Value + "个" + danmuku["data"]["gift_name"].Value + " x" + danmuku["data"]["combo_num"].Value;
					//if (BanListDetect(danmuku["data"]["uid"].Value.ToString(), "uid") || BanListDetect(danmuku["data"]["uname"].Value.ToString(), "username"))
					//	this.MessageType = "banned";
					break;
				case "SUPER_CHAT_MESSAGE":
					this.MessageType = "super_chat";
					this.Message = "【醒目留言】(￥" + danmuku["data"]["price"].Value + ") " + danmuku["data"]["user_info"]["uname"].Value + " 留言说: " + danmuku["data"]["Message"].Value;
					//if (BanListDetect(danmuku["data"]["user_info"]["uid"].Value.ToString(), "uid") || BanListDetect(danmuku["data"]["user_info"]["uname"].Value.ToString(), "username") || BanListDetect(danmuku["data"]["Message"].Value.ToString(), "content"))
					//	this.MessageType = "banned";
					break;
				case "SUPER_CHAT_MESSAGE_JPN":
					this.MessageType = "super_chat_japanese";
					this.Message = "【スーパーチャット】(CNY￥" + danmuku["data"]["price"].Value + ") " + danmuku["data"]["user_info"]["uname"].Value + " は言う: " + danmuku["data"]["message_jpn"].Value;
					//if (BanListDetect(danmuku["data"]["user_info"]["uid"].Value.ToString(), "uid") || BanListDetect(danmuku["data"]["user_info"]["uname"].Value.ToString(), "username") || BanListDetect(danmuku["data"]["Message"].Value.ToString(), "content"))
					//	this.MessageType = "banned";
					break;
				case "WELCOME":
					this.MessageType = "welcome";
					this.Message = "【入场】" + "欢迎老爷" + danmuku["data"]["uname"].Value + "进入直播间";
					//if (BanListDetect(danmuku["data"]["uid"].Value.ToString(), "uid") || BanListDetect(danmuku["data"]["uname"].Value.ToString(), "username"))
					//	this.MessageType = "banned";
					break;
				case "INTERACT_WORD":
					this.MessageType = "welcome";
					this.Message = "【入场】" + "欢迎" + danmuku["data"]["uname"].Value + "进入直播间";
					//if (BanListDetect(danmuku["data"]["uid"].Value.ToString(), "uid") || BanListDetect(danmuku["data"]["uname"].Value.ToString(), "username"))
					//	this.MessageType = "banned";
					break;
				case "WELCOME_GUARD":
					this.MessageType = "welcome_guard";
					this.Message = "【舰队】" + "欢迎舰长" + danmuku["data"]["username"].Value + "进入直播间";
					//if (BanListDetect(danmuku["data"]["uid"].Value.ToString(), "uid") || BanListDetect(danmuku["data"]["uname"].Value.ToString(), "username"))
					//	this.MessageType = "banned";
					break;
				case "ENTRY_EFFECT":
					this.MessageType = "effect";
					this.Message = "【特效】" + danmuku["data"]["copy_writing"].Value.Replace("<%", "").Replace("%>", "");
					//if (BanListDetect(danmuku["data"]["copy_writing"].Value.ToString(), "username"))
					//	this.MessageType = "banned";
					break;
				case "ROOM_RANK":
					this.MessageType = "global";
					this.Message = "【打榜】" + danmuku["data"]["rank_desc"].Value;
					break;
				case "ACTIVITY_BANNER_UPDATE_V2":
					this.MessageType = "global";
					this.Message = "【横幅】" + "当前分区排名" + danmuku["data"]["title"].Value;
					break;
				case "ROOM_REAL_TIME_MESSAGE_UPDATE":
					this.MessageType = "global";
					this.Message = "【关注】" + "粉丝数:" + danmuku["data"]["fans"].Value;
					break;
				case "NOTICE_MSG":
					this.MessageType = "junk";
					this.Message = "【喇叭】" + danmuku["data"]["msg_common"].Value;
					break;
				case "ANCHOR_LOT_START":
					this.MessageType = "anchor_lot_start";
					this.Message = "【天选】" + "天选之子活动开始啦";
					break;
				case "ANCHOR_LOT_CHECKSTATUS":
					this.MessageType = "anchor_lot_checkstatus";
					this.Message = "【天选】" + "天选之子活动开始啦";
					break;
				case "ANCHOR_LOT_END":
					this.MessageType = "anchor_lot_end";
					this.Message = "【天选】" + "天选之子活动结束啦，奖品是" + danmuku["data"]["award_name"].Value;
					break;
				case "ANCHOR_LOT_AWARD":
					this.MessageType = "anchor_lot";
					JSONArray list = danmuku["data"]["award_users"].AsArray;
					string usernameList = "";
					//for (int i = 0; i < list.Count; i++)
					//{
					//	usernameList += (BanListDetect(list[i]["uname"].Value.ToString(), "username") || BanListDetect(list[i]["uid"].Value.ToString(), "uid")) ? "【该用户已被过滤】" : list[i]["uname"].Value.ToString();
					//}
					this.Message = "【天选】" + "恭喜" + usernameList + "获得" + danmuku["data"]["award_name"].Value;
					break;
				case "RAFFLE_START":
					this.MessageType = "raffle_start";
					this.Message = "【抽奖】" + danmuku["data"]["title"].Value + "开始啦!";
					break;
				case "ROOM_BLOCK_MSG":
					this.MessageType = "blacklist";
					this.Message = "【封禁】" + danmuku["data"]["uname"].Value + "(UID: " + danmuku["data"]["uid"].Value + ")";
					break;
				case "GUARD_BUY":
					this.MessageType = "new_guard";
					this.Message = "【上舰】" + danmuku["data"]["username"].Value + "成为" + danmuku["data"]["gift_name"].Value + "进入舰队啦";
					//if (BanListDetect(danmuku["data"]["uid"].Value.ToString(), "uid") || BanListDetect(danmuku["data"]["username"].Value.ToString(), "username"))
					//	this.MessageType = "banned";
					break;
				case "USER_TOAST_MSG":
					this.MessageType = "new_guard_msg";
					this.Message = "【上舰】" + danmuku["data"]["username"].Value + "开通了" + danmuku["data"]["num"].Value + "个" + danmuku["data"]["unit"].Value + "的" + danmuku["data"]["role_name"].Value + "进入舰队啦";
					//if (BanListDetect(danmuku["data"]["uid"].Value.ToString(), "uid") || BanListDetect(danmuku["data"]["username"].Value.ToString(), "username"))
					//	this.MessageType = "banned";
					break;
				case "GUARD_MSG":
					if (danmuku["broadcast_type"].Value != "0")
					{
						this.MessageType = "guard_msg";
						this.Message = "【上舰】" + danmuku["data"]["msg"].Value.Replace(":?", "");
						//if (BanListDetect(danmuku["data"]["msg"].Value.ToString(), "username"))
						//	this.MessageType = "banned";
					}
					else
					{
						this.MessageType = "junk";
						this.Message = "【上舰广播】" + danmuku["data"]["msg"].Value.Replace(":?", "");
						//if (BanListDetect(danmuku["data"]["msg"].Value.ToString(), "username") || BanListDetect(danmuku["data"]["msg"].Value.ToString(), "content"))
						//	this.MessageType = "banned";
					}
					break;
				case "GUARD_LOTTERY_START":
					this.MessageType = "guard_lottery_msg";
					this.Message = "【抽奖】" + "上舰抽奖开始啦";
					break;
				case "ROOM_CHANGE":
					this.MessageType = "room_change";
					this.Message = "【变更】" + "直播间名称为: " + danmuku["data"]["title"].Value;
					break;
				case "PREPARING":
					this.MessageType = "room_perparing";
					this.Message = "【下播】" + "直播间准备中";
					break;
				case "LIVE":
					this.MessageType = "room_live";
					this.Message = "【开播】" + "直播间开播啦";
					break;
				default:
					this.MessageType = "unkown";
					this.Message = "【暂不支持该消息】";
					break;
			}
		}
    }
}
