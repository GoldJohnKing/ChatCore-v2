using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using ChatCore;
using ChatCore.Interfaces;
using ChatCore.Models.Twitch;
using ChatCore.Services;
using ChatCore.Services.Bilibili;
using ChatCore.Services.Twitch;

namespace ChatCoreTester
{
	public partial class Form1 : Form
    {
        private readonly ChatServiceMultiplexer _streamingService;
        private TwitchService _twitchService;
		private BilibiliService _biliBiliService;
		private OpenBLiveProvider _openBLiveProvider;
        public Form1()
        {
            InitializeComponent();

            var chatCore = ChatCoreInstance.Create(null);
            chatCore.OnLogReceived += (level, category, message) => Debug.WriteLine($"{level} | {category} | {message}");
			_streamingService = chatCore.RunAllServices();
			_twitchService = _streamingService.GetTwitchService();
			_biliBiliService = _streamingService.GetBilibiliService();
			_openBLiveProvider = chatCore.RunBLiveServices();
			_streamingService.OnLogin += StreamingService_OnLogin;
            _streamingService.OnTextMessageReceived += StreamServiceProvider_OnMessageReceived;
            _streamingService.OnJoinChannel += StreamServiceProvider_OnChannelJoined;
            _streamingService.OnLeaveChannel += StreamServiceProvider_OnLeaveChannel;
            _streamingService.OnRoomStateUpdated += StreamServiceProvider_OnChannelStateUpdated;
            //Console.WriteLine($"StreamService is of type {streamServiceProvider.ServiceType.Name}");
        }

        private void StreamingService_OnLogin(IChatService svc)
        {
            if(svc is TwitchService twitchService)
            {
                twitchService.JoinChannel("realeris");
            }
        }

        private void StreamServiceProvider_OnChannelStateUpdated(IChatService svc, IChatChannel channel)
        {
            Console.WriteLine($"Channel state updated for {channel.GetType().Name} {channel.Id}");
            if (channel is TwitchChannel twitchChannel)
            {
                Console.WriteLine($"RoomId: {twitchChannel.Roomstate.RoomId}");
            }
        }

        private void StreamServiceProvider_OnLeaveChannel(IChatService svc, IChatChannel channel)
        {
            Console.WriteLine($"Left channel {channel.Id}");
        }

        private void StreamServiceProvider_OnChannelJoined(IChatService svc, IChatChannel channel)
        {
            Console.WriteLine($"Joined channel {channel.Id}");
        }

        private void StreamServiceProvider_OnMessageReceived(IChatService svc, IChatMessage msg)
        {
            Console.WriteLine($"{msg.Sender.DisplayName}: {msg.Message}");
            //Console.WriteLine(msg.ToJson().ToString());
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _streamingService.GetTwitchService().PartChannel("baoziiii");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            _streamingService.GetTwitchService().JoinChannel("baoziiii");
        }

		private void button3_Click(object sender, EventArgs e)
		{
			_twitchService.SendTextMessage("Heya", "realeris");
		}

		private void button_svg_open_Click(object sender, EventArgs e)
		{
			var dialog = new OpenFileDialog();
			dialog.Multiselect = false;
			dialog.Title = "Select a SVG";
			dialog.Filter = "SVG(*.svg)|*.svg";
			if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				svg_input_path.Text = dialog.FileName;
				svg_output_path.Text = Path.GetFileNameWithoutExtension(dialog.FileName).ToString() + ".png";
			}
		}

		private void button_svg_save_Click(object sender, EventArgs e)
		{
			var dialog = new SaveFileDialog();
			dialog.Title = "Save a PNG";
			dialog.Filter = "PNG(*.png)|*.png";
			if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				svg_output_path.Text = dialog.FileName;
			}
		}

		private void button_svg_convert_Click(object sender, EventArgs e)
		{
			try
			{
				if (!File.Exists(svg_input_path.Text))
				{
					MessageBox.Show("Input SVG not exists.");
					return;
				}
				else if (svg_output_path.Text == "")
				{
					MessageBox.Show("Please select a path to save PNG.");
					return;
				}
				else
				{

					ChatCore.Utilities.ImageUtils.genImg(svg_input_path.Text, svg_output_path.Text);
				}
			} catch (Exception ex)
			{
				MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
			}
		}

		private void button_bili_connect_Click(object sender, EventArgs e)
		{
			_biliBiliService.connectBilibili();
		}

		private void button_bili_mock_Click(object sender, EventArgs e)
		{
			_biliBiliService.DanmukuProcessor(Assembly.GetCallingAssembly(), textBox_bili_mock.Text);
		}

		private void button_bili_secret_check_Click(object sender, EventArgs e)
		{
			MessageBox.Show(_biliBiliService.BilibiliLiveAppSecretChecker());
		}

		private void button_bili_disconnect_Click(object sender, EventArgs e)
		{
			_biliBiliService.disconnectBilibili();
		}

		private void button_blive_start_Click(object sender, EventArgs e)
		{
			_openBLiveProvider.Start();
		}

		private void button_blive_end_Click(object sender, EventArgs e)
		{
			_openBLiveProvider.Stop(true);
		}
	}
}
