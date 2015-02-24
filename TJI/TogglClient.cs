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

        private string _apiToken;
        private Cookie authCookie = null;

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

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(SESSION_URL);
            request.Headers.Add("Authorization", authHeader);
            request.Method = "POST";

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                WebHeaderCollection headers = response.Headers;
                Match cookieMatch = CookieContents.Match(response.Headers["Set-Cookie"]);
                if (cookieMatch.Success)
                {
                    authCookie = new Cookie(COOKIE_NAME, cookieMatch.Groups["contents"].Value, cookieMatch.Groups["path"].Value, cookieMatch.Groups["domain"].Value);
                    return true;
                }
            }

            return false;
        }

        public bool LogOut()
        {
            if (authCookie == null)
                return false;

            CookieContainer cookieContainer = new CookieContainer();
            cookieContainer.Add(authCookie);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(SESSION_URL);
            request.CookieContainer = cookieContainer;
            request.Method = "POST";

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return true;
                }
            }

            return false;
        }

        public TogglEntry[] GetEntries(DateTime from, DateTime to)
        {
            TogglEntry[] entries = null;
            string completeUrl = string.Format("{0}?start_date={1}&end_date={2}", ENTRIES_URL, GetFormattedTime(from), GetFormattedTime(to));

            CookieContainer cookieContainer = new CookieContainer();
            cookieContainer.Add(authCookie);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(completeUrl);
            request.CookieContainer = cookieContainer;
            request.Method = "GET";
            request.ContentType = "application/json";

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(TogglEntry[]));
                    entries = serializer.ReadObject(stream) as TogglEntry[];
                }
            }

            if (entries != null)
            {
                entries = (from e in entries
                           where e.Duration > 30
                           select e).ToArray();
            }

            return entries;
        }

        private string GetFormattedTime(DateTime time)
        {
            return System.Uri.EscapeDataString(time.ToString(TIME_FORMAT));
        }
    }
}
