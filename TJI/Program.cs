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
            Settings settings = new Settings();

            if (!settings.HasSettings || args.Contains("-resetSettings"))
            {
                AskUserForSettings(settings);
                settings.Save();
            }

            TogglClient togglClient = new TogglClient(settings.TogglApiToken);
            if (togglClient.LogIn())
            {
                Console.WriteLine("Logged in to Toggl");
            }

            TogglEntry[] togglEntries = togglClient.GetEntries(DateTime.Now.AddDays(-2), DateTime.Now);
            
            List<WorkEntry> workEntries = new List<WorkEntry>();
            foreach (TogglEntry tEntry in togglEntries)
            {
                WorkEntry wEntry = WorkEntry.Create(tEntry);
                if (wEntry != null)
                {
                    workEntries.Add(wEntry);
                }
            }

            IEnumerable<IGrouping<string, WorkEntry>> groupedEntries = from e in workEntries
                                                                       group e by e.IssueID into eg
                                                                       select eg;

            JiraClient jiraClient = new JiraClient(settings.JiraUsername, settings.JiraPassword, settings.JiraServerUrl);
            foreach (IGrouping<string, WorkEntry> entriesForIssue in groupedEntries)
            {
                JiraWorklog worklog = jiraClient.GetIssueWorklog(entriesForIssue.Key);
                foreach (WorkEntry wEntry in entriesForIssue)
                {
                    JiraWorkEntry jEntry = wEntry.FindMatchingEntry(worklog);
                    if (jEntry == null)
                    {
                        jiraClient.AddWorkEntry(wEntry);
                    }
                    else if (jEntry.TimeSpentSeconds != (wEntry.DurationInMinutes * 60))
                    {
                        jiraClient.SyncWorkEntry(jEntry, wEntry);
                    }
                }
            }

            if (togglClient.LogOut())
            {
                Console.WriteLine("Logged out of Toggl");
            }
            Console.ReadKey();
        }

        static void AskUserForSettings(Settings settings)
        {
            Console.WriteLine("Enter missing settings.");

            if (string.IsNullOrEmpty(settings.TogglApiToken))
            {
                string token = GetFromUser("Toggl API Token: ");
                settings.TogglApiToken = token;
            }

            if (string.IsNullOrEmpty(settings.JiraServerUrl))
            {
                string url = GetFromUser("Jira server URL: ");
                settings.JiraServerUrl = url.TrimEnd(new[] { '/' });
            }

            if (string.IsNullOrEmpty(settings.JiraUsername))
            {
                string username = GetFromUser("Jira username: ");
                settings.JiraUsername = username;
            }

            if (string.IsNullOrEmpty(settings.JiraPassword))
            {
                string password = GetFromUser("Jira password: ");
                settings.JiraPassword = password;
            }
        }

        private static string GetFromUser(string message)
        {
            Console.Write(message);
            return Console.ReadLine().Trim();
        }
    }
}
