using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OpenBLive.Client.Data
{
    public class AppStartInfo
    {
		/// <summary>
		/// 请求相应 非0为异常case 业务处理
		/// </summary>
		[JsonPropertyName("code")]
		public int? Code { get; set; }
		/// <summary>
		/// 异常case提示文案
		/// </summary>
		[JsonPropertyName("message")]
		public string? Message { get; set; }
		/// <summary>
		///响应体
		/// </summary>
		[JsonPropertyName("data")]
		public AppStartData? Data { get; set; }


        /// <summary>
        /// 获取GameId
        /// </summary>
        /// <returns></returns>
        public string? GetGameId() => Data?.GameInfo?.GameId;
        /// <summary>
        /// 获取长链地址
        /// </summary>
        /// <returns></returns>
        public IList<string>? GetWssLink() => Data?.WebsocketInfo?.WssLink;


        /// <summary>
        /// 获取长链地址
        /// </summary>
        /// <returns></returns>
        public string? GetAuthBody() => Data?.WebsocketInfo?.AuthBody;
    }

    public class AppStartData
    {
		/// <summary>
		/// 场次信息
		/// </summary>
		[JsonPropertyName("game_info")]
		public AppStartGameInfo? GameInfo { get; set; }
		/// <summary>
		/// 长连信息
		/// </summary>
		[JsonPropertyName("websocket_info")]
		public AppStartWebsocketInfo? WebsocketInfo { get; set; }
		/// <summary>
		/// 主播信息
		/// </summary>
		[JsonPropertyName("anchor_info")]
		public AppStartAnchorInfo? AnchorInfo { get; set; }
	}

    public class AppStartGameInfo
    {
		/// <summary>
		/// 场次id,心跳key(心跳保持20s-60s)调用一次,超过60s无心跳自动关闭,长连停止推送消息
		/// </summary>
		[JsonPropertyName("game_id")]
		public string? GameId { get; set; }
    }
    public class AppStartWebsocketInfo
    {
		/// <summary>
		/// 长连使用的请求json体 第三方无需关注内容,建立长连时使用即可
		/// </summary>
		[JsonPropertyName("auth_body")]
		public string? AuthBody { get; set; }
		/// <summary>
		///  wss 长连地址
		/// </summary>
		[JsonPropertyName("wss_link")]
		public List<string>? WssLink { get; set; }
	}


    public class AppStartAnchorInfo
    {
		/// <summary>
		/// 主播房间号
		/// </summary>
		[JsonPropertyName("room_id")]
		public long? RoomId { get; set; }
		/// <summary>
		/// 主播昵称
		/// </summary>
		[JsonPropertyName("uname")]
		public string? UName { get; set; }
		/// <summary>
		/// 主播头像
		/// </summary>
		[JsonPropertyName("uface")]
		public string? UFace { get; set; }
		/// <summary>
		/// 主播Uid
		/// </summary>
		[JsonPropertyName("uid")]
		public long? Uid { get; set; }
	}
}
