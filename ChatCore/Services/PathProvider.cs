using System;
using System.IO;
using ChatCore.Interfaces;

namespace ChatCore.Services
{
	public class PathProvider : IPathProvider
	{
		public string GetDataPath()
		{
			return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ".chatcore");
		}

		public string GetImagePath()
		{
			return Path.Combine(GetDataPath(), @"cache");
		}

		public string GetAvatarImagePath()
		{
			return Path.Combine(GetImagePath(), @"Avatars");
		}

		public string GetBadgesImagePath()
		{
			return Path.Combine(GetImagePath(), @"Badges");
		}

		public string GetResourcePath()
		{
			return Path.Combine(GetDataPath(), "resources");
		}
	}
}
