using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TJI
{
    class Program
    {
        static void Main(string[] args)
        {
            /*
            string url = "https://toggl.com/api/v8/time_entries?start_date=2015-02-16T00%3A00%3A01%2B02%3A00&end_date=2015-02-16T20%3A00%3A00%2B02%3A00";
            string infoUrl = "https://www.toggl.com/api/v8/me";
            string userpass = "d70be60d75602781190f6c9f4545d403" + ":api_token";
            string userpassB64 = Convert.ToBase64String(Encoding.Default.GetBytes(userpass.Trim()));
            string authHeader = "Basic " + userpassB64;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Headers.Add("Authorization", authHeader);
            request.Method = "GET";
            request.ContentType = "application/json";
            */
            /*
            WebRequest request = WebRequest.Create("https://toggl.com/reports/api/v2/summary?workspace_id=539168&user_agent=sheazar86@gmail.com");
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
            CredentialCache credentialCache = new CredentialCache();
            credentialCache.Add(new System.Uri("https://toggl.com/reports/api/v2/summary"), "Basic", new NetworkCredential("d70be60d75602781190f6c9f4545d403", "api_token"));
            request.Credentials = new NetworkCredential("d70be60d75602781190f6c9f4545d403", "api_token");
            request.Method = "GET";
            */
            /*
            using (WebResponse response = request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                {
                    Console.WriteLine(reader.ReadLine());
                }
            }
             */

            TogglClient togglClient = new TogglClient("d70be60d75602781190f6c9f4545d403");
            if (togglClient.LogIn())
            {
                Console.WriteLine("Logged In");
            }

            TogglEntry[] togglEntries = togglClient.GetEntries(DateTime.Today, DateTime.Now);
            
            List<WorkEntry> workEntries = new List<WorkEntry>();
            foreach (TogglEntry tEntry in togglEntries)
            {
                WorkEntry wEntry = WorkEntry.Create(tEntry);
                if (wEntry != null)
                {
                    workEntries.Add(wEntry);
                }
            }


            JiraClient jiraClient = new JiraClient("nima", "", "http://seas0131:8080");
            foreach (WorkEntry wEntry in workEntries)
            {
                jiraClient.GetIssueWorklog(wEntry.IssueID);
            }

            if (togglClient.LogOut())
            {
                Console.WriteLine("Logged Out");
            }
            Console.ReadKey();
        }
    }
}
