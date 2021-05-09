using ChatCore.Interfaces;
using ChatCore.Utilities;

namespace ChatCore.Models.BiliBili
{
	public class BiliBiliChatBadge : IChatBadge
	{
		public string Id { get; internal set; } = null!;
		public string Name { get; internal set; } = null!;
		public string Uri { get; internal set; } = null!;
		public string Color { get; internal set; } = null!;
		public int Level { get; internal set; } = 0;
		public int Guard { get; internal set; } = 0;

		public BiliBiliChatBadge() { }
		public BiliBiliChatBadge(string json)
		{
			var obj = JSON.Parse(json);
			if (obj.TryGetKey(nameof(Id), out var id))
			{ Id = id.Value; }
			if (obj.TryGetKey(nameof(Name), out var name))
			{ Name = name.Value; }
			if (obj.TryGetKey(nameof(Uri), out var uri))
			{ Uri = uri.Value; }
			if (obj.TryGetKey(nameof(Uri), out var color))
			{ Color = color.Value; }
			if (obj.TryGetKey(nameof(Uri), out var level))
			{ Level = level.AsInt; }
			if (obj.TryGetKey(nameof(Uri), out var guard))
			{ Guard = guard.AsInt; }
		}
		public JSONObject ToJson()
		{
			var obj = new JSONObject();
			obj.Add(nameof(Id), new JSONString(Id));
			obj.Add(nameof(Name), new JSONString(Name));
			obj.Add(nameof(Uri), new JSONString(Uri));
			obj.Add(nameof(Color), new JSONString(Color));
			obj.Add(nameof(Level), new JSONNumber(Level));
			obj.Add(nameof(Guard), new JSONNumber(Guard));
			return obj;
		}
	}
}
