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
using System.Threading.Tasks;

namespace TJI
{
    class JiraClient
    {
        private static string GET_WORKLOG_URL = "/rest/api/2/issue/{0}/worklog";
        private const string TIME_FORMAT = "yyyy-MM-ddTHH:mm:ss.fff";

        private static readonly ILog log = LogManager.GetLogger(typeof(JiraClient));

        private string _username;
        private string _password;
        private string _serverUrl;

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
            _serverUrl = serverUrl;
        }

        /// <summary>
        /// Retreive the worklog from a specific issue
        /// </summary>
        /// <param name="issue">Issue key or id</param>
        /// <returns>A worklog containing all the work registered in the issue</returns>
        public JiraWorklog GetIssueWorklog(string issue)
        {
            JiraWorklog worklog = null;

            log.DebugFormat("Getting worklog for {0}", issue);
            HttpWebRequest request = GetRequest(string.Format(GET_WORKLOG_URL, issue), true);
            request.Method = "GET";

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        log.Debug("Got an OK from Jira when fetching worklog");
                        DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(JiraWorklog));
                        worklog = serializer.ReadObject(stream) as JiraWorklog;
                        if (worklog != null)
                        {
                            log.Debug("Jira worklog serialzed");
                        }
                    }
                    else
                    {
                        log.WarnFormat("Didn't get an OK from Jira when fetching worklog for {0}, got {1}.", issue, response.StatusCode);
                    }
                }
            }
            catch (WebException we)
            {
                log.Error("Unable to get worklog", we);
                worklog = null;
            }

            return worklog;
        }

        /// <summary>
        /// Adds the gived entry to the worklog of its issue
        /// </summary>
        /// <param name="wEntry">Worklog entry to add</param>
        /// <returns>True if added successfully</returns>
        public bool AddWorkEntry(WorkEntry wEntry)
        {
            JiraWorkEntry jEntry = new JiraWorkEntry();
            jEntry.Started = GetStartTime(wEntry);
            jEntry.Comment = wEntry.CommentWithMarker;
            jEntry.TimeSpent = wEntry.TimeSpent;
            
            log.DebugFormat("Creating a entry for {0} corresponding to {1}.", wEntry.IssueID, wEntry.TogglID);
            HttpWebRequest request = GetRequest(string.Format(GET_WORKLOG_URL, wEntry.IssueID), true);
            request.ContentType = "application/json;charset=UTF-8";
            request.Method = "POST";

            WriteWorkEntry(jEntry, request);

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode == HttpStatusCode.Created)
                    {
                        log.DebugFormat("Work entry created for issue {0}", wEntry.IssueID);
                        return true;
                    }
                    else
                    {
                        log.WarnFormat("Didn't get the expected status code back when creating a work entry for {0}. Got {1}", wEntry.IssueID, response.StatusCode);
                    }
                }
            }
            catch (WebException we)
            {
                log.Error("Unable add work entry", we);
            }

            return false;
        }

        /// <summary>
        /// Returns a string containing the start time of the entry in a correct format for Jira
        /// </summary>
        /// <param name="wEntry">Entry to get start time from</param>
        /// <returns>Start time of the entry in a correctly formatted string</returns>
        private static string GetStartTime(WorkEntry wEntry)
        {
            return wEntry.Start.ToString(TIME_FORMAT) + wEntry.Start.ToString("zzz").Replace(":", "");
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

            log.DebugFormat("Syncronizing {0} in {1}", wEntry.TogglID, wEntry.IssueID);
            HttpWebRequest request = GetRequest(jEntry.Self, false);
            request.ContentType = "application/json;charset=UTF-8";
            request.Method = "PUT";

            WriteWorkEntry(jEntry, request);

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        log.DebugFormat("Syncronized entry {0} in {1}", wEntry.TogglID, wEntry.IssueID);
                        return true;
                    }
                    else
                    {
                        log.WarnFormat("Didn't get the expected status code back when syncing a work entry for {0}. Got {1}", wEntry.IssueID, response.StatusCode);
                    }
                }
            }
            catch (WebException we)
            {
                log.Error("Unable to sync web entry", we);
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
            using (Stream outStream = request.GetRequestStream())
            using (StreamWriter writer = new StreamWriter(outStream))
            using (MemoryStream memStream = new MemoryStream())
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(JiraWorkEntry));
                serializer.WriteObject(memStream, jEntry);
                string jsonData = Encoding.UTF8.GetString(memStream.ToArray());
                writer.Write(jsonData);
                log.Debug("Work entry written to stream");
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
            string userpassB64 = Convert.ToBase64String(Encoding.Default.GetBytes(_username + ":" + _password));
            string authHeader = "Basic " + userpassB64;

            if (relative)
                url = _serverUrl + url;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Headers.Add("Authorization", authHeader);

            return request;
        }
    }
}
