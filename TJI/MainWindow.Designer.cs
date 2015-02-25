namespace TJI
{
    partial class MainWindow
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            this.TrayIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.label1 = new System.Windows.Forms.Label();
            this.togglApiToken = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.jiraServerUrl = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.jiraUsername = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.jiraPassword = new System.Windows.Forms.TextBox();
            this.saveSettingsButton = new System.Windows.Forms.Button();
            this.startStopButton = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.syncSleepTime = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // TrayIcon
            // 
            this.TrayIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("TrayIcon.Icon")));
            this.TrayIcon.Visible = true;
            this.TrayIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon1_MouseDoubleClick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(88, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Toggl API Token";
            // 
            // togglApiToken
            // 
            this.togglApiToken.Location = new System.Drawing.Point(13, 30);
            this.togglApiToken.Name = "togglApiToken";
            this.togglApiToken.Size = new System.Drawing.Size(267, 20);
            this.togglApiToken.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 58);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(69, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Jira server url";
            // 
            // jiraServerUrl
            // 
            this.jiraServerUrl.Location = new System.Drawing.Point(13, 74);
            this.jiraServerUrl.Name = "jiraServerUrl";
            this.jiraServerUrl.Size = new System.Drawing.Size(267, 20);
            this.jiraServerUrl.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 102);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(72, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Jira username";
            // 
            // jiraUsername
            // 
            this.jiraUsername.Location = new System.Drawing.Point(13, 118);
            this.jiraUsername.Name = "jiraUsername";
            this.jiraUsername.Size = new System.Drawing.Size(267, 20);
            this.jiraUsername.TabIndex = 5;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(13, 146);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(71, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Jira password";
            // 
            // jiraPassword
            // 
            this.jiraPassword.Location = new System.Drawing.Point(13, 162);
            this.jiraPassword.Name = "jiraPassword";
            this.jiraPassword.PasswordChar = '*';
            this.jiraPassword.Size = new System.Drawing.Size(267, 20);
            this.jiraPassword.TabIndex = 7;
            this.jiraPassword.UseSystemPasswordChar = true;
            // 
            // saveSettingsButton
            // 
            this.saveSettingsButton.Location = new System.Drawing.Point(200, 238);
            this.saveSettingsButton.Name = "saveSettingsButton";
            this.saveSettingsButton.Size = new System.Drawing.Size(80, 23);
            this.saveSettingsButton.TabIndex = 8;
            this.saveSettingsButton.Text = "Save settings";
            this.saveSettingsButton.UseVisualStyleBackColor = true;
            this.saveSettingsButton.Click += new System.EventHandler(this.saveSettingsButton_Click);
            // 
            // startStopButton
            // 
            this.startStopButton.Location = new System.Drawing.Point(119, 238);
            this.startStopButton.Name = "startStopButton";
            this.startStopButton.Size = new System.Drawing.Size(75, 23);
            this.startStopButton.TabIndex = 9;
            this.startStopButton.Text = "Start";
            this.startStopButton.UseVisualStyleBackColor = true;
            this.startStopButton.Click += new System.EventHandler(this.startStopButton_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(16, 189);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(95, 13);
            this.label5.TabIndex = 10;
            this.label5.Text = "Sync sleep time (s)";
            // 
            // syncSleepTime
            // 
            this.syncSleepTime.Location = new System.Drawing.Point(13, 206);
            this.syncSleepTime.Name = "syncSleepTime";
            this.syncSleepTime.Size = new System.Drawing.Size(267, 20);
            this.syncSleepTime.TabIndex = 11;
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 273);
            this.Controls.Add(this.syncSleepTime);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.startStopButton);
            this.Controls.Add(this.saveSettingsButton);
            this.Controls.Add(this.jiraPassword);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.jiraUsername);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.jiraServerUrl);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.togglApiToken);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "MainWindow";
            this.Text = "TJI";
            this.Load += new System.EventHandler(this.MainWindow_Load);
            this.Resize += new System.EventHandler(this.MainWindow_Resize);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NotifyIcon TrayIcon;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox togglApiToken;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox jiraServerUrl;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox jiraUsername;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox jiraPassword;
        private System.Windows.Forms.Button saveSettingsButton;
        private System.Windows.Forms.Button startStopButton;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox syncSleepTime;

    }
}