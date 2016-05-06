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
using System.Windows.Forms;

namespace TJI
{
    public partial class SettingsWindow : Form
    {
        private static readonly Log Logger = Log.GetLogger(typeof(MainWindow));
        private delegate void UpdateStatus(string message, SyncronizerStatus status);

        private readonly TjiSettings _settings;

        public SettingsWindow(TjiSettings settings)
        {
            _settings = settings;
            Logger.Debug("Initializing _settings window");
            InitializeComponent();
            Logger.Debug("Settings window initialized");
        }

        private void SettingsWindow_Load(object sender, EventArgs e)
        {

            jiraServerUrl.Text = _settings.JiraServerUrl;
            jiraUsername.Text = _settings.JiraUsername;
            jiraPassword.Text = _settings.JiraPassword;
            togglApiToken.Text = _settings.TogglApiToken;
            syncSleepTime.Text = _settings.SyncIntervall.ToString();
            daysBack.Text = _settings.DaysBack.ToString();
        }

        private void SettingsUpdated(object sender, EventArgs e)
        {
            bool changeMade = false;

            changeMade |= jiraServerUrl.Text != _settings.JiraServerUrl;
            changeMade |= jiraUsername.Text != _settings.JiraUsername;
            changeMade |= jiraPassword.Text != _settings.JiraPassword;
            changeMade |= togglApiToken.Text != _settings.TogglApiToken;
            changeMade |= syncSleepTime.Text != _settings.SyncIntervall.ToString();
            changeMade |= daysBack.Text != _settings.DaysBack.ToString();

            if (changeMade)
            {
                SaveSettings();
            }
        }

        private void SaveSettings()
        {
            Logger.Debug("Saving _settings");

            _settings.TogglApiToken = togglApiToken.Text;
            _settings.JiraServerUrl = jiraServerUrl.Text;
            _settings.JiraUsername = jiraUsername.Text;
            _settings.JiraPassword = jiraPassword.Text;
            _settings.SyncIntervall = int.Parse(syncSleepTime.Text);
            _settings.DaysBack = int.Parse(daysBack.Text);

            _settings.Save();
            Logger.Debug("Settings saved");
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            SettingsUpdated(sender, e);

            Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}

