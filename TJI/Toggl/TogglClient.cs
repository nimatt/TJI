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
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using log4net;
using TJI.Communication;

namespace TJI.Toggl
{
    public class TogglClient
    {
        internal const string SessionUrl = "https://www.toggl.com/api/v8/sessions";
        internal const string EntriesUrl = "https://toggl.com/api/v8/time_entries";
        internal const string CookieName = "toggl_api_session_new";
        private const string TimeFormat = "yyyy-MM-ddTHH:mm:sszzz";
        private static readonly Regex CookieContents = new Regex(CookieName + @"=(?<contents>[^;]+);.*Path=(?<path>[^;]+);.*", RegexOptions.Compiled);

        private static readonly Log Logger = Log.GetLogger(typeof(TogglClient));

        private readonly string _apiToken;
        private Cookie _authCookie;

        public event Action LogonSucceeded;
        public event Action LogonFailed;
        
        public event Action LogoutSucceeded;
        public event Action LogoutFailed;

        public event Action<string> FetchingEntriesFailed;

        private IHttpDataSource DataSource { get; }
        
        public bool EncounteredError
        {
            get;
            private set;
        }

        // TODO: Add expiration
        public bool IsLoggedIn => _authCookie != null;

        public TogglClient(string apiToken)
        {
            if (string.IsNullOrEmpty(apiToken))
                throw new ArgumentException("Cannot have an empty api token");
                
            DataSource = new HttpDataSource();

            _apiToken = apiToken;
        }

        internal TogglClient(string apiToken, IHttpDataSource dataSource)
            : this(apiToken)
        {
            DataSource = dataSource;
        }

        public void LogIn()
        {
            string userpass = _apiToken + ":api_token";
            string userpassB64 = Convert.ToBase64String(Encoding.Default.GetBytes(userpass.Trim()));
            string authHeader = "Basic " + userpassB64;

            Logger.Debug("Logger in to Toggl started");
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(SessionUrl);
            request.Headers.Add("Authorization", authHeader);
            request.Method = "POST";

            try
            {
                using (IHttpResponse response = DataSource.GetResponse(request))
                {
                    var headers = response.Headers;
                    if (headers.Keys.Any(k => k.Equals("Set-Cookie")))
                    {
                        Match cookieMatch = CookieContents.Match(headers["Set-Cookie"]);
                        if (cookieMatch.Success)
                        {
                            _authCookie = new Cookie(CookieName, cookieMatch.Groups["contents"].Value, cookieMatch.Groups["path"].Value);
                            Logger.Debug("Logged in to Toggl");
                            EncounteredError = false;
                            LogonSucceeded?.Invoke();
                        }
                        else
                        {
                            Logger.WarnFormat("Response to log in from Toggl didn't contain a Set-Cookie in the format {0}.{1}Instead we got: {2}",
                                CookieContents, Environment.NewLine, headers["Set-Cookie"]);
                            EncounteredError = true;
                            LogonFailed?.Invoke();
                        }
                    }
                    else
                    {
                        Logger.WarnFormat("Response to log in from Toggl didn't contain a Set-Cookie header. Status received is {0}", response.StatusCode);
                        EncounteredError = true;
                        LogonFailed?.Invoke();
                    }
                }
            }
            catch (WebException we)
            {
                Logger.Error("Error during log in to Toggl", we);
                EncounteredError = true;
                LogonFailed?.Invoke();
            }
        }

        public void LogOut()
        {
            if (_authCookie == null)
                return;

            Logger.Debug("Logging out from Toggl");
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(SessionUrl);
            CookieContainer cookieContainer = new CookieContainer();
            cookieContainer.Add(request.RequestUri, _authCookie);
            request.CookieContainer = cookieContainer;
            request.Method = "DELETE";

            try
            {
                using (IHttpResponse response = DataSource.GetResponse(request))
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        EncounteredError = false;
                        _authCookie = null;
                        Logger.Debug("Logged out from Toggl");
                        LogoutSucceeded?.Invoke();
                    }
                    else
                    {
                        EncounteredError = true;
                        Logger.Debug("Failed to log out from Toggl");
                        LogoutFailed?.Invoke();
                    }
                }
            }
            catch (WebException we)
            {
                EncounteredError = true;
                Logger.Error("Error during log out from Toggl", we);
                LogoutFailed?.Invoke();
            }
        }

        public IEnumerable<TogglEntry> GetEntries(DateTime from, DateTime to)
        {
            IEnumerable<TogglEntry> entries = new TogglEntry[0];
            string errorMessage = string.Empty;
            string completeUrl = GetFormattedEntriesUrl(from, to);

            Logger.DebugFormat("Getting entries from Toggl updated between {0} and {1}", GetFormattedTime(from), GetFormattedTime(to));
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(completeUrl);

            CookieContainer cookieContainer = new CookieContainer();
            cookieContainer.Add(request.RequestUri, _authCookie);

            request.CookieContainer = cookieContainer;
            request.Method = "GET";
            request.ContentType = "application/json";

            try
            {
                using (IHttpResponse response = DataSource.GetResponse(request))
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        Logger.Debug("Got a OK when getting entries from Toggl");
                        try
                        {
                            entries = response.GetResponseData<TogglEntry[]>();
                        }
                        catch (FormatException fe)
                        {
                            errorMessage = "Invalid response from server";
                            Logger.Error("Error while parsing Toggl server response", fe);
                        }
                        if (entries == null)
                        {
                            Logger.Error("Serializing Toggl server response gave null");
                            errorMessage = "Invalid response from server";
                        }
                        else if (entries.Any())
                        {
                            Logger.DebugFormat("Got {0} entries from Toggl", entries.Count());
                        }
                        else
                        {
                            Logger.Debug("Got an empty array of entries from Toggl");
                        }
                    }
                    else
                    {
                        errorMessage = $"Server returned '{response.StatusCode}'";
                        Logger.WarnFormat("Did not get an OK when fetching entries from Toggle {0}", response.StatusCode);
                    }
                }
            }
            catch (WebException we)
            {
                errorMessage = we.Message;
                Logger.Error("Exception while getting Toggl entries", we);
            }

            if (entries != null)
            {
                EncounteredError = false;
                entries = RemoveTooShortEntries(entries);
            }
            else
            {
                EncounteredError = true;
                FetchingEntriesFailed?.Invoke(errorMessage);
            }

            return entries;
        }

        internal string GetFormattedEntriesUrl(DateTime from, DateTime to)
        {
            return $"{EntriesUrl}?start_date={GetFormattedTime(from)}&end_date={GetFormattedTime(to)}";
        }

        private static IEnumerable<TogglEntry> RemoveTooShortEntries(IEnumerable<TogglEntry> entries)
        {
            entries = from e in entries
                      where e.Duration > 30
                      select e;

            entries = entries as IList<TogglEntry> ?? entries.ToList();
            if (entries.Any())
            {
                Logger.DebugFormat("{0} entries exceed 30 seconds", entries.Count());
            }
            else
            {
                Logger.Debug("No entries exceeding 30 seconds");
            }
            return entries;
        }

        private string GetFormattedTime(DateTime time)
        {
            return Uri.EscapeDataString(time.ToString(TimeFormat));
        }
    }
}
