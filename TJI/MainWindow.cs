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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TJI
{
    public partial class MainWindow : Form
    {
        Syncronizer syncronizer;

        public MainWindow()
        {
            InitializeComponent();
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

            togglApiToken.Text = syncronizer.Settings.TogglApiToken;
            jiraServerUrl.Text = syncronizer.Settings.JiraServerUrl;
            jiraUsername.Text = syncronizer.Settings.JiraUsername;
            jiraPassword.Text = syncronizer.Settings.JiraPassword;
            syncSleepTime.Text = syncronizer.Settings.SyncIntervall.ToString();
            exceptionPath.Text = ExceptionHandler.ExceptionInfoPath;
            debugCheckbox.Checked = syncronizer.Settings.Debug;

            ExceptionHandler.LogExceptions = syncronizer.Settings.Debug;

            FormClosing += MainWindow_FormClosing;
        }

        void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            syncronizer.Stop();
        }

        void syncronizer_StatusChange(string status)
        {
            TrayIcon.BalloonTipText = status;
            TrayIcon.ShowBalloonTip(3000);
        }

        private void saveSettingsButton_Click(object sender, EventArgs e)
        {
            syncronizer.Settings.TogglApiToken = togglApiToken.Text;
            syncronizer.Settings.JiraServerUrl = jiraServerUrl.Text;
            syncronizer.Settings.JiraUsername = jiraUsername.Text;
            syncronizer.Settings.JiraPassword = jiraPassword.Text;
            syncronizer.Settings.SyncIntervall = int.Parse(syncSleepTime.Text);
            syncronizer.Settings.Debug = debugCheckbox.Checked;

            syncronizer.Settings.Save();
        }

        private void startStopButton_Click(object sender, EventArgs e)
        {
            if (syncronizer.IsRunning)
            {
                startStopButton.Text = "Start";
                syncronizer.Stop();
            }
            else
            {
                startStopButton.Text = "Stop";
                syncronizer.Start();
            }
        }

        private void exceptionPath_TextChanged(object sender, EventArgs e)
        {

        }

        private void debugCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            syncronizer.Settings.Debug = debugCheckbox.Checked;
            ExceptionHandler.LogExceptions = syncronizer.Settings.Debug;
        }
    }
}

