using System;
using System.Collections.Generic;
using ChatCore.Interfaces;
using ChatCore.Services;
using ChatCore.Utilities;

namespace ChatCore.Models.BiliBili
{
	public class BiliBiliChatUser : IChatUser
	{
		public string Id { get; internal set; } = "";
		public string UserName { get; internal set; } = "";
		public string DisplayName { get; internal set; } = "";
		public string Color { get; internal set; } = "#FFFFFF";
		public bool IsBroadcaster { get; internal set; }
		public bool IsModerator { get; internal set; }
		public bool IsFan { get; internal set; }
		public IChatBadge[] Badges { get; internal set; } = new IChatBadge[0];
		public int GuardLevel { get; internal set; } = 0;

		public BiliBiliChatUser() { }
		public BiliBiliChatUser(JSONNode info, int _room_id)
		{
			var infos = info.AsArray;
			if (infos == null)
			{
				return;
			}
			var userData = infos[2].AsArray;
			if (userData == null)
			{
				return;
			}
			Id = userData[0].AsInt.ToString();
			UserName = userData[1].Value;
			DisplayName = userData[1].Value;
			Color = !string.IsNullOrEmpty(userData[7].Value) ? userData[7].Value : "#FFFFFF";
			IsModerator = userData[2].AsInt == 1;
			IsBroadcaster = userData[0].AsInt == _room_id;
			if (infos[3].AsArray!.Count > 0)
			{
				IsFan = (infos[3].AsArray!)[3] == _room_id;
			}
			var badgeList = new List<IChatBadge>();
			if (!string.IsNullOrEmpty(infos[3][1].Value))
			{
				badgeList.Add(new BiliBiliChatBadge("{\"Name\":\"" + infos[3][1].Value + "\",\"Level\":" + infos[3][0].AsInt + ",\"Guard\":" + infos[3][10].AsInt + ",\"Color\":" + infos[3][4].AsInt + "}"));
			}
			Badges = badgeList.ToArray();
			GuardLevel = infos[7].AsInt;

			if (IsFan)
			{
				DisplayName = "[Lv "+ (infos[3].AsArray!)[0] + "]" + DisplayName;
			}

			if (GuardLevel > 0)
			{
				DisplayName = "[" + (GuardLevel == 3 ? "舰长" : (GuardLevel == 2 ? "提督" : "总督")) + "]" + DisplayName;
			}

			if (IsBroadcaster)
			{
				DisplayName = "[主播]" + DisplayName;
			} else if (IsModerator)
			{
				DisplayName = "[房管]" + DisplayName;
			}
		}
		public JSONObject ToJson()
		{
			var obj = new JSONObject();
			obj.Add(nameof(Id), new JSONString(Id));
			obj.Add(nameof(UserName), new JSONString(UserName));
			obj.Add(nameof(DisplayName), new JSONString(DisplayName));
			obj.Add(nameof(Color), new JSONString(Color));
			obj.Add(nameof(IsBroadcaster), new JSONBool(IsBroadcaster));
			obj.Add(nameof(IsModerator), new JSONBool(IsModerator));
			var badges = new JSONArray();
			foreach (var badge in Badges)
			{
				badges.Add(badge.ToJson());
			}
			obj.Add(nameof(Badges), badges);
			obj.Add(nameof(GuardLevel), new JSONNumber(GuardLevel));
			return obj;
		}
	}
}
