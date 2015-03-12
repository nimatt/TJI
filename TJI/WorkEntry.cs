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
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TJI
{
    class WorkEntry
    {
        private static readonly Regex JiraInfo = new Regex("^(?<issue>[A-ZÅÄÖ]+-[0-9]+)[:]?\\s*(?<comment>.*)$");

        private string LogMarker
        {
            get
            {
                return string.Format("toggl_id:{0}", TogglID);
            }
        }

        public int TogglID { get; private set; }
        public string IssueID { get; private set; }
        public string Comment { get; private set; }
        public DateTime Start { get; private set; }
        public DateTime Updated { get; private set; }
        public int Duration { get; private set; }
        public int DurationInMinutes
        {
            get
            {
                return (Duration + 30) / 60;
            }
        }
        public string TimeSpent
        {
            get
            {
                return DurationInMinutes + "m";
            }
        }

        public string CommentWithMarker
        {
            get
            {
                return string.Format("{0}{1}{1}{2}", Comment, Environment.NewLine, LogMarker);
            }
        }

        private WorkEntry(TogglEntry entry)
        {
            Match info = JiraInfo.Match(entry.Description);

            TogglID = entry.ID;
            IssueID = info.Groups["issue"].Value;
            Comment = info.Groups["comment"].Value;
            Start = DateTime.Parse(entry.Start);
            Updated = DateTime.Parse(entry.At);
            Duration = entry.Duration;
        }

        public static WorkEntry Create(TogglEntry entry)
        {
            return JiraInfo.IsMatch(entry.Description ?? string.Empty) ? new WorkEntry(entry) : null;
        }

        internal JiraWorkEntry FindMatchingEntry(JiraWorklog worklog)
        {
            return worklog.Entries.FirstOrDefault(e => e.Comment.Contains(LogMarker));
        }
    }
}
