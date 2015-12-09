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
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using TJI.Communication;

namespace TJI.Jira
{
    class JiraClient : SessionClient
    {
        private const string GetWorklogUrl = "/rest/api/2/issue/{0}/worklog";
        private const string TimeFormat = "yyyy-MM-ddTHH:mm:ss.fff";

        private static readonly Log Logger = Log.GetLogger(typeof(JiraClient));

        public override string SessionUrl => $"{_serverUrl}/rest/auth/1/session";
        protected override IHttpDataSource DataSource { get; }
        public override string CookieName => "JSESSIONID";
        protected override string ClientName => "Jira";

        private readonly string _username;
        private readonly string _password;
        private readonly string _serverUrl;

        public bool EncounteredError
        {
            get;
            private set;
        }

        public event Action<WorkEntry> WorkEntryCreated;
        public event Action<WorkEntry> WorkEntryCreationFailed;
        public event Action<WorkEntry> WorkEntryUpdated;
        public event Action<WorkEntry> WorkEntryUpdateFailed;

        /// <summary>
        /// Creates a client object used to interact with a Jira installation
        /// </summary>
        /// <param name="user">The users username in Jira</param>
        /// <param name="password">The users password in Jira</param>
        /// <param name="serverUrl">The absolute url to the Jira instance to interact with</param>
        public JiraClient(string user, string password, string serverUrl)
        {
            _username = user;
            _password = password;
            _serverUrl = serverUrl?.TrimEnd('/');

            LogonFailed += () => { EncounteredError = true; };
            LogonSucceeded += () => { EncounteredError = false; };
            LogoutFailed += () => { EncounteredError = true; };
            LogoutSucceeded += () => { EncounteredError = false; };
            WorkEntryCreationFailed += wEntry => { EncounteredError = true; };
            WorkEntryUpdateFailed += wEntry => { EncounteredError = true; };

            DataSource = new HttpDataSource();
        }

        internal JiraClient(string user, string password, string serverUrl, IHttpDataSource source)
            : this(user, password, serverUrl)
        {
            DataSource = source;
        }

        protected override void AddAuthenticationData(HttpWebRequest request)
        {
            DataSource.WriteRequestData(request, $"{{ \"username\": \"{_username}\", \"password\": \"{_password}\" }}");
        }

