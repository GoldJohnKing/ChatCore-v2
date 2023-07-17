using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.Json;
using ChatCore.Utilities;

namespace ChatCore.Models
{
	public class CookieInfo
	{
		public string Name { get; set; }
		public string Value { get; set; }
		public string Path { get; set; }
		public string Domain { get; set; }
		public CookieInfo(Cookie cookie)
		{
			Name = cookie.Name;
			Value = cookie.Value;
			Path = cookie.Path;
			Domain = cookie.Domain;
		}
		
		public Cookie toCookie()
		{
			return new Cookie(Name, Value, Path, Domain);
		}

		public string toJson(CookieContainer cookies, Uri uri)
		{
			var collection = cookies.GetCookies(uri);
			var list = new List<CookieInfo>();
			foreach (Cookie item in collection)
			{
				list.Add(new CookieInfo(item));
			}
			var json = JsonSerializer.Serialize(list);
			return json;
		}

		public CookieContainer fromJson(string json, Uri uri)
		{
			var cookies = new CookieContainer();
			var list = JsonSerializer.Deserialize<List<CookieInfo>>(json);
			var collection = new CookieCollection();
			foreach (CookieInfo item in list)
			{
				collection.Add(item.toCookie());
			}

			cookies.Add(collection);
			return cookies;
		}
	}

}
