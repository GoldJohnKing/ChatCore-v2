using ChatCore.Interfaces;
using System.Collections.Generic;
using ChatCore.Utilities;
using System;

namespace ChatCore.Models.BiliBili
{
    public class BiliBiliChatUser : IChatUser
    {
        public string Id { get; internal set; }  = null!;
        public string UserName { get; internal set; } = null!;
        public string DisplayName { get; internal set; } = null!;
        public string Color { get; internal set; } = null!;
        public bool IsBroadcaster { get; internal set; }
        public bool IsModerator { get; internal set; }
		public IChatBadge[] Badges { get; internal set; } = Array.Empty<IChatBadge>();

        public BiliBiliChatUser() { }
        public BiliBiliChatUser(string json)
        {
            var info = JSON.Parse(json).AsArray;
			if (info == null)
			{
				return;
			}
			Id = info[2][0].Value;
			UserName = info[2][1];
			DisplayName = info[2][1];
			this.Color = string.IsNullOrEmpty(info[2][7].Value) ? info[2][7].Value : "#FFFFFF";
			if (info.TryGetKey(nameof(IsBroadcaster), out var isBroadcaster)) { IsBroadcaster = isBroadcaster.AsBool; }
            if (info.TryGetKey(nameof(IsModerator), out var isModerator)) { IsModerator = isModerator.AsBool; }
            //if (info.TryGetKey(nameof(Badges), out var badges))
            //{
            //    var badgeList = new List<IChatBadge>();
            //    if (badges.AsArray is not null)
            //    {
	           //     foreach (var badge in badges.AsArray)
	           //     {
		          //      badgeList.Add(new UnknownChatBadge(badge.Value.ToString()));
	           //     }
            //    }

            //    Badges = badgeList.ToArray();
            //}
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
            return obj;
        }
    }
}
