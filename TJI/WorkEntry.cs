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
using System.Linq;
using System.Text.RegularExpressions;
using log4net.Repository.Hierarchy;
using TJI.Jira;
using TJI.Toggl;

namespace TJI
{
    internal class WorkEntry
    {
        private static readonly Log Logger = Log.GetLogger(typeof(WorkEntry));

        private static readonly Regex JiraInfo = new Regex("^(?<issue>[A-ZÅÄÖ]+-[0-9]+)[:]?\\s*(?<comment>.*)$");

        private string LogMarker => $"toggl_id:{TogglId}";

        public int TogglId { get; }
        public string IssueId { get; private set; }
        public string Comment { get; }
        public DateTime Start { get; private set; }
        public DateTime Updated { get; private set; }
        public int Duration { get; }
        public int DurationInMinutes => (Duration + 30) / 60;

        public string TimeSpent => DurationInMinutes + "m";

        public string CommentWithMarker => string.Format("{0}{1}{1}{2}", Comment, Environment.NewLine, LogMarker);

        private WorkEntry(TogglEntry entry)
        {
            Match info = JiraInfo.Match(entry.Description);

            TogglId = entry.ID;
            IssueId = info.Groups["issue"].Value;
            Comment = info.Groups["comment"].Value;
            Start = DateTime.Parse(entry.Start);
            Updated = DateTime.Parse(entry.At);
            Duration = entry.Duration;
        }

        public static WorkEntry Create(TogglEntry entry)
        {
            if (entry == null)
                throw new ArgumentException("Entry cannot be null", nameof(entry));
            if (string.IsNullOrEmpty(entry.At))
                throw new ArgumentException("Entry.At cannot be null", nameof(entry.At));
            if (string.IsNullOrEmpty(entry.Start))
                throw new ArgumentException("Entry.Start cannot be null", nameof(entry.Start));

            WorkEntry wEntry = null;
            if (JiraInfo.IsMatch(entry.Description ?? string.Empty))
            {
                try
                {
                    wEntry = new WorkEntry(entry);
                }
                catch (FormatException fe)
                {
                    Logger.Error("Unable to create work entry from Toggl entry", fe);
                }
            }

            return wEntry;
        }

        internal JiraWorkEntry FindMatchingEntry(JiraWorklog worklog)
        {
            return worklog.Entries.FirstOrDefault(e => e.Comment.Contains(LogMarker));
        }
    }
}
