using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Threading.Tasks;
using ChatCore.Interfaces;
using ChatCore.Models;
using ChatCore.Utilities;
using Microsoft.Extensions.Logging;

namespace ChatCore.Services.Twitch
{
	// ReSharper disable once InconsistentNaming
	public class BTTVDataProvider : IChatResourceProvider<ChatResourceData>
	{
		private readonly ILogger _logger;
		private readonly HttpClient _httpClient;

		public BTTVDataProvider(ILogger<BTTVDataProvider> logger, HttpClient httpClient)
		{
			_logger = logger;
			_httpClient = httpClient;
		}

		public ConcurrentDictionary<string, ChatResourceData?> Resources { get; } = new ConcurrentDictionary<string, ChatResourceData?>();

		public async Task<bool> TryRequestResources(string category)
		{
			var isGlobal = string.IsNullOrEmpty(category);
			try
			{
				_logger.LogDebug($"[BTTVDataProvider] | [TryRequestResources] | Requesting BTTV {(isGlobal ? "global " : "")}emotes{(isGlobal ? "." : $" for channel {category}")}.");
				using var msg = new HttpRequestMessage(HttpMethod.Get, isGlobal ? "https://api.betterttv.net/2/emotes" : $"https://api.betterttv.net/2/channels/{category}");
				msg.Headers.Add("User-Agent", $"ChatCore/{ChatCoreInstance.Version.ToString(3)}");
				var resp = await _httpClient.SendAsync(msg);
				if (!resp.IsSuccessStatusCode)
				{
					_logger.LogError($"[BTTVDataProvider] | [TryRequestResources] | Unsuccessful status code when requesting BTTV {(isGlobal ? "global " : "")}emotes{(isGlobal ? "." : " for channel " + category)}. {resp.ReasonPhrase}");
					return false;
				}

				var json = JSON.Parse(await resp.Content.ReadAsStringAsync());
				if (!json["emotes"].IsArray)
				{
					_logger.LogError("[BTTVDataProvider] | [TryRequestResources] | emotes was not an array.");
					return false;
				}

				var count = 0;
				var emoteArray = json["emotes"].AsArray;
				if (emoteArray != null)
				{
					foreach (JSONObject o in emoteArray)
					{
						var uri = $"https://cdn.betterttv.net/emote/{o["id"].Value}/3x";
						var identifier = isGlobal ? o["code"].Value : $"{category}_{o["code"].Value}";
						Resources.TryAdd(identifier, new ChatResourceData() { Uri = uri, IsAnimated = o["imageType"].Value == "gif", Type = isGlobal ? "BTTVGlobalEmote" : "BTTVChannelEmote" });
						count++;
					}
				}

				_logger.LogDebug($"[BTTVDataProvider] | [TryRequestResources] | Success caching {count} BTTV {(isGlobal ? "global " : "")}emotes{(isGlobal ? "." : " for channel " + category)}.");
				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"[BTTVDataProvider] | [TryRequestResources] | An error occurred while requesting BTTV {(isGlobal ? "global " : "")}emotes{(isGlobal ? "." : " for channel " + category)}.");
			}

			return false;
		}

		public bool TryGetResource(string identifier, string category, out ChatResourceData? data)
		{
			if (!string.IsNullOrEmpty(category) && Resources.TryGetValue($"{category}_{identifier}", out data))
			{
				return true;
			}

			if (Resources.TryGetValue(identifier, out data))
			{
				return true;
			}

			data = null;
			return false;
		}
	}
}