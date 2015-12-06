namespace TJI
{
    partial class SettingsWindow
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
            this.TrayIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.jiraServerUrlLabel = new System.Windows.Forms.Label();
            this.jiraServerUrl = new System.Windows.Forms.TextBox();
            this.jiraUsernameLabel = new System.Windows.Forms.Label();
            this.jiraUsername = new System.Windows.Forms.TextBox();
            this.jiraPasswordLabel = new System.Windows.Forms.Label();
            this.jiraPassword = new System.Windows.Forms.TextBox();
            this.cancelButton = new System.Windows.Forms.Button();
            this.togglTokenLabel = new System.Windows.Forms.Label();
            this.togglApiToken = new System.Windows.Forms.TextBox();
            this.syncTimeLabel = new System.Windows.Forms.Label();
            this.syncSleepTime = new System.Windows.Forms.TextBox();
            this.okButton = new System.Windows.Forms.Button();
            this.jiraGroup = new System.Windows.Forms.GroupBox();
            this.togglGroup = new System.Windows.Forms.GroupBox();
            this.advancedGroup = new System.Windows.Forms.GroupBox();
            this.jiraGroup.SuspendLayout();
            this.togglGroup.SuspendLayout();
            this.advancedGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // jiraServerUrlLabel
            // 
            this.jiraServerUrlLabel.AutoSize = true;
            this.jiraServerUrlLabel.Location = new System.Drawing.Point(6, 16);
            this.jiraServerUrlLabel.Name = "jiraServerUrlLabel";
            this.jiraServerUrlLabel.Size = new System.Drawing.Size(63, 13);
            this.jiraServerUrlLabel.TabIndex = 2;
            this.jiraServerUrlLabel.Text = "Server URL";
            // 
            // jiraServerUrl
            // 
            this.jiraServerUrl.Location = new System.Drawing.Point(6, 32);
            this.jiraServerUrl.Name = "jiraServerUrl";
            this.jiraServerUrl.Size = new System.Drawing.Size(267, 20);
            this.jiraServerUrl.TabIndex = 3;
            // 
            // jiraUsernameLabel
            // 
            this.jiraUsernameLabel.AutoSize = true;
            this.jiraUsernameLabel.Location = new System.Drawing.Point(6, 60);
            this.jiraUsernameLabel.Name = "jiraUsernameLabel";
            this.jiraUsernameLabel.Size = new System.Drawing.Size(55, 13);
            this.jiraUsernameLabel.TabIndex = 4;
            this.jiraUsernameLabel.Text = "Username";
            // 
            // jiraUsername
            // 
            this.jiraUsername.Location = new System.Drawing.Point(6, 76);
            this.jiraUsername.Name = "jiraUsername";
            this.jiraUsername.Size = new System.Drawing.Size(267, 20);
            this.jiraUsername.TabIndex = 5;
            // 
            // jiraPasswordLabel
            // 
            this.jiraPasswordLabel.AutoSize = true;
            this.jiraPasswordLabel.Location = new System.Drawing.Point(6, 104);
            this.jiraPasswordLabel.Name = "jiraPasswordLabel";
            this.jiraPasswordLabel.Size = new System.Drawing.Size(53, 13);
            this.jiraPasswordLabel.TabIndex = 6;
            this.jiraPasswordLabel.Text = "Password";
            // 
            // jiraPassword
            // 
            this.jiraPassword.Location = new System.Drawing.Point(6, 120);
            this.jiraPassword.Name = "jiraPassword";
            this.jiraPassword.PasswordChar = '*';
            this.jiraPassword.Size = new System.Drawing.Size(267, 20);
            this.jiraPassword.TabIndex = 7;
            this.jiraPassword.UseSystemPasswordChar = true;
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(223, 326);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 9;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // togglTokenLabel
            // 
            this.togglTokenLabel.AutoSize = true;
            this.togglTokenLabel.Location = new System.Drawing.Point(6, 16);
            this.togglTokenLabel.Name = "togglTokenLabel";
            this.togglTokenLabel.Size = new System.Drawing.Size(58, 13);
            this.togglTokenLabel.TabIndex = 2;
            this.togglTokenLabel.Text = "API Token";
            // 
            // togglApiToken
            // 
            this.togglApiToken.Location = new System.Drawing.Point(6, 32);
            this.togglApiToken.Name = "togglApiToken";
            this.togglApiToken.Size = new System.Drawing.Size(267, 20);
            this.togglApiToken.TabIndex = 3;
            // 
            // syncTimeLabel
            // 
            this.syncTimeLabel.AutoSize = true;
            this.syncTimeLabel.Location = new System.Drawing.Point(6, 16);
            this.syncTimeLabel.Name = "syncTimeLabel";
            this.syncTimeLabel.Size = new System.Drawing.Size(95, 13);
            this.syncTimeLabel.TabIndex = 10;
            this.syncTimeLabel.Text = "Sync sleep time (s)";
            // 
            // syncSleepTime
            // 
            this.syncSleepTime.Location = new System.Drawing.Point(6, 32);
            this.syncSleepTime.Name = "syncSleepTime";
            this.syncSleepTime.Size = new System.Drawing.Size(267, 20);
            this.syncSleepTime.TabIndex = 11;
            // 
            // okButton
            // 
            this.okButton.Location = new System.Drawing.Point(142, 326);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 13;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // jiraGroup
            // 
            this.jiraGroup.Controls.Add(this.jiraServerUrlLabel);
            this.jiraGroup.Controls.Add(this.jiraServerUrl);
            this.jiraGroup.Controls.Add(this.jiraPasswordLabel);
            this.jiraGroup.Controls.Add(this.jiraUsernameLabel);
            this.jiraGroup.Controls.Add(this.jiraUsername);
            this.jiraGroup.Controls.Add(this.jiraPassword);
            this.jiraGroup.Location = new System.Drawing.Point(13, 13);
            this.jiraGroup.Name = "jiraGroup";
            this.jiraGroup.Size = new System.Drawing.Size(285, 149);
            this.jiraGroup.TabIndex = 14;
            this.jiraGroup.TabStop = false;
            this.jiraGroup.Text = "Jira";
            // 
            // togglGroup
            // 
            this.togglGroup.Controls.Add(this.togglTokenLabel);
            this.togglGroup.Controls.Add(this.togglApiToken);
            this.togglGroup.Location = new System.Drawing.Point(13, 169);
            this.togglGroup.Name = "togglGroup";
            this.togglGroup.Size = new System.Drawing.Size(285, 64);
            this.togglGroup.TabIndex = 15;
            this.togglGroup.TabStop = false;
            this.togglGroup.Text = "Toggl";
            // 
            // advancedGroup
            // 
            this.advancedGroup.Controls.Add(this.syncTimeLabel);
            this.advancedGroup.Controls.Add(this.syncSleepTime);
            this.advancedGroup.Location = new System.Drawing.Point(13, 240);
            this.advancedGroup.Name = "advancedGroup";
            this.advancedGroup.Size = new System.Drawing.Size(285, 64);
            this.advancedGroup.TabIndex = 16;
            this.advancedGroup.TabStop = false;
            this.advancedGroup.Text = "Advanced";
            // 
            // SettingsWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(310, 361);
            this.Controls.Add(this.advancedGroup);
            this.Controls.Add(this.togglGroup);
            this.Controls.Add(this.jiraGroup);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.cancelButton);
            this.Icon = global::TJI.Properties.Resources.StandardIcon;
            this.MaximizeBox = false;
            this.Name = "SettingsWindow";
            this.Text = "TJI - Settings";
            this.Load += new System.EventHandler(this.SettingsWindow_Load);
            this.jiraGroup.ResumeLayout(false);
            this.jiraGroup.PerformLayout();
            this.togglGroup.ResumeLayout(false);
            this.togglGroup.PerformLayout();
            this.advancedGroup.ResumeLayout(false);
            this.advancedGroup.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.NotifyIcon TrayIcon;
        private System.Windows.Forms.Label jiraServerUrlLabel;
        private System.Windows.Forms.TextBox jiraServerUrl;
        private System.Windows.Forms.Label jiraUsernameLabel;
        private System.Windows.Forms.TextBox jiraUsername;
        private System.Windows.Forms.Label jiraPasswordLabel;
        private System.Windows.Forms.TextBox jiraPassword;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Label togglTokenLabel;
        private System.Windows.Forms.TextBox togglApiToken;
        private System.Windows.Forms.Label syncTimeLabel;
        private System.Windows.Forms.TextBox syncSleepTime;
        private System.Windows.Forms.GroupBox jiraGroup;
        private System.Windows.Forms.GroupBox togglGroup;
        private System.Windows.Forms.GroupBox advancedGroup;
    }
}