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

        private string _username;
        private string _password;
        private string _serverUrl;

        public JiraClient(string user, string password, string serverUrl)
        {
            _username = user;
            _password = password;
            _serverUrl = serverUrl;
        }

        public JiraWorklog GetIssueWorklog(string issue)
        {
            JiraWorklog worklog = null;

            HttpWebRequest request = GetRequest(string.Format(GET_WORKLOG_URL, issue), true);
            request.Method = "GET";

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(JiraWorklog));
                    worklog = serializer.ReadObject(stream) as JiraWorklog;
                }
            }

            return worklog;
        }

        public void AddWorkEntry(WorkEntry wEntry)
        {
            JiraWorkEntry jEntry = new JiraWorkEntry();
            jEntry.Started = GetStartTime(wEntry);
            jEntry.Comment = wEntry.CommentWithMarker;
            jEntry.TimeSpent = wEntry.TimeSpent;
            
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
                        Console.WriteLine("Created entry for " + wEntry.IssueID);
                    }
                    else
                    {
                        Console.WriteLine("Failed to create entry for " + wEntry.IssueID + ": " + response.StatusCode);
                    }
                }
            }
            catch (WebException we)
            {
                Console.WriteLine("Failed to create entry for " + wEntry.IssueID + ": " + we.Message);
            }

            return;
        }

        private static string GetStartTime(WorkEntry wEntry)
        {
            return wEntry.Start.ToString(TIME_FORMAT) + wEntry.Start.ToString("zzz").Replace(":", "");
        }

        public void SyncWorkEntry(JiraWorkEntry jEntry, WorkEntry wEntry)
        {
            jEntry.TimeSpent = wEntry.TimeSpent;
            jEntry.TimeSpentSeconds = 0;

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
                        Console.WriteLine("Created entry for " + wEntry.IssueID);
                    }
                    else
                    {
                        Console.WriteLine("Failed to update entry for " + wEntry.IssueID + ": " + response.StatusCode);
                    }
                }
            }
            catch (WebException we)
            {
                Console.WriteLine("Failed to update entry for " + wEntry.IssueID + ": " + we.Message);
            }
        }

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
            }
        }

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
