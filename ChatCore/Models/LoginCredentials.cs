using System.Collections.Generic;
using ChatCore.Config;

namespace ChatCore.Models
{
	public class LoginCredentials
	{
		[ConfigSection("Twitch")]
		[ConfigMeta(Comment = "The OAuth token associated with your Twitch account. Grab it from https://twitchapps.com/tmi/")]
		public string Twitch_OAuthToken = "";
		public readonly List<string> Twitch_Channels = new List<string>();

		[ConfigSection("Bilibili")]
		[ConfigMeta(Comment = "When value is postive number, Bilibili Live Danmuku will be listened.")]
		public int Bilibili_room_id = 0;
		[ConfigMeta(Comment = "The Identity Code associated with your Bilibili account. Grab it from https://link.bilibili.com/p/center/index#/my-room/start-live")]
		public string Bilibili_identity_code = "";
		[ConfigMeta(Comment = "Remember Identity code")]
		public bool Bilibili_identity_code_save = false;
		[ConfigMeta(Comment = "The Cookies associated with your Bilibili account has full permission!")]
		public string Bilibili_cookies = "";
	}
}
