using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using ChatCoreSVG;

namespace ChatCore.Utilities
{
	public static class ImageUtils
	{
		public static void genImg(string fileName, string imageName, int width = 144, int height = 80, bool defaultFont = false, List<SvgFontStruct>? importedFonts = null)
		{
			var result = new List<SvgFontStruct>();
			if (defaultFont)
			{
				result = SVG.defaultFonts();
			}

			if (importedFonts != null)
			{
				foreach (var font in importedFonts)
				{
					result.Add(font);
				}
			}
			new SVG().genImg(fileName, imageName, width, height, result);
		}

		public static string Base64fromImg(string imageFile)
		{
			return Convert.ToBase64String(File.ReadAllBytes(imageFile));
		}

		public static string Base64fromHTTPImg(string url)
		{
			byte[] bytes;
			using (var memoryStream = new MemoryStream())
			{
				var buffer = new StreamReader(new WebClient().OpenRead(url));
				buffer.BaseStream.CopyTo(memoryStream);
				bytes = memoryStream.ToArray();
			}
			return Convert.ToBase64String(bytes);
		}

		public static string Base64fromResourceImg(string imageFile)
		{
			byte[] bytes;
			using (var memoryStream = new MemoryStream())
			{
				var buffer = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream($"ChatCore.Resources.Web.Statics.Images.{imageFile}")!);
				buffer.BaseStream.CopyTo(memoryStream);
				bytes = memoryStream.ToArray();
			}
			return Convert.ToBase64String(bytes);
		}

		public static string AddBase64DataType(string base64)
		{
			return @"data:image/png;base64," + base64;
		}
	}
}
