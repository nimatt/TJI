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
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TJI
{
    public class Syncronizer
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Syncronizer));

        public TJISettings Settings { get; private set; }

        private bool _running = false;
        private Thread _syncThread;
        private DateTime _lastSyncTime = DateTime.MinValue;

        private TogglClient _togglClient;
        private TogglClient Toggl
        {
            get
            {
                if (_togglClient == null && Settings.HasSettings)
                {
                    _togglClient = new TogglClient(Settings.TogglApiToken);
                    _togglClient.LogonSucceded += Toggl_LogonSucceded;
                    _togglClient.LogonFailed += Toggl_LogonFailed;
                    _togglClient.LogoutSucceded += Toggl_LogoutSucceded;
                    _togglClient.LogoutFailed += Toggl_LogoutFailed;
                    _togglClient.FetchedEntries += Toggl_FetchedEntries;
                    _togglClient.FetchingEntriesFailed += Toggl_FetchingEntriesFailed;
                }

                return _togglClient;
            }
            set
            {
                _togglClient = value;
            }
        }

        private JiraClient _jiraClient;
        private JiraClient Jira
        {
            get
            {
                if (_jiraClient == null)
                {
                    _jiraClient = new JiraClient(Settings.JiraUsername, Settings.JiraPassword, Settings.JiraServerUrl);
                    _jiraClient.WorkEntryCreated += Jira_WorkEntryCreated;
                    _jiraClient.WorkEntryCreationFailed += Jira_WorkEntryCreationFailed;
                    _jiraClient.WorkEntryUpdated += Jira_WorkEntryUpdated;
                    _jiraClient.WorkEntryUpdateFailed += Jira_WorkEntryUpdateFailed;
                }

                return _jiraClient;
            }
            set
            {
                _jiraClient = value;
            }
        }

        public bool IsRunning
        {
            get
            {
                return _running && _syncThread != null && _syncThread.IsAlive;
            }
        }

        public SyncronizerStatus Status
        {
            get
            {
                SyncronizerStatus status = SyncronizerStatus.Unknown;

                if (!IsRunning)
                {
                    status = SyncronizerStatus.Stopped;
                }
                else if (Toggl == null || Toggl.EncounteredError || Jira == null || Jira.EncounteredError)
                {
                    status = SyncronizerStatus.Error;
                }
                else if (Toggl.IsLoggedIn)
                {
                    status = SyncronizerStatus.Sleeping;
                }

                return status;
            }
        }

        public event Action<string, SyncronizerStatus> StatusChange;

        private void StatusChangeInternal(string msg)
        {
            StatusChange(msg, Status);
        }

        public Syncronizer()
        {
            Settings = new TJISettings();
        }

        public void Start()
        {
            if (_running)
                return;

            _running = true;

            LogOut();
            Toggl = null;
            Jira = null;

            _syncThread = new Thread((ThreadStart) delegate
            {
                while (_running)
                {
                    try
                    {
                        if (Toggl == null || !Toggl.IsLoggedIn)
                        {
                            LogIn();
                        }

                        if (Toggl != null && Toggl.IsLoggedIn)
                        {
                            DateTime startSyncTime = DateTime.Now;
                            Toggl.GetEntries(startSyncTime.AddDays(-2), startSyncTime);
                        }

                        try
                        {
                            Thread.Sleep(Settings.SyncIntervall * 1000);
                        }
                        catch (ThreadInterruptedException) { }
                    }
                    catch (Exception e)
                    {
                        log.Error("Exception while syncronizing", e);
                    }
                }
            });

            _syncThread.Start();
        }

        public void Stop()
        {
            _running = false;

            if (_syncThread != null && _syncThread.IsAlive)
            {
                _syncThread.Interrupt();
                _syncThread.Join();
            }
        }

        private void LogIn()
        {
            if (Settings.HasSettings)
            {
                Toggl.LogIn();
            }
        }

        private void LogOut()
        {
            if (Toggl != null && Toggl.IsLoggedIn)
            {
                Toggl.LogOut();
            }
        }

        private void SyncTogglEntries(TogglEntry[] togglEntries, DateTime startSyncTime)
        {
            bool succeded = true;
            bool changedIssue = false;
            List<WorkEntry> workEntries = TranslateEntries(togglEntries);

            IEnumerable<IGrouping<string, WorkEntry>> groupedEntries = from e in workEntries
                                                                       where e.Updated > _lastSyncTime
                                                                       group e by e.IssueID into eg
                                                                       select eg;

            JiraClient jiraClient = Jira;
            foreach (IGrouping<string, WorkEntry> entriesForIssue in groupedEntries)
            {
                // TODO: Handle when issues aren't editable
                JiraWorklog worklog = jiraClient.GetIssueWorklog(entriesForIssue.Key);
                if (worklog != null)
                {
                    SyncronizeWorklog(worklog, entriesForIssue);
                    changedIssue = true;
                }
                else
                {
                    StatusChangeInternal("Failed to get Jira issue worklog");
                    log.ErrorFormat("Unable to get worklog for {0}", entriesForIssue.Key);
                    succeded = false;
                }
            }

            if (succeded)
            {
                if (changedIssue)
                {
                    StatusChangeInternal("Syncronized successfully");
                }
                log.Debug("Successfully syncronized systems");
                _lastSyncTime = startSyncTime;
            }
        }

        private static List<WorkEntry> TranslateEntries(TogglEntry[] togglEntries)
        {
            List<WorkEntry> workEntries = new List<WorkEntry>();
            foreach (TogglEntry tEntry in togglEntries)
            {
                WorkEntry wEntry = WorkEntry.Create(tEntry);
                if (wEntry != null)
                {
                    log.DebugFormat("Found work entry in Toggl for {0}", wEntry.IssueID);
                    workEntries.Add(wEntry);
                }
                else
                {
                    log.DebugFormat("Toggl entry is not in valid format{0}{1}", Environment.NewLine, tEntry.Description ?? "<null>");
                }
            }
            return workEntries;
        }

        private void SyncronizeWorklog(JiraWorklog worklog, IEnumerable<WorkEntry> entriesForIssue)
        {
            foreach (WorkEntry wEntry in entriesForIssue)
            {
                JiraWorkEntry jEntry = wEntry.FindMatchingEntry(worklog);
                if (jEntry == null)
                {
                    Jira.AddWorkEntry(wEntry);
                }
                else if (jEntry.TimeSpentSeconds != (wEntry.DurationInMinutes * 60))
                {
                    Jira.SyncWorkEntry(jEntry, wEntry);
                }
            }
        }

        private void Toggl_FetchedEntries(TogglEntry[] entries, DateTime to)
        {
            if (entries != null && entries.Length > 0)
            {
                SyncTogglEntries(entries, to);
            }
        }

        private void Toggl_FetchingEntriesFailed(string errorMsg)
        {
            StatusChangeInternal("Failed to get Toggl entries: " + errorMsg);
            log.Error("Failed to get Toggl entries");
        }

        private void Toggl_LogonSucceded()
        {
            StatusChange("Logged in to Toggl", Status);
            log.Info("Logged in to Toggl");
        }

        private void Toggl_LogonFailed()
        {
            StatusChange("Failed to log in to Toggl", Status);
            log.Warn("Failed to log in to Toggl");
        }

        private void Toggl_LogoutSucceded()
        {
            log.Info("Logged out from Toggl");
        }

        private void Toggl_LogoutFailed()
        {
            StatusChange("Failed to log out from Toggl", Status);
            log.Warn("Failed to log out from Toggl");
        }

        private void Jira_WorkEntryUpdated(WorkEntry wEntry)
        {
            log.InfoFormat("Syncronized entry for {0}", wEntry.IssueID);
            StatusChangeInternal("Updated entry for " + wEntry.IssueID);
        }

        private void Jira_WorkEntryUpdateFailed(WorkEntry wEntry)
        {
            log.ErrorFormat("Failed to syncronize entry for {0}", wEntry.IssueID);
            StatusChangeInternal("Failed to update entry for " + wEntry.IssueID);
        }

        private void Jira_WorkEntryCreated(WorkEntry wEntry)
        {
            log.InfoFormat("Added entry for {0}", wEntry.IssueID);
            StatusChangeInternal("Added entry for " + wEntry.IssueID);
        }

        private void Jira_WorkEntryCreationFailed(WorkEntry wEntry)
        {
            log.ErrorFormat("Failed to add entry for {0}", wEntry.IssueID);
            StatusChangeInternal("Failed to add entry for " + wEntry.IssueID);
        }
    }
}
