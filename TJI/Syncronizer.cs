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

        private TogglClient _togglClient;
        private bool _running = false;
        private Thread _syncThread;
        private DateTime _lastSyncTime = DateTime.MinValue;

        public bool IsRunning
        {
            get
            {
                return _running && _syncThread != null && _syncThread.IsAlive;
            }
        }

        public event Action<string> StatusChange;

        public Syncronizer()
        {
            Settings = new TJISettings();

            if (Settings.HasSettings)
            {
                _togglClient = new TogglClient(Settings.TogglApiToken);
            }
        }

        public void Start()
        {
            if (_running)
                return;

            _running = true;

            LogOut();
            _togglClient = null;

            _syncThread = new Thread((ThreadStart) delegate
            {
                while (_running)
                {
                    try
                    {
                        if (_togglClient == null || !_togglClient.IsLoggedIn)
                        {
                            LogIn();
                        }

                        if (_togglClient != null && _togglClient.IsLoggedIn)
                        {
                            SyncronizeSystems();
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
            if (!Settings.HasSettings)
                return;

            if (_togglClient == null)
            {
                _togglClient = new TogglClient(Settings.TogglApiToken);
            }

            if (_togglClient.LogIn())
            {
                StatusChange("Logged in to Toggl");
                log.Info("Logged in to Toggl");
            }
            else
            {
                StatusChange("Failed to log in to Toggl");
                log.Warn("Failed to log in to Toggl");
            }
        }

        private void LogOut()
        {
            if (_togglClient != null && _togglClient.IsLoggedIn)
            {
                if (_togglClient.LogOut())
                {
                    log.Info("Logged out from Toggl");
                }
                else
                {
                    log.Warn("Failed to log out from Toggl");
                }
            }
        }

        private void SyncronizeSystems()
        {
            bool succeded = true;
            DateTime startSyncTime = DateTime.Now;
            TogglEntry[] togglEntries = _togglClient.GetEntries(startSyncTime.AddDays(-2), startSyncTime);
            if (togglEntries == null)
            {
                StatusChange("Failed to get Toggl entries");
                log.Error("Failed to get Toggl entries");
                return;
            }

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

            IEnumerable<IGrouping<string, WorkEntry>> groupedEntries = from e in workEntries
                                                                       where e.Updated > _lastSyncTime
                                                                       group e by e.IssueID into eg
                                                                       select eg;

            JiraClient jiraClient = new JiraClient(Settings.JiraUsername, Settings.JiraPassword, Settings.JiraServerUrl);
            foreach (IGrouping<string, WorkEntry> entriesForIssue in groupedEntries)
            {
                // TODO: Handle when issues aren't editable
                JiraWorklog worklog = jiraClient.GetIssueWorklog(entriesForIssue.Key);
                if (worklog != null)
                {
                    foreach (WorkEntry wEntry in entriesForIssue)
                    {
                        JiraWorkEntry jEntry = wEntry.FindMatchingEntry(worklog);
                        if (jEntry == null)
                        {
                            if (PerformWebOperation(() => jiraClient.AddWorkEntry(wEntry)))
                            {
                                log.InfoFormat("Added entry for {0}", wEntry.IssueID);
                                StatusChange("Added entry for " + wEntry.IssueID);
                            }
                            else
                            {
                                log.ErrorFormat("Failed to add entry for {0}", wEntry.IssueID);
                                StatusChange("Failed to add entry for " + wEntry.IssueID);
                            }
                        }
                        else if (jEntry.TimeSpentSeconds != (wEntry.DurationInMinutes * 60))
                        {
                            if (PerformWebOperation(() => jiraClient.SyncWorkEntry(jEntry, wEntry)))
                            {
                                log.InfoFormat("Syncronized entry for {0}", wEntry.IssueID);
                                StatusChange("Updated entry for " + wEntry.IssueID);
                            }
                            else
                            {
                                log.ErrorFormat("Failed to syncronize entry for {0}", wEntry.IssueID);
                                StatusChange("Failed to update entry for " + wEntry.IssueID);
                            }
                        }
                    }
                }
                else
                {
                    StatusChange("Failed to get Jira issue worklog");
                    log.ErrorFormat("Unable to get worklog for {0}", entriesForIssue.Key);
                    succeded = false;
                }
            }

            if (succeded)
            {
                log.Debug("Successfully syncronized systems");
                _lastSyncTime = startSyncTime;
            }
        }

        private bool PerformWebOperation(Func<bool> operation)
        {
            try
            {
                return operation();
            }
            catch (Exception e)
            {
                log.Error("Encountered an exception during a web operation", e);
                StatusChange("Exception during web operation");
                return false;
            }
        }
    }
}
