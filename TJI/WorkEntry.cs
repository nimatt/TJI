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
        private static readonly Regex JiraInfo = new Regex("^(?<issue>[A-ZÅÄÖ]+-[0-9]+)[:]?(?<comment>.*)$");

        public int TogglID { get; private set; }
        public string IssueID { get; private set; }
        public string Comment { get; private set; }
        public DateTime Start { get; private set; }

        private WorkEntry(TogglEntry entry)
        {
            Match info = JiraInfo.Match(entry.Description);

            TogglID = entry.ID;
            IssueID = info.Groups["issue"].Value;
            Comment = info.Groups["comment"].Value;
            Start = DateTime.Parse(entry.Start);
        }

        public static WorkEntry Create(TogglEntry entry)
        {
            return JiraInfo.IsMatch(entry.Description) ? new WorkEntry(entry) : null;
        }
    }
}
