using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using OpenBLive.Runtime.Data;
using OpenBLive.Runtime.Utilities;
using Logger = OpenBLive.Runtime.Utilities.Logger;

namespace OpenBLive.Runtime
{
	/// <summary>
	/// 各类b站api
	/// </summary>
	public static class BApi
	{
		/// <summary>
		/// 是否为测试环境的api
		/// </summary>
		public static bool isTestEnv;

		/// <summary>
		/// 开放平台域名
		/// </summary>
		private static string OpenLiveDomain =>
			isTestEnv ? "http://test-live-open.biliapi.net" : "https://live-open.biliapi.com";

		/// <summary>
		/// 应用开启
		/// </summary>
#pragma warning disable IDE1006 // Naming Styles
		private const string k_InteractivePlayStart = "/v2/app/start";
#pragma warning restore IDE1006 // Naming Styles

		/// <summary>
		/// 应用关闭
		/// </summary>
#pragma warning disable IDE1006 // Naming Styles
		private const string k_InteractivePlayEnd = "/v2/app/end";
#pragma warning restore IDE1006 // Naming Styles

		/// <summary>
		/// 应用心跳
		/// </summary>
#pragma warning disable IDE1006 // Naming Styles
		private const string k_InteractivePlayHeartBeat = "/v2/app/heartbeat";
#pragma warning restore IDE1006 // Naming Styles

		/// <summary>
		/// 应用批量心跳
		/// </summary>
#pragma warning disable IDE1006 // Naming Styles
		private const string k_InteractivePlayBatchHeartBeat = "/v2/app/batchHeartbeat";
#pragma warning restore IDE1006 // Naming Styles


#pragma warning disable IDE1006 // Naming Styles
		private const string k_Post = "POST";
#pragma warning restore IDE1006 // Naming Styles



		public static async Task<string> StartInteractivePlay(string code, string appId)
		{
			var postUrl = OpenLiveDomain + k_InteractivePlayStart;
			var param = $"{{\"code\":\"{code}\",\"app_id\":{appId}}}";

			var result = await RequestWebUTF8(postUrl, k_Post, param);
			return result;
		}

		public static async Task<string> EndInteractivePlay(string appId, string gameId)
		{
			var postUrl = OpenLiveDomain + k_InteractivePlayEnd;
			var param = $"{{\"app_id\":{appId},\"game_id\":\"{gameId}\"}}";

			var result = await RequestWebUTF8(postUrl, k_Post, param);
			return result;
		}

		public static async Task<string> HeartBeatInteractivePlay(string gameId)
		{
			var postUrl = OpenLiveDomain + k_InteractivePlayHeartBeat;
			var param = "";
			if (gameId != null)
			{
				param = $"{{\"game_id\":\"{gameId}\"}}";

			}

			var result = await RequestWebUTF8(postUrl, k_Post, param);
			return result;
		}

		public static async Task<string> BatchHeartBeatInteractivePlay(string[] gameIds)
		{
			var postUrl = OpenLiveDomain + k_InteractivePlayBatchHeartBeat;
			var games = new GameIds()
			{
				gameIds = gameIds
			};
			var param = JsonSerializer.Serialize(games);
			var result = await RequestWebUTF8(postUrl, k_Post, param);
			return result;
		}

		private static async Task<string> RequestWebUTF8(string url, string method, string param,
			string? cookie = null)
		{
			var result = "";
			var req = (HttpWebRequest)WebRequest.Create(url);
			req.Method = method;

			if (param != null)
			{
				SignUtility.SetReqHeader(req, param, cookie);
			}

			var httpResponse = (HttpWebResponse)(await req.GetResponseAsync());
			var stream = httpResponse.GetResponseStream();

			if (stream != null)
			{
				using var reader = new StreamReader(stream, Encoding.UTF8);
				result = await reader.ReadToEndAsync();
			}

			return result;
		}
	}
}