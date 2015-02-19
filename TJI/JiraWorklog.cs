using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace TJI
{
    [DataContract]
    class JiraWorklog
    {
        [DataMember(Name = "startAt")]
        public int StartAt { get; set; }

        [DataMember(Name = "maxResults")]
        public int MaxResults { get; set; }

        [DataMember(Name = "total")]
        public int Total { get; set; }

        [DataMember(Name = "worklogs")]
        public JiraWorkEntry[] Entries { get; set; }
    }
}
