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

using System.Runtime.Serialization;

namespace TJI.Jira
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
