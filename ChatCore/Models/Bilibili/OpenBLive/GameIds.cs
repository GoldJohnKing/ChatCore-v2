using System;
using System.Text.Json.Serialization;

namespace OpenBLive.Runtime.Data
{
    /// <summary>
    /// 互动玩法心跳 https://open-live.bilibili.com/doc/2/1/3
    /// </summary>
    public struct GameIds
    {
		/// <summary>
		/// 游戏场次
		/// </summary>
		[JsonPropertyName("game_ids")]
		public string[] gameIds { get; set; }
	}
}