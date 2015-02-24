using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace TJI
{
    [DataContract]
    public class JiraUser
    {
        [DataMember(Name = "self")]
        public string Self { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "displayName")]
        public string DisplayName { get; set; }

        [DataMember(Name = "active")]
        public bool Active { get; set; }
    }
}
