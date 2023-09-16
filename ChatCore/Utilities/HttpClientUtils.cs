using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ChatCore.Utilities
{
    public class HttpClientUtils
    {
		public static string UserAgent = @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/113.0.0.0 Safari/537.36";
		private static HttpClient client = new HttpClient(new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate });

        public static async Task<List<string>> HttpClient(string url, HttpMethod httpMethod, string? cookieVal, HttpContent? content)
        {
            var result = new List<string>();
            try
			{
				var httpRequestMessage = new HttpRequestMessage(httpMethod, url);

                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Connection.Add("keep-alive");
                client.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);
                client.DefaultRequestHeaders.Add("ContentType", "application/json");
                if (cookieVal != null && cookieVal.Trim() != "")
                {
                    httpRequestMessage.Headers.Add("Cookie", cookieVal);
                }
                if (httpMethod == HttpMethod.Post && content != null)
                {
                    httpRequestMessage.Content = content;
                }
				HttpResponseMessage response = await client.SendAsync(httpRequestMessage);
				var responseBody = await response.Content.ReadAsStringAsync();
                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        result.Add("OK");
                        result.Add(responseBody);
                        break;
                    default:
                        result.Add("error");
                        result.Add($"{response.StatusCode}");
                        break;
                }
            }
            catch (Exception e)
            {
                result.Add("error");
                result.Add($"{e.Message}");
            }
            return result;
        }

        public static async Task<string> GetRedirectedUrl(string url)
        {
            //this allows you to set the settings so that we can get the redirect url
            var handler = new HttpClientHandler()
            {
                AllowAutoRedirect = false
			};
			var redirectedUrl = string.Empty;

            using (var client = new HttpClient(handler))
            using (HttpResponseMessage response = await client.GetAsync(url))
            using (HttpContent content = response.Content)
            {
                // ... Read the response to see if we have the redirected url
                if (response.StatusCode == System.Net.HttpStatusCode.Found)
                {
                    HttpResponseHeaders headers = response.Headers;
                    if (headers != null && headers.Location != null)
                    {
                        redirectedUrl = headers.Location.AbsoluteUri;
                    }
                }
            }

            return redirectedUrl;
        }
    }
}
