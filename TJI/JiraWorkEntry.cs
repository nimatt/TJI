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
        [DataMember(Name = "self", EmitDefaultValue = false)]
        public string Self { get; set; }

        [DataMember(Name = "author", EmitDefaultValue = false)]
        public JiraUser Author { get; set; }

        [DataMember(Name = "updateAuthor", EmitDefaultValue = false)]
        public JiraUser UpdateAuthor { get; set; }

        [DataMember(Name = "comment", EmitDefaultValue = false)]
        public string Comment { get; set; }

        [DataMember(Name = "created", EmitDefaultValue = false)]
        public string Created { get; set; }

        [DataMember(Name = "started", EmitDefaultValue = false)]
        public string Started { get; set; }

        [DataMember(Name = "timeSpent", EmitDefaultValue = false)]
        public string TimeSpent { get; set; }

        [DataMember(Name = "timeSpentSeconds", EmitDefaultValue = false)]
        public int TimeSpentSeconds { get; set; }

        [DataMember(Name = "id", EmitDefaultValue = false)]
        public int ID { get; set; }
    }
}
