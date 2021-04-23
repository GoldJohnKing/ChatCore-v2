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
		public bool Danmuku = true;
		public bool Popularity = false;
		public bool Gift = false;
		public bool Guard = false;
		public bool Welcome = false;
		public bool Anchor = false;
		public bool Global = false;
		public bool Blacklist = false;
		public bool RoomInfo = false;
		public bool Junk = false;

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
