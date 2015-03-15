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
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TJI
{
    class TogglClient
    {
        private const string SESSION_URL = "https://www.toggl.com/api/v8/sessions";
        private const string ENTRIES_URL = "https://toggl.com/api/v8/time_entries";
        private const string COOKIE_NAME = "toggl_api_session_new";
        private const string TIME_FORMAT = "yyyy-MM-ddTHH:mm:sszzz";
        private static readonly Regex CookieContents = new Regex(COOKIE_NAME + @"=(?<contents>[^;]+);.*Path=(?<path>[^;]+);.*Domain=(?<domain>[^;]+);", RegexOptions.Compiled);

        private static readonly ILog log = LogManager.GetLogger(typeof(TogglClient));

        private string _apiToken;
        private Cookie authCookie = null;

        public bool IsLoggedIn
        {
            get
            {
                // TODO: Add expiration if possible
                return authCookie != null;
            }
        }

        public TogglClient(string apiToken)
        {
            if (string.IsNullOrEmpty(apiToken))
                throw new ArgumentException("Cannot have an empty api token");
                
            _apiToken = apiToken;
        }

        public bool LogIn()
        {
            string userpass = _apiToken + ":api_token";
            string userpassB64 = Convert.ToBase64String(Encoding.Default.GetBytes(userpass.Trim()));
            string authHeader = "Basic " + userpassB64;

            log.Debug("Log in to Toggl started");
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(SESSION_URL);
            request.Headers.Add("Authorization", authHeader);
            request.Method = "POST";

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    WebHeaderCollection headers = response.Headers;
                    if (headers.AllKeys.Any(k => k.Equals("Set-Cookie")))
                    {
                        Match cookieMatch = CookieContents.Match(headers["Set-Cookie"]);
                        if (cookieMatch.Success)
                        {
                            authCookie = new Cookie(COOKIE_NAME, cookieMatch.Groups["contents"].Value, cookieMatch.Groups["path"].Value, cookieMatch.Groups["domain"].Value);
                            log.Debug("Logged in to Toggl");
                            return true;
                        }
                        else
                        {
                            log.WarnFormat("Response to log in from Toggl didn't contain a Set-Cookie in the format {0}.{1}Instead we got: {2}",
                                CookieContents.ToString(), Environment.NewLine, headers["Set-Cookie"]);
                        }
                    }
                    else
                    {
                        log.WarnFormat("Response to log in from Toggl didn't contain a Set-Cookie header. Status received is {0}", response.StatusCode);
                    }
                }
            }
            catch (WebException we)
            {
                log.Error("Error during log in to Toggl", we);
            }

            return false;
        }

        public bool LogOut()
        {
            if (authCookie == null)
                return false;

            CookieContainer cookieContainer = new CookieContainer();
            cookieContainer.Add(authCookie);

            log.Debug("Logging out from Toggl");
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(SESSION_URL);
            request.CookieContainer = cookieContainer;
            request.Method = "POST";

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        log.Debug("Logged out from Toggl");
                        return true;
                    }
                    else
                    {
                        log.Debug("Failed to log out from Toggl");
                    }
                }
            }
            catch (WebException we)
            {
                log.Error("Error during log out from Toggl", we);
            }

            return false;
        }

        public TogglEntry[] GetEntries(DateTime from, DateTime to)
        {
            TogglEntry[] entries = null;
            string completeUrl = string.Format("{0}?start_date={1}&end_date={2}", ENTRIES_URL, GetFormattedTime(from), GetFormattedTime(to));

            CookieContainer cookieContainer = new CookieContainer();
            cookieContainer.Add(authCookie);

            log.DebugFormat("Getting entries from Toggl updated between {0} and {1}", GetFormattedTime(from), GetFormattedTime(to));
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(completeUrl);
            request.CookieContainer = cookieContainer;
            request.Method = "GET";
            request.ContentType = "application/json";

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        log.Debug("Got a OK when getting entries from Toggl");
                        DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(TogglEntry[]));
                        entries = serializer.ReadObject(stream) as TogglEntry[];
                        if (entries.Length > 0)
                        {
                            log.InfoFormat("Got {0} entries from Toggl", entries.Length);
                        }
                        else
                        {
                            log.Debug("Got an empty array of entries from Toggl");
                        }
                    }
                    else
                    {
                        log.WarnFormat("Did not get an OK when fetching entries from Toggle {0}", response.StatusCode);
                    }
                }
            }
            catch (WebException we)
            {
                log.Error("Exception while getting Toggl entries", we);
            }

            if (entries != null)
            {
                entries = (from e in entries
                           where e.Duration > 30
                           select e).ToArray();

                if (entries.Length > 0)
                {
                    log.InfoFormat("{0} entries exceed 30 seconds", entries.Length);
                }
                else
                {
                    log.Debug("No entries exceeding 30 seconds");
                }
            }

            return entries;
        }

        private string GetFormattedTime(DateTime time)
        {
            return System.Uri.EscapeDataString(time.ToString(TIME_FORMAT));
        }
    }
}
