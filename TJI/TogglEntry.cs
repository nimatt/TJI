using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace TJI
{
    [DataContract]
    class TogglEntry
    {
        [DataMember(Name = "id")]
        public int ID { get; set; }

        [DataMember(Name = "guid")]
        public string Guid { get; set; }

        [DataMember(Name = "wid")]
        public int Wid { get; set; }

        [DataMember(Name = "billable")]
        public bool Billable { get; set; }

        [DataMember(Name = "start")]
        public string Start { get; set; }

        [DataMember(Name = "stop")]
        public string Stop { get; set; }

        [DataMember(Name = "duration")]
        public int Duration { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }

        [DataMember(Name = "duronly")]
        public bool Duronly { get; set; }

        [DataMember(Name = "at")]
        public string At { get; set; }

        [DataMember(Name = "uid")]
        public string Uid { get; set; }
    }
}
