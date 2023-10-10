using System;

namespace ChatCoreGUI
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.groupBox_Console = new System.Windows.Forms.GroupBox();
            this.textBox_console = new System.Windows.Forms.TextBox();
            this.button_web = new System.Windows.Forms.Button();
            this.languageText = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.button_config = new System.Windows.Forms.Button();
            this.groupBox_Console.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox_Console
            // 
            this.groupBox_Console.Controls.Add(this.textBox_console);
            resources.ApplyResources(this.groupBox_Console, "groupBox_Console");
            this.groupBox_Console.Name = "groupBox_Console";
            this.groupBox_Console.TabStop = false;
            // 
            // textBox_console
            // 
            this.textBox_console.CausesValidation = false;
            resources.ApplyResources(this.textBox_console, "textBox_console");
            this.textBox_console.Name = "textBox_console";
            // 
            // button_web
            // 
            resources.ApplyResources(this.button_web, "button_web");
            this.button_web.Name = "button_web";
            this.button_web.UseVisualStyleBackColor = true;
            this.button_web.Click += new System.EventHandler(this.button_web_Click);
            // 
            // languageText
            // 
            this.languageText.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.languageText.FormattingEnabled = true;
            this.languageText.Items.AddRange(new object[] {
            resources.GetString("languageText.Items"),
            resources.GetString("languageText.Items1")});
            resources.ApplyResources(this.languageText, "languageText");
            this.languageText.Name = "languageText";
            this.languageText.SelectedIndexChanged += new System.EventHandler(this.languageText_SelectedIndexChanged);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // button_config
            // 
            resources.ApplyResources(this.button_config, "button_config");
            this.button_config.Name = "button_config";
            this.button_config.UseVisualStyleBackColor = true;
            this.button_config.Click += new System.EventHandler(this.button_config_Click);
            // 
            // MainForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label1);
            this.Controls.Add(this.languageText);
            this.Controls.Add(this.button_config);
            this.Controls.Add(this.button_web);
            this.Controls.Add(this.groupBox_Console);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FromClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.groupBox_Console.ResumeLayout(false);
            this.groupBox_Console.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

		#endregion

		private System.Windows.Forms.GroupBox groupBox_Console;
		private System.Windows.Forms.TextBox textBox_console;
		private System.Windows.Forms.Button button_web;
		private System.Windows.Forms.ComboBox languageText;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button button_config;
	}
}

