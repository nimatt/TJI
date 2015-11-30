/*
 * This file is part of TJI.
 * 
 * TJI is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * TJI is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with TJI.  If not, see <http://www.gnu.org/licenses/>.
 */

using log4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

[assembly: log4net.Config.XmlConfigurator(ConfigFile = "TJI.exe.config", Watch = true)]  
namespace TJI
{
    public partial class MainWindow : Form
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(MainWindow));
        private delegate void UpdateStatus(string message, SyncronizerStatus status);

        Syncronizer syncronizer;

        public MainWindow()
        {
            log.Debug("Initializing window");
            InitializeComponent();
            log.Debug("Main window initialized");
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
        }

        private void MainWindow_Resize(object sender, System.EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                Hide();
            }
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            syncronizer = new Syncronizer();
            syncronizer.StatusChange += syncronizer_StatusChange;

            jiraServerUrl.Text = syncronizer.Settings.JiraServerUrl;
            jiraUsername.Text = syncronizer.Settings.JiraUsername;
            jiraPassword.Text = syncronizer.Settings.JiraPassword;
            togglApiToken.Text = syncronizer.Settings.TogglApiToken;
            syncSleepTime.Text = syncronizer.Settings.SyncIntervall.ToString();

            jiraServerUrl.LostFocus += SettingsUpdated;
            jiraUsername.LostFocus += SettingsUpdated;
            jiraPassword.LostFocus += SettingsUpdated;
            togglApiToken.LostFocus += SettingsUpdated;
            syncSleepTime.LostFocus += SettingsUpdated;

            FormClosing += MainWindow_FormClosing;

            if (syncronizer.Settings.HasSettings)
            {
                StartSyncronization();
            }
        }

        private void SettingsUpdated(object sender, EventArgs e)
        {
            bool changeMade = false;

            changeMade |= jiraServerUrl.Text != syncronizer.Settings.JiraServerUrl;
            changeMade |= jiraUsername.Text != syncronizer.Settings.JiraUsername;
            changeMade |= jiraPassword.Text != syncronizer.Settings.JiraPassword;
            changeMade |= togglApiToken.Text != syncronizer.Settings.TogglApiToken;
            changeMade |= syncSleepTime.Text != syncronizer.Settings.SyncIntervall.ToString();

            if (changeMade)
            {
                SaveSettings();
            }
        }

        void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            syncronizer.Stop();
            TrayIcon.Visible = false;
            TrayIcon.Dispose();
        }

        void syncronizer_StatusChange(string message, SyncronizerStatus status)
        {
            if (InvokeRequired)
            {
                Invoke(new UpdateStatus(syncronizer_StatusChange), new object[] {message, status});
                return;
            }

            TrayIcon.BalloonTipText = message;
            TrayIcon.ShowBalloonTip(3000);
            switch (status)
            {
                case SyncronizerStatus.Stopped:
                    SetIcon(TJI.Properties.Resources.StandardIcon);
                    break;
                case SyncronizerStatus.Processing:
                    SetIcon(TJI.Properties.Resources.RunningIcon);
                    break;
                case SyncronizerStatus.Sleeping:
                    SetIcon(TJI.Properties.Resources.RunningIcon);
                    break;
                case SyncronizerStatus.Error:
                    SetIcon(TJI.Properties.Resources.ErrorIcon);
                    break;
                default:
                    SetIcon(TJI.Properties.Resources.ErrorIcon);
                    break;
            }
        }

        private void SetIcon(Icon newIcon)
        {
            TrayIcon.Icon = newIcon;
            Icon = newIcon;
        }

        private void SaveSettings()
        {
            log.Debug("Saving settings");

            syncronizer.Settings.TogglApiToken = togglApiToken.Text;
            syncronizer.Settings.JiraServerUrl = jiraServerUrl.Text;
            syncronizer.Settings.JiraUsername = jiraUsername.Text;
            syncronizer.Settings.JiraPassword = jiraPassword.Text;
            syncronizer.Settings.SyncIntervall = int.Parse(syncSleepTime.Text);

            syncronizer.Settings.Save();
            syncronizer.Stop();
            syncronizer.Start();
            log.Debug("Settings saved");
        }

        private void startStopButton_Click(object sender, EventArgs e)
        {
            if (syncronizer.IsRunning)
            {
                StopSyncronization();
            }
            else
            {
                StartSyncronization();
            }
        }

        private void StopSyncronization()
        {
            log.Info("Stopping syncronization");
            startStopButton.Text = "Start";
            syncronizer.Stop();
        }

        private void StartSyncronization()
        {
            log.Info("Starting syncronization");
            startStopButton.Text = "Stop";
            syncronizer.Start();
        }

        private void minimize_Click(object sender, EventArgs e)
        {
            Hide();
        }
    }
}

