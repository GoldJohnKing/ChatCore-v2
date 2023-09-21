namespace ChatCoreTester
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.TwitchGroup = new System.Windows.Forms.GroupBox();
            this.BilibiliBox = new System.Windows.Forms.GroupBox();
            this.button_blive_end = new System.Windows.Forms.Button();
            this.button_blive_start = new System.Windows.Forms.Button();
            this.button_bili_secret_check = new System.Windows.Forms.Button();
            this.textBox_bili_mock = new System.Windows.Forms.TextBox();
            this.button_bili_mock = new System.Windows.Forms.Button();
            this.button_bili_disconnect = new System.Windows.Forms.Button();
            this.button_bili_connect = new System.Windows.Forms.Button();
            this.SVGGroup = new System.Windows.Forms.GroupBox();
            this.button_svg_open_directory = new System.Windows.Forms.Button();
            this.button_svg_open_output = new System.Windows.Forms.Button();
            this.button_svg_convert = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.svg_output_path = new System.Windows.Forms.TextBox();
            this.svg_input_path = new System.Windows.Forms.TextBox();
            this.button_svg_save = new System.Windows.Forms.Button();
            this.button_svg_open = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.TwitchGroup.SuspendLayout();
            this.BilibiliBox.SuspendLayout();
            this.SVGGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(76, 19);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(64, 24);
            this.button1.TabIndex = 0;
            this.button1.Text = "Part";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(6, 19);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(64, 24);
            this.button2.TabIndex = 1;
            this.button2.Text = "Join";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(147, 19);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(88, 24);
            this.button3.TabIndex = 2;
            this.button3.Text = "Test Message";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // TwitchGroup
            // 
            this.TwitchGroup.Controls.Add(this.button2);
            this.TwitchGroup.Controls.Add(this.button3);
            this.TwitchGroup.Controls.Add(this.button1);
            this.TwitchGroup.Location = new System.Drawing.Point(12, 12);
            this.TwitchGroup.Name = "TwitchGroup";
            this.TwitchGroup.Size = new System.Drawing.Size(508, 53);
            this.TwitchGroup.TabIndex = 3;
            this.TwitchGroup.TabStop = false;
            this.TwitchGroup.Text = "Twitch";
            // 
            // BilibiliBox
            // 
            this.BilibiliBox.Controls.Add(this.button_blive_end);
            this.BilibiliBox.Controls.Add(this.button_blive_start);
            this.BilibiliBox.Controls.Add(this.button_bili_secret_check);
            this.BilibiliBox.Controls.Add(this.textBox_bili_mock);
            this.BilibiliBox.Controls.Add(this.button_bili_mock);
            this.BilibiliBox.Controls.Add(this.button_bili_disconnect);
            this.BilibiliBox.Controls.Add(this.button_bili_connect);
            this.BilibiliBox.Location = new System.Drawing.Point(12, 71);
            this.BilibiliBox.Name = "BilibiliBox";
            this.BilibiliBox.Size = new System.Drawing.Size(508, 173);
            this.BilibiliBox.TabIndex = 4;
            this.BilibiliBox.TabStop = false;
            this.BilibiliBox.Text = "Bilibili";
            // 
            // button_blive_end
            // 
            this.button_blive_end.Location = new System.Drawing.Point(426, 135);
            this.button_blive_end.Name = "button_blive_end";
            this.button_blive_end.Size = new System.Drawing.Size(76, 23);
            this.button_blive_end.TabIndex = 5;
            this.button_blive_end.Text = "BLive End";
            this.button_blive_end.UseVisualStyleBackColor = true;
            this.button_blive_end.Click += new System.EventHandler(this.button_blive_end_Click);
            // 
            // button_blive_start
            // 
            this.button_blive_start.Location = new System.Drawing.Point(340, 135);
            this.button_blive_start.Name = "button_blive_start";
            this.button_blive_start.Size = new System.Drawing.Size(80, 23);
            this.button_blive_start.TabIndex = 4;
            this.button_blive_start.Text = "BLive Start";
            this.button_blive_start.UseVisualStyleBackColor = true;
            this.button_blive_start.Click += new System.EventHandler(this.button_blive_start_Click);
            // 
            // button_bili_secret_check
            // 
            this.button_bili_secret_check.Location = new System.Drawing.Point(254, 135);
            this.button_bili_secret_check.Name = "button_bili_secret_check";
            this.button_bili_secret_check.Size = new System.Drawing.Size(80, 23);
            this.button_bili_secret_check.TabIndex = 3;
            this.button_bili_secret_check.Text = "Check Secret";
            this.button_bili_secret_check.UseVisualStyleBackColor = true;
            this.button_bili_secret_check.Click += new System.EventHandler(this.button_bili_secret_check_Click);
            // 
            // textBox_bili_mock
            // 
            this.textBox_bili_mock.Location = new System.Drawing.Point(10, 19);
            this.textBox_bili_mock.Multiline = true;
            this.textBox_bili_mock.Name = "textBox_bili_mock";
            this.textBox_bili_mock.Size = new System.Drawing.Size(492, 110);
            this.textBox_bili_mock.TabIndex = 2;
            this.textBox_bili_mock.Text = resources.GetString("textBox_bili_mock.Text");
            // 
            // button_bili_mock
            // 
            this.button_bili_mock.Location = new System.Drawing.Point(172, 135);
            this.button_bili_mock.Name = "button_bili_mock";
            this.button_bili_mock.Size = new System.Drawing.Size(75, 23);
            this.button_bili_mock.TabIndex = 1;
            this.button_bili_mock.Text = "Mock";
            this.button_bili_mock.UseVisualStyleBackColor = true;
            this.button_bili_mock.Click += new System.EventHandler(this.button_bili_mock_Click);
            // 
            // button_bili_disconnect
            // 
            this.button_bili_disconnect.Location = new System.Drawing.Point(91, 135);
            this.button_bili_disconnect.Name = "button_bili_disconnect";
            this.button_bili_disconnect.Size = new System.Drawing.Size(75, 23);
            this.button_bili_disconnect.TabIndex = 0;
            this.button_bili_disconnect.Text = "Disconnect";
            this.button_bili_disconnect.UseVisualStyleBackColor = true;
            this.button_bili_disconnect.Click += new System.EventHandler(this.button_bili_disconnect_Click);
            // 
            // button_bili_connect
            // 
            this.button_bili_connect.Location = new System.Drawing.Point(10, 135);
            this.button_bili_connect.Name = "button_bili_connect";
            this.button_bili_connect.Size = new System.Drawing.Size(75, 23);
            this.button_bili_connect.TabIndex = 0;
            this.button_bili_connect.Text = "Connect";
            this.button_bili_connect.UseVisualStyleBackColor = true;
            this.button_bili_connect.Click += new System.EventHandler(this.button_bili_connect_Click);
            // 
            // SVGGroup
            // 
            this.SVGGroup.Controls.Add(this.button_svg_open_directory);
            this.SVGGroup.Controls.Add(this.button_svg_open_output);
            this.SVGGroup.Controls.Add(this.button_svg_convert);
            this.SVGGroup.Controls.Add(this.label2);
            this.SVGGroup.Controls.Add(this.label1);
            this.SVGGroup.Controls.Add(this.svg_output_path);
            this.SVGGroup.Controls.Add(this.svg_input_path);
            this.SVGGroup.Controls.Add(this.button_svg_save);
            this.SVGGroup.Controls.Add(this.button_svg_open);
            this.SVGGroup.Location = new System.Drawing.Point(12, 250);
            this.SVGGroup.Name = "SVGGroup";
            this.SVGGroup.Size = new System.Drawing.Size(507, 103);
            this.SVGGroup.TabIndex = 5;
            this.SVGGroup.TabStop = false;
            this.SVGGroup.Text = "SVG";
            // 
            // button_svg_open_directory
            // 
            this.button_svg_open_directory.Location = new System.Drawing.Point(245, 71);
            this.button_svg_open_directory.Name = "button_svg_open_directory";
            this.button_svg_open_directory.Size = new System.Drawing.Size(89, 23);
            this.button_svg_open_directory.TabIndex = 5;
            this.button_svg_open_directory.Text = "Open Directory";
            this.button_svg_open_directory.UseVisualStyleBackColor = true;
            // 
            // button_svg_open_output
            // 
            this.button_svg_open_output.Location = new System.Drawing.Point(146, 71);
            this.button_svg_open_output.Name = "button_svg_open_output";
            this.button_svg_open_output.Size = new System.Drawing.Size(88, 23);
            this.button_svg_open_output.TabIndex = 4;
            this.button_svg_open_output.Text = "Open Output";
            this.button_svg_open_output.UseVisualStyleBackColor = true;
            // 
            // button_svg_convert
            // 
            this.button_svg_convert.Location = new System.Drawing.Point(51, 71);
            this.button_svg_convert.Name = "button_svg_convert";
            this.button_svg_convert.Size = new System.Drawing.Size(88, 23);
            this.button_svg_convert.TabIndex = 3;
            this.button_svg_convert.Text = "Convert";
            this.button_svg_convert.UseVisualStyleBackColor = true;
            this.button_svg_convert.Click += new System.EventHandler(this.button_svg_convert_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(39, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Output";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(31, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Input";
            // 
            // svg_output_path
            // 
            this.svg_output_path.Location = new System.Drawing.Point(51, 45);
            this.svg_output_path.Name = "svg_output_path";
            this.svg_output_path.Size = new System.Drawing.Size(283, 20);
            this.svg_output_path.TabIndex = 1;
            this.svg_output_path.Text = "C:\\Users\\baoziii\\AppData\\Local\\ChatCore\\Badges\\幹杯_26_舰长.png";
            // 
            // svg_input_path
            // 
            this.svg_input_path.Location = new System.Drawing.Point(51, 19);
            this.svg_input_path.Name = "svg_input_path";
            this.svg_input_path.Size = new System.Drawing.Size(283, 20);
            this.svg_input_path.TabIndex = 1;
            this.svg_input_path.Text = "C:\\Users\\baoziii\\AppData\\Local\\ChatCore\\Badges\\幹杯_26_舰长.svg";
            // 
            // button_svg_save
            // 
            this.button_svg_save.Location = new System.Drawing.Point(340, 44);
            this.button_svg_save.Name = "button_svg_save";
            this.button_svg_save.Size = new System.Drawing.Size(75, 23);
            this.button_svg_save.TabIndex = 0;
            this.button_svg_save.Text = "Select";
            this.button_svg_save.UseVisualStyleBackColor = true;
            // 
            // button_svg_open
            // 
            this.button_svg_open.Location = new System.Drawing.Point(340, 18);
            this.button_svg_open.Name = "button_svg_open";
            this.button_svg_open.Size = new System.Drawing.Size(75, 23);
            this.button_svg_open.TabIndex = 0;
            this.button_svg_open.Text = "Select";
            this.button_svg_open.UseVisualStyleBackColor = true;
            this.button_svg_open.Click += new System.EventHandler(this.button_svg_open_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(532, 365);
            this.Controls.Add(this.SVGGroup);
            this.Controls.Add(this.BilibiliBox);
            this.Controls.Add(this.TwitchGroup);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ChatCore Test Console";
            this.TwitchGroup.ResumeLayout(false);
            this.BilibiliBox.ResumeLayout(false);
            this.BilibiliBox.PerformLayout();
            this.SVGGroup.ResumeLayout(false);
            this.SVGGroup.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
		private System.Windows.Forms.GroupBox TwitchGroup;
		private System.Windows.Forms.GroupBox BilibiliBox;
		private System.Windows.Forms.GroupBox SVGGroup;
		private System.Windows.Forms.Button button_svg_open_directory;
		private System.Windows.Forms.Button button_svg_open_output;
		private System.Windows.Forms.Button button_svg_convert;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox svg_output_path;
		private System.Windows.Forms.TextBox svg_input_path;
		private System.Windows.Forms.Button button_svg_save;
		private System.Windows.Forms.Button button_svg_open;
		private System.Windows.Forms.OpenFileDialog openFileDialog1;
		private System.Windows.Forms.Button button_bili_disconnect;
		private System.Windows.Forms.Button button_bili_connect;
		private System.Windows.Forms.TextBox textBox_bili_mock;
		private System.Windows.Forms.Button button_bili_mock;
		private System.Windows.Forms.Button button_bili_secret_check;
		private System.Windows.Forms.Button button_blive_start;
		private System.Windows.Forms.Button button_blive_end;
	}
}

