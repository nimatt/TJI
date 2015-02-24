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
            Duration = entry.Duration;
        }

        public static WorkEntry Create(TogglEntry entry)
        {
            return JiraInfo.IsMatch(entry.Description) ? new WorkEntry(entry) : null;
        }

        internal JiraWorkEntry FindMatchingEntry(JiraWorklog worklog)
        {
            return worklog.Entries.FirstOrDefault(e => e.Comment.Contains(LogMarker));
        }
    }
}
