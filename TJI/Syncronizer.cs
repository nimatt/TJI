﻿/*
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
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TJI
{
    public class Syncronizer
    {
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
                    if (_togglClient == null || !_togglClient.IsLoggedIn)
                    {
                        LogIn();
                    }

                    if (_togglClient.IsLoggedIn)
                    {
                        SyncronizeSystems();
                    }

                    try
                    {
                        Thread.Sleep(Settings.SyncIntervall * 1000);
                    }
                    catch (ThreadInterruptedException) { }
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
            }
            else
            {
                StatusChange("Failed to log in to Toggl");
            }
        }

        private void LogOut()
        {
            if (_togglClient != null && _togglClient.IsLoggedIn)
            {
                _togglClient.LogOut();
            }
        }

        private void SyncronizeSystems()
        {
            DateTime startSyncTime = DateTime.Now;
            TogglEntry[] togglEntries = _togglClient.GetEntries(startSyncTime.AddDays(-2), startSyncTime);

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
                                                                       where e.Updated > _lastSyncTime
                                                                       group e by e.IssueID into eg
                                                                       select eg;

            JiraClient jiraClient = new JiraClient(Settings.JiraUsername, Settings.JiraPassword, Settings.JiraServerUrl);
            foreach (IGrouping<string, WorkEntry> entriesForIssue in groupedEntries)
            {
                JiraWorklog worklog = jiraClient.GetIssueWorklog(entriesForIssue.Key);
                foreach (WorkEntry wEntry in entriesForIssue)
                {
                    JiraWorkEntry jEntry = wEntry.FindMatchingEntry(worklog);
                    if (jEntry == null)
                    {
                        StatusChange("Adding entry for " + wEntry.IssueID);
                        jiraClient.AddWorkEntry(wEntry);
                    }
                    else if (jEntry.TimeSpentSeconds != (wEntry.DurationInMinutes * 60))
                    {
                        StatusChange("Updating entry for " + wEntry.IssueID);
                        jiraClient.SyncWorkEntry(jEntry, wEntry);
                    }
                }
            }

            _lastSyncTime = startSyncTime;
        }
    }
}
