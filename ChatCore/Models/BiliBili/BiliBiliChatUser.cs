using System;
using ChatCore.Interfaces;
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
		public IChatBadge[] Badges { get; internal set; } = Array.Empty<IChatBadge>();

		public BiliBiliChatUser() { }
		public BiliBiliChatUser(JSONNode info)
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
			if (info.TryGetKey(nameof(IsBroadcaster), out var isBroadcaster))
			{ IsBroadcaster = isBroadcaster.AsBool; }
			if (info.TryGetKey(nameof(IsModerator), out var isModerator))
			{ IsModerator = isModerator.AsBool; }
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
