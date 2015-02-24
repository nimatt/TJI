using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace TJI
{
    public class Settings
    {
        public string TogglApiToken { get; set; }
        public string JiraServerUrl { get; set; }
        public string JiraUsername { get; set; }
        public string JiraPassword { get; set; }

        public Settings()
        {
            Properties.Settings loadedSettings = Properties.Settings.Default;

            TogglApiToken = loadedSettings.TogglApiToken;
            JiraServerUrl = loadedSettings.JiraServerUrl;
            JiraUsername = loadedSettings.JiraUsername;
            JiraPassword = Decrypt(loadedSettings.JiraPassword);
        }

        public void Save()
        {
            Properties.Settings loadedSettings = Properties.Settings.Default;

            loadedSettings.TogglApiToken = TogglApiToken;
            loadedSettings.JiraServerUrl = JiraServerUrl;
            loadedSettings.JiraUsername = JiraUsername;
            loadedSettings.JiraPassword = Encrypt(JiraPassword);

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
