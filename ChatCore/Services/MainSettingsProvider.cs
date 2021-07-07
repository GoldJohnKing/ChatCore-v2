using System.IO;
using ChatCore.Config;
using ChatCore.Interfaces;
using ChatCore.Utilities;

namespace ChatCore.Services
{
	public class MainSettingsProvider
	{
		internal const int WEB_APP_PORT = 8338;

		[ConfigSection("WebApp")]
		[HtmlIgnore]
		[ConfigMeta(Comment = "Set to true to disable the webapp entirely.")]
		public bool DisableWebApp = false;
		[ConfigMeta(Comment = "Whether or not to launch the webapp in your default browser when ChatCore is started.")]
		public bool LaunchWebAppOnStartup = true;

		[ConfigSection("Global")]
		[ConfigMeta(Comment = "When enabled, emojis will be parsed.")]
		public bool ParseEmojis = true;

		[ConfigSection("Twitch")]
		[ConfigMeta(Comment = "When enabled, BetterTwitchTV emotes will be parsed.")]
		// ReSharper disable once InconsistentNaming
		public bool ParseBTTVEmotes = true;
		[ConfigMeta(Comment = "When enabled, FrankerFaceZ emotes will be parsed.")]
		// ReSharper disable once InconsistentNaming
		public bool ParseFFZEmotes = true;
		[ConfigMeta(Comment = "When enabled, Twitch emotes will be parsed.")]
		public bool ParseTwitchEmotes = true;
		[ConfigMeta(Comment = "When enabled, Twitch cheermotes will be parsed.")]
		public bool ParseCheermotes = true;

		[ConfigSection("Bilibili")]
		[ConfigMeta(Comment = "When value is postive number, Bilibili Live Danmuku will be listened.")]
		public int bilibili_room_id = 0;
		[ConfigMeta(Comment = "When enabled, Danmuku Msg will be parsed.")]
		public bool danmuku_danmuku = true;
		[ConfigMeta(Comment = "When enabled, Super Chat Msg will be parsed.")]
		public bool danmuku_superchat = true;
		[ConfigMeta(Comment = "When enabled, Gift Msg will be parsed.")]
		public bool danmuku_gift = true;
		[ConfigMeta(Comment = "When enabled, Gift Combo Msg will be parsed.")]
		public bool danmuku_gift_combo = false;
		[ConfigMeta(Comment = "When enabled, Enter Room Msg will be parsed.")]
		public bool danmuku_interaction_enter = true;
		[ConfigMeta(Comment = "When enabled, Follow Msg will be parsed.")]
		public bool danmuku_interaction_follow = true;
		[ConfigMeta(Comment = "When enabled, Share Msg will be parsed.")]
		public bool danmuku_interaction_share = true;
		[ConfigMeta(Comment = "When enabled, Special Follow Msg will be parsed.")]
		public bool danmuku_interaction_special_follow = false;
		[ConfigMeta(Comment = "When enabled, Mutual Follow Msg will be parsed.")]
		public bool danmuku_interaction_mutual_follow = false;
		[ConfigMeta(Comment = "When enabled, Guard Enter Msg will be parsed.")]
		public bool danmuku_interaction_guard_enter = true;
		[ConfigMeta(Comment = "When enabled, Effect Msg will be parsed.")]
		public bool danmuku_interaction_effect = false;
		[ConfigMeta(Comment = "When enabled, Anchor Msg will be parsed.")]
		public bool danmuku_interaction_anchor = false;
		[ConfigMeta(Comment = "When enabled, Raffle Msg will be parsed.")]
		public bool danmuku_interaction_raffle = false;
		[ConfigMeta(Comment = "When enabled, New Guard Msg will be parsed.")]
		public bool danmuku_new_guard = true;
		[ConfigMeta(Comment = "When enabled, New Guard Details Msg will be parsed.")]
		public bool danmuku_new_guard_msg = false;
		[ConfigMeta(Comment = "When enabled, Guard Msg will be parsed.")]
		public bool danmuku_guard_msg = false;
		[ConfigMeta(Comment = "When enabled, Guard Lottery Msg will be parsed.")]
		public bool danmuku_guard_lottery = false;
		[ConfigMeta(Comment = "When enabled, Block List Msg will be parsed.")]
		public bool danmuku_notification_block_list = false;
		[ConfigMeta(Comment = "When enabled, Room Info Change Msg will be parsed.")]
		public bool danmuku_notification_room_info_change = false;
		[ConfigMeta(Comment = "When enabled, Room Preparing Msg will be parsed.")]
		public bool danmuku_notification_room_prepare = false;
		[ConfigMeta(Comment = "When enabled, Room Online Msg will be parsed.")]
		public bool danmuku_notification_room_online = false;
		[ConfigMeta(Comment = "When enabled, Room Rank Msg will be parsed.")]
		public bool danmuku_notification_room_rank = false;
		[ConfigMeta(Comment = "When enabled, Boardcast Msg will be parsed.")]
		public bool danmuku_notification_boardcast = false;
		[ConfigMeta(Comment = "When enabled, Junk Msg will be parsed.")]
		public bool danmuku_notification_junk = false;
		[ConfigMeta(Comment = "When enabled, PK Msg will be parsed.")]
		public bool danmuku_notification_pk = false;
		[ConfigMeta(Comment = "User with keyword in the list in their username will be blocked.")]
		public string bilibili_block_list_username = "[]";
		[ConfigMeta(Comment = "User with UID will be blocked.")]
		public string bilibili_block_list_uid = "[]";
		[ConfigMeta(Comment = "Message contains the  keyword in the list will be blocked.")]
		public string bilibili_block_list_keyword = "[]";

		private readonly IPathProvider _pathProvider;
		private readonly ObjectSerializer _configSerializer;

		public MainSettingsProvider(IPathProvider pathProvider)
		{
			_pathProvider = pathProvider;
			_configSerializer = new ObjectSerializer();

			var path = Path.Combine(_pathProvider.GetDataPath(), "settings.ini");
			_configSerializer.Load(this, path);
			_configSerializer.Save(this, path);
		}

		public void Save()
		{
			_configSerializer.Save(this, Path.Combine(_pathProvider.GetDataPath(), "settings.ini"));
		}

		public JSONObject GetSettingsAsJson()
		{
			return _configSerializer.GetSettingsAsJson(this);
		}

		public void SetFromDictionary(JSONObject postData)
		{
			_configSerializer.SetFromDictionary(this, postData);
		}
	}
}