        /// <summary>
        /// Retreive the worklog from a specific issue
        /// </summary>
        /// <param name="issue">Issue key or id</param>
        /// <returns>A worklog containing all the work registered in the issue</returns>
        public JiraWorklog GetIssueWorklog(string issue)
        {
            JiraWorklog worklog = null;

            Logger.DebugFormat("Getting worklog for {0}", issue);
            HttpWebRequest request = GetRequest(string.Format(GetWorklogUrl, issue), true);
            request.Method = "GET";

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        Logger.Debug("Got an OK from Jira when fetching worklog");
                        DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(JiraWorklog));
                        if (stream != null)
                        {
                            worklog = serializer.ReadObject(stream) as JiraWorklog;
                        }
                        if (worklog != null)
                        {
                            Logger.Debug("Jira worklog serialzed");
                        }
                    }
                    else
                    {
                        Logger.WarnFormat("Didn't get an OK from Jira when fetching worklog for {0}, got {1}.", issue, response.StatusCode);
                    }
                }
            }
            catch (WebException we)
            {
                Logger.Error("Unable to get worklog", we);
                worklog = null;
            }

            EncounteredError = worklog == null;

            return worklog;
        }

        /// <summary>
        /// Adds the gived entry to the worklog of its issue
        /// </summary>
        /// <param name="wEntry">Worklog entry to add</param>
        /// <returns>True if added successfully</returns>
        public bool AddWorkEntry(WorkEntry wEntry)
        {
            JiraWorkEntry jEntry = new JiraWorkEntry
            {
                Started = GetStartTime(wEntry),
                Comment = wEntry.CommentWithMarker,
                TimeSpent = wEntry.TimeSpent
            };

            Logger.DebugFormat("Creating a entry for {0} corresponding to {1}.", wEntry.IssueId, wEntry.TogglId);
            HttpWebRequest request = GetRequest(string.Format(GetWorklogUrl, wEntry.IssueId), true);
            request.ContentType = "application/json;charset=UTF-8";
            request.Method = "POST";

            try
            {
                WriteWorkEntry(jEntry, request);

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode == HttpStatusCode.Created)
                    {
                        Logger.DebugFormat("Work entry created for issue {0}", wEntry.IssueId);
                        EncounteredError = false;
                        WorkEntryCreated?.Invoke(wEntry);
                        return true;
                    }
                    else
                    {
                        WorkEntryCreationFailed?.Invoke(wEntry);
                        Logger.WarnFormat("Didn't get the expected status code back when creating a work entry for {0}. Got {1}", wEntry.IssueId, response.StatusCode);
                    }
                }
            }
            catch (WebException we)
            {
                WorkEntryCreationFailed?.Invoke(wEntry);
                Logger.Error("Unable add work entry", we);
            }

            EncounteredError = true;
            return false;
        }

        /// <summary>
        /// Returns a string containing the start time of the entry in a correct format for Jira
        /// </summary>
        /// <param name="wEntry">Entry to get start time from</param>
        /// <returns>Start time of the entry in a correctly formatted string</returns>
        private static string GetStartTime(WorkEntry wEntry)
        {
            return wEntry.Start.ToString(TimeFormat) + wEntry.Start.ToString("zzz").Replace(":", "");
        }

        /// <summary>
        /// Syncs the Jira entry with the work entry
        /// </summary>
        /// <param name="jEntry">Jira entry to update</param>
        /// <param name="wEntry">Work entry to update Jira with</param>
        /// <returns>True if updated successfully</returns>
        public bool SyncWorkEntry(JiraWorkEntry jEntry, WorkEntry wEntry)
        {
            jEntry.TimeSpent = wEntry.TimeSpent;
            jEntry.TimeSpentSeconds = 0;

            Logger.DebugFormat("Syncronizing {0} in {1}", wEntry.TogglId, wEntry.IssueId);
            HttpWebRequest request = GetRequest(jEntry.Self, false);
            request.ContentType = "application/json;charset=UTF-8";
            request.Method = "PUT";

            try
            {
                WriteWorkEntry(jEntry, request);

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        EncounteredError = false;
                        WorkEntryUpdated?.Invoke(wEntry);
                        return true;
                    }
                    else
                    {
                        EncounteredError = true;
                        WorkEntryUpdateFailed?.Invoke(wEntry);
                        Logger.WarnFormat("Didn't get the expected status code back when syncing a work entry for {0}. Got {1}", wEntry.IssueId, response.StatusCode);
                    }
                }
            }
            catch (WebException we)
            {
                EncounteredError = true;
                WorkEntryUpdateFailed?.Invoke(wEntry);
                Logger.Error("Unable to sync web entry", we);
            }

            return false;
        }

        /// <summary>
        /// Writes the Jira entry to the request stream
        /// </summary>
        /// <param name="jEntry">Entry to write</param>
        /// <param name="request">Request to write to</param>
        private static void WriteWorkEntry(JiraWorkEntry jEntry, HttpWebRequest request)
        {
            using (StreamWriter writer = new StreamWriter(request.GetRequestStream()))
            using (MemoryStream memStream = new MemoryStream())
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(JiraWorkEntry));
                serializer.WriteObject(memStream, jEntry);
                string jsonData = Encoding.UTF8.GetString(memStream.ToArray());
                writer.Write(jsonData);
                Logger.Debug("Work entry written to stream");
            }
        }

        /// <summary>
        /// Builds a request to be used to get or update information in Jira
        /// </summary>
        /// <param name="url">Url to send the request to</param>
        /// <param name="relative">Set to true if url doesn't contain server base url</param>
        /// <returns>A request with basic configuration</returns>
        private HttpWebRequest GetRequest(string url, bool relative)
        {
            if (relative)
                url = _serverUrl + url;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            CookieContainer cookieContainer = new CookieContainer();
            cookieContainer.Add(request.RequestUri, AuthCookie);
            request.CookieContainer = cookieContainer;

            return request;
        }
    }
}
