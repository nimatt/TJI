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

        public TJISettings()
        {
            Properties.Settings loadedSettings = Properties.Settings.Default;

            TogglApiToken = loadedSettings.TogglApiToken;
            JiraServerUrl = loadedSettings.JiraServerUrl;
            JiraUsername = loadedSettings.JiraUsername;
            JiraPassword = Decrypt(loadedSettings.JiraPassword);
            SyncIntervall = loadedSettings.SyncIntervall;
        }

        public void Save()
        {
            Properties.Settings loadedSettings = Properties.Settings.Default;

            loadedSettings.TogglApiToken = TogglApiToken;
            loadedSettings.JiraServerUrl = JiraServerUrl;
            loadedSettings.JiraUsername = JiraUsername;
            loadedSettings.JiraPassword = Encrypt(JiraPassword);
            loadedSettings.SyncIntervall = SyncIntervall;

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
