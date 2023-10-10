using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ChatCore;
using ChatCore.Interfaces;
using ChatCore.Models.Twitch;
using ChatCore.Services;
using ChatCore.Services.Bilibili;
using ChatCore.Services.Twitch;

namespace ChatCoreGUI
{
	public partial class MainForm : Form
    {
		private ChatCoreInstance _instance;
		private readonly ChatServiceMultiplexer _streamingService;
		private TwitchService _twitchService;
		private BilibiliService _biliBiliService;
		private OpenBLiveProvider _openBLiveProvider;
		private bool FirstLine = true;

		public delegate void update(string msg);
		public MainForm()
        {
            InitializeComponent();
			_instance = ChatCoreInstance.Create(null);
			_instance.OnLogReceived += (level, category, message) => Console.WriteLine($"{level} | {category} | {message}");
			_streamingService = _instance.RunAllServices();
			_twitchService = _streamingService.GetTwitchService();
			_biliBiliService = _streamingService.GetBilibiliService();
			_openBLiveProvider = _instance.RunBLiveServices();
			_streamingService.OnTextMessageReceived += StreamServiceProvider_OnMessageReceived;
		}

		private void StreamServiceProvider_OnMessageReceived(IChatService svc, IChatMessage msg)
		{
			addMsg($"{msg.Sender.DisplayName}: {msg.Message}");
		}

		private void button_web_Click(object sender, EventArgs e)
		{
			_instance.LaunchWebApp();
		}

		private void addMsg(string msg)
		{
			//var _update = new update(updateMsg);
			Task.Run(() => {
				textBox_console.BeginInvoke(new MethodInvoker(() =>
				{
					textBox_console.Text = $"{msg}\r\n" + textBox_console.Text + (FirstLine ? "\r\n\r\n" : "");
					FirstLine = false;
				}));
			});
		}

		private void button_exit_Click(object sender, EventArgs e)
		{
			_instance.StopBLiveServices();
			_streamingService.OnTextMessageReceived -= StreamServiceProvider_OnMessageReceived;
			Application.Exit();
		}

		private void MainForm_FromClosing(object sender, FormClosingEventArgs e)
		{
			_openBLiveProvider.Stop();
			_instance.StopAllServices();
		}

		private void languageText_SelectedIndexChanged(object sender, EventArgs e)
		{
			languageText.Enabled = false;
			switch (languageText.Text)
			{
				case "简体中文(默认)":
					MultiLanguage.setDefaultLanguage("zh-CN");
					break;
				case "English":
				default:
					MultiLanguage.setDefaultLanguage("en-US");
					break;
			}
			foreach (Form form in Application.OpenForms)
			{
				LoadAll(form);
			}
			languageText.Enabled = true;
		}

		private void LoadAll(Form form)
		{
			MultiLanguage.LoadLanguage(form, typeof(MainForm));
		}

		private void MainForm_Load(object sender, EventArgs e)
		{
			var language = Properties.Settings.Default.DefaultLanguage;
			switch (language)
			{
				case "zh-CN":
					languageText.Text = "简体中文(默认)";
					break;
				case "en-US":
				default:
					languageText.Text = "English";
					break;
			}
			MultiLanguage.LoadLanguage(this, typeof(MainForm));
		}

		private void button_config_Click(object sender, EventArgs e)
		{
			_instance.OpenConfigDirectory();
		}
	}
}
