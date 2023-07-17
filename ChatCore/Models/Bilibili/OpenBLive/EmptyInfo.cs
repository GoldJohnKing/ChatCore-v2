using System.Text.Json.Serialization;

namespace OpenBLive.Client.Data
{
    public class EmptyInfo
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

	}
}
