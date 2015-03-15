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
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace TJI
{
    public class TJISettings
    {
        public string TogglApiToken { get; set; }
        public string JiraServerUrl { get; set; }
        public string JiraUsername { get; set; }
        public string JiraPassword { get; set; }
        public int SyncIntervall { get; set; }
        public bool Debug { get; set; }

        public TJISettings()
        {
            Properties.Settings loadedSettings = Properties.Settings.Default;

            EnsureSettings(loadedSettings);

            TogglApiToken = loadedSettings.TogglApiToken;
            JiraServerUrl = loadedSettings.JiraServerUrl;
            JiraUsername = loadedSettings.JiraUsername;
            JiraPassword = Decrypt(loadedSettings.JiraPassword);
            SyncIntervall = loadedSettings.SyncIntervall;
            Debug = loadedSettings.Debug;
        }

        private static void EnsureSettings(Properties.Settings loadedSettings)
        {
            // TODO: What to do with settings that has a default?
            if (string.IsNullOrEmpty(loadedSettings.TogglApiToken))
            {
                string setting = "TogglApiToken";
                loadedSettings.TogglApiToken = LoadStringFromPreviousVersion(loadedSettings, setting);
            }
            if (string.IsNullOrEmpty(loadedSettings.JiraServerUrl))
            {
                string setting = "JiraServerUrl";
                loadedSettings.JiraServerUrl = LoadStringFromPreviousVersion(loadedSettings, setting);
            }
            if (string.IsNullOrEmpty(loadedSettings.JiraUsername))
            {
                string setting = "JiraUsername";
                loadedSettings.JiraUsername = LoadStringFromPreviousVersion(loadedSettings, setting);
            }
            if (string.IsNullOrEmpty(loadedSettings.JiraPassword))
            {
                string setting = "JiraPassword";
                loadedSettings.JiraPassword = LoadStringFromPreviousVersion(loadedSettings, setting);
            }
        }

        private static string LoadStringFromPreviousVersion(Properties.Settings loadedSettings, string setting)
        {
            try
            {
                return loadedSettings.GetPreviousVersion(setting) as string ?? string.Empty;
            }
            catch (SettingsPropertyNotFoundException)
            {
                return string.Empty;
            }
        }

        private static T LoadBaseTypeFromPreviousVersion<T>(Properties.Settings loadedSettings, string setting, T defaultValue) where T : struct, IComparable
        {
            try
            {
                T? oldSetting = loadedSettings.GetPreviousVersion(setting) as T?;
                return oldSetting.HasValue ? oldSetting.Value : defaultValue;
            }
            catch (SettingsPropertyNotFoundException)
            {
                return defaultValue;
            }
        }

        public void Save()
        {
            Properties.Settings loadedSettings = Properties.Settings.Default;

            loadedSettings.TogglApiToken = TogglApiToken;
            loadedSettings.JiraServerUrl = JiraServerUrl;
            loadedSettings.JiraUsername = JiraUsername;
            loadedSettings.JiraPassword = Encrypt(JiraPassword);
            loadedSettings.SyncIntervall = SyncIntervall;
            
            loadedSettings.Debug = Debug;

            loadedSettings.Save();
        }

        public bool HasSettings
        {
            get
            {
                return !string.IsNullOrEmpty(TogglApiToken) &&
                    !string.IsNullOrEmpty(JiraServerUrl) &&
                    !string.IsNullOrEmpty(JiraUsername) &&
                    !string.IsNullOrEmpty(JiraPassword);
            }
        }

        private static byte[] Entropy
        {
            get
            {
                return Encoding.Unicode.GetBytes(System.Environment.MachineName);
            }
        }

        private static string Encrypt(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            byte[] encryptedData = ProtectedData.Protect(Encoding.Unicode.GetBytes(input),
                Entropy, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(encryptedData);
        }

        private static string Decrypt(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            byte[] encryptedData = ProtectedData.Unprotect(Convert.FromBase64String(input),
                Entropy, DataProtectionScope.CurrentUser);
            return Encoding.Unicode.GetString(encryptedData);
        }
    }
}
