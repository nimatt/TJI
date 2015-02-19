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

            string userpassB64 = Convert.ToBase64String(Encoding.Default.GetBytes(_username + ":" + _password));
            string authHeader = "Basic " + userpassB64;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_serverUrl + string.Format(GET_WORKLOG_URL, issue));
            request.Headers.Add("Authorization", authHeader);
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
    }
}
