using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace TJI
{
    [DataContract]
    class JiraWorkEntry
    {
        [DataMember(Name = "self")]
        public string Self { get; set; }

        [DataMember(Name = "author")]
        public JiraUser Author { get; set; }

        [DataMember(Name = "updateAuthor")]
        public JiraUser UpdateAuthor { get; set; }

        [DataMember(Name = "comment")]
        public string Comment { get; set; }

        [DataMember(Name = "created")]
        public string Created { get; set; }

        [DataMember(Name = "updated")]
        public string Updated { get; set; }

        [DataMember(Name = "started")]
        public string Started { get; set; }

        [DataMember(Name = "timeSpent")]
        public string timeSpent { get; set; }

        [DataMember(Name = "timeSpentSeconds")]
        public int TimeSpentSeconds { get; set; }

        [DataMember(Name = "id")]
        public int ID { get; set; }
    }
}
