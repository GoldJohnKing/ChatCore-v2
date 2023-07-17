using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatCoreGUI
{
	public class MultiLanguage
	{
		public static string DefaultLanguage = "zh-CN";
		public static void setDefaultLanguage(string lang)
		{
			System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(lang);
			DefaultLanguage = lang;
			Properties.Settings.Default.DefaultLanguage = lang;
			Properties.Settings.Default.Save();
		}

		public static void LoadLanguage(Form form, Type formType)
		{
			if (form != null)
			{
				var resources = new System.ComponentModel.ComponentResourceManager(formType);
				resources.ApplyResources(form, "$this");
				Loading(form, resources);
			}
		}

		private static void Loading(Control control, System.ComponentModel.ComponentResourceManager resources)
		{
			if (control is MenuStrip)
			{
				resources.ApplyResources(control, control.Name);
				var ms = (MenuStrip)control;
				if (ms.Items.Count > 0)
				{
					foreach (ToolStripMenuItem c in ms.Items)
					{
						Loading(c, resources);
					}
				}
			}

			foreach (Control c in control.Controls)
			{
				resources.ApplyResources(c, c.Name);
				Loading(c, resources);
			}
		}

		private static void Loading(ToolStripMenuItem item, System.ComponentModel.ComponentResourceManager resources)
		{
			if (item is ToolStripMenuItem)
			{
				resources.ApplyResources(item, item.Name);
				var tsmi = (ToolStripMenuItem)item;
				if (tsmi.DropDownItems.Count > 0)
				{
					foreach (ToolStripMenuItem c in tsmi.DropDownItems)
					{
						Loading(c, resources);
					}
				}
			}
		}
	}
}
