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
using System.Threading;
using TJI.Jira;
using TJI.Toggl;

namespace TJI
{
    public class Syncronizer
    {
        private static readonly Log Logger = Log.GetLogger(typeof(Syncronizer));

        public TJISettings Settings { get; }

        private bool _running;
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
                    SetToggleClientStatusChangeCallbacks(_togglClient);
                }

                return _togglClient;
            }
            set
            {
                _togglClient = value;
            }
        }

        private void SetToggleClientStatusChangeCallbacks(TogglClient client)
        {
            client.LogonSucceeded += () => { AlertStatusChange("Logged in to Toggl", Logger.Info); };
            client.LogonFailed += () => { AlertStatusChange("Failed to log in to Toggl", Logger.Warn); };
            client.LogoutSucceeded += () => { AlertStatusChange("Logged out from Toggl", Logger.Info); };
            client.LogoutFailed += () => { AlertStatusChange("Failed to log in to Toggl", Logger.Warn); };
            client.FetchingEntriesFailed += errorMsg => { AlertStatusChange($"Failed to get Toggl entries: {errorMsg}", Logger.Error); };
        }

        private JiraClient _jiraClient;
        private JiraClient Jira
        {
            get
            {
                if (_jiraClient == null)
                {
                    _jiraClient = new JiraClient(Settings.JiraUsername, Settings.JiraPassword, Settings.JiraServerUrl);
                    SetJiraClientStatusChangeCallbacks(_jiraClient);
                }

                return _jiraClient;
            }
            set
            {
                _jiraClient = value;
            }
        }

        private void SetJiraClientStatusChangeCallbacks(JiraClient client)
        {
            client.LogonSucceeded += () => { AlertStatusChange("Logged in to Jira", Logger.Info); };
            client.LogonFailed += () => { AlertStatusChange("Failed to log in to Jira", Logger.Warn); };
            client.LogoutSucceeded += () => { AlertStatusChange("Logged out from Jira", Logger.Info); };
            client.LogoutFailed += () => { AlertStatusChange("Failed to log in to Jira", Logger.Warn); };
            client.WorkEntryCreated += wEntry => { AlertStatusChange($"Added entry for {wEntry.IssueID}", Logger.Info); };
            client.WorkEntryCreationFailed += wEntry => { AlertStatusChange($"Failed to add entry for {wEntry.IssueID}", Logger.Error); };
            client.WorkEntryUpdated += wEntry => { AlertStatusChange($"Updated entry for {wEntry.IssueID}", Logger.Info); };
            client.WorkEntryUpdateFailed += wEntry => { AlertStatusChange($"Failed to update entry for {wEntry.IssueID}", Logger.Error); };
        }

        public bool IsRunning => _running && _syncThread != null && _syncThread.IsAlive;

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
                    status = Jira.IsLoggedIn ? SyncronizerStatus.Sleeping : SyncronizerStatus.Stopped;
                }

                return status;
            }
        }

        public event Action<string, SyncronizerStatus> StatusChange;

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
                        if (Toggl == null || !Toggl.IsLoggedIn || Jira == null || !Jira.IsLoggedIn)
                        {
                            LogIn();
                        }

                        if (Toggl != null && Toggl.IsLoggedIn && Jira != null && Jira.IsLoggedIn)
                        {
                            DateTime startSyncTime = DateTime.Now;
                            var entries = Toggl.GetEntries(startSyncTime.AddDays(-2), startSyncTime);

                            if (entries != null && entries.Any())
                            {
                                SyncTogglEntries(entries, startSyncTime);
                            }
                        }

                        try
                        {
                            Thread.Sleep(Settings.SyncIntervall * 1000);
                        }
                        catch (ThreadInterruptedException) { }
                    }
                    catch (Exception e)
                    {
                        Logger.Error("Exception while syncronizing", e);
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
                try
                {
                    _syncThread.Join(1000);
                }
                catch (Exception e)
                {
                    Logger.Warn("Unable to join with syncronizer thread.", e);
                }
            }

            LogOut();
        }

        private void LogIn()
        {
            if (!Settings.HasSettings)
                return;

            Toggl.LogIn();
            Jira.LogIn();
        }

        private void LogOut()
        {
            if (Toggl != null && Toggl.IsLoggedIn)
            {
                Toggl.LogOut();
            }
            if (Jira != null && Jira.IsLoggedIn)
            {
                Jira.LogOut();
            }
        }

        private void SyncTogglEntries(IEnumerable<TogglEntry> togglEntries, DateTime startSyncTime)
        {
            bool succeeded = true;
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
                    Logger.DebugFormat("Syncronizing worklog for {0}", entriesForIssue.Key);
                    SyncronizeWorklog(worklog, entriesForIssue);
                    changedIssue = true;
                }
                else
                {
                    AlertStatusChange("Failed to get Jira issue worklog", Logger.Warn);
                    Logger.ErrorFormat("Unable to get worklog for {0}", entriesForIssue.Key);
                    succeeded = false;
                }
            }

            if (succeeded)
            {
                if (changedIssue)
                {
                    AlertStatusChange("Syncronized successfully", Logger.Info);
                }
                Logger.Debug("Successfully syncronized systems");
                _lastSyncTime = startSyncTime;
            }
        }

        private static List<WorkEntry> TranslateEntries(IEnumerable<TogglEntry> togglEntries)
        {
            List<WorkEntry> workEntries = new List<WorkEntry>();
            foreach (TogglEntry tEntry in togglEntries)
            {
                WorkEntry wEntry = WorkEntry.Create(tEntry);
                if (wEntry != null)
                {
                    Logger.DebugFormat("Found work entry in Toggl for {0}", wEntry.IssueID);
                    workEntries.Add(wEntry);
                }
                else
                {
                    Logger.DebugFormat("Toggl entry is not in valid format{0}{1}", Environment.NewLine, tEntry.Description ?? "<null>");
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
                else
                {
                    Logger.DebugFormat("Entry {0} in {1} already correct", wEntry.TogglID, wEntry.IssueID);
                }
            }
        }

        private void AlertStatusChange(string message, Action<string> logAction)
        {
            StatusChange?.Invoke(message, Status);
            logAction(message);
        }
    }
}
