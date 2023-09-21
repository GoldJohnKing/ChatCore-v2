using System;
using System.Text.Json.Serialization;

namespace OpenBLive.Runtime.Data
{
    /// <summary>
    /// 付费留言数据下线 https://open-live.bilibili.com/doc/2/2/3
    /// </summary>
    [Serializable]
    public struct SuperChatDel
    {
        /// <summary>
        /// 直播间ID
        /// </summary>
        [JsonPropertyName("room_id")]
		public long roomId;

        /// <summary>
        /// 留言id
        /// </summary>
        [JsonPropertyName("message_ids")]
		public long[] messageIds;
    }
}