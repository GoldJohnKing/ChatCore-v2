using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls.WebParts;
using ExCSS;
using Svg;

namespace ChatCoreSVG
{
	public class SVG
	{
		public void genImg(string fileName, string imageName, int width = 144, int height = 880, List<SvgFontStruct> importedFonts = null)
		{
			/*using (var src = new Bitmap(fileName))
			{
				using (var ms = new MemoryStream())
				{
					src.Save(ms, ImageFormat.Png);
					File.WriteAllBytes(imageName, ms.ToArray());
				}
			}*/

			/*var svgDocument = SvgDocument.Open(fileName);
			var bitmap = svgDocument.Draw();
			bitmap.Save(imageName, ImageFormat.Png);*/

			using (var bitmap = new Bitmap(width, height))
			{
				using (var g = Graphics.FromImage(bitmap))
				{
					var svgDocument = SvgDocument.Open(fileName);
					if (importedFonts != null)
					{
						foreach (var font in importedFonts)
						{
							if (File.Exists(font.FontPath))
							{
								SvgFontManager.PrivateFontPathList.Add(font.FontPath);
								SvgFontManager.LocalizedFamilyNames.Add(font.FontNames);
							}
						}
					}
					var renderer = SvgRenderer.FromGraphics(g);
					svgDocument.Width = width;
					svgDocument.Height = height;
					svgDocument.Draw(renderer);
				}
				bitmap.Save(imageName, ImageFormat.Png);
			}
		}

		public static SvgFontStruct ImportFont(string fontName)
		{
			var result = new SvgFontStruct();

			switch (fontName)
			{
				case "MSYaheiUIL":
					result.FontNames = new string[] { "Microsoft YaHei UI Light", "MicrosoftYaHeiUILight", "微软雅黑 Light" };
					result.FontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Microsoft\Windows\Fonts\msyhl.ttc").ToString();
					break;
				case "MSYaheiUISL":
					result.FontNames = new string[] { "Microsoft YaHei UI Semilight", "MicrosoftYaHeiUISemilight", "微软雅黑 Semilight" };
					result.FontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Microsoft\Windows\Fonts\微软雅黑 Semilight & Microsoft YaHei UI Semilight.ttc").ToString();
					break;
				case "MSYaheiUI":
					result.FontNames = new string[] { "Microsoft YaHei UI", "MicrosoftYaHeiUI", "微软雅黑" };
					result.FontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Microsoft\Windows\Fonts\msyh.ttc").ToString();
					break;
				case "MSYaheiUISB":
					result.FontNames = new string[] { "Microsoft YaHei UI Semibold", "MicrosoftYaHeiUISemibold", "微软雅黑 Semibold" };
					result.FontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Microsoft\Windows\Fonts\微软雅黑 Semibold & Microsoft YaHei UI Semibold.ttc").ToString();
					break;
				case "MSYaheiUIB":
					result.FontNames = new string[] { "Microsoft YaHei UI Bold", "MicrosoftYaHeiUIBold", "微软雅黑 Bold" };
					result.FontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Microsoft\Windows\Fonts\msyhbd.ttc").ToString();
					break;
				default:
					result = ImportFont("MSYaheiUI");
					break;
			}

			return result;
		}

		public static List<SvgFontStruct> defaultFonts()
		{
			return new List<SvgFontStruct> { ImportFont("MSYaheiUISB") };
		}
	}

	public class SvgFontStruct
	{
		public string FontPath { get; set; }
		public string[] FontNames { get; set; }
	}
}
