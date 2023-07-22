using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GIH_Report.Models
{
    public class Customfield14746
    {
        public string self { get; set; }
        public string value { get; set; }
        public string id { get; set; }
        public bool disabled { get; set; }
    }

    public class Fields
    {
        public string summary { get; set; }
        public Issuetype issuetype { get; set; }
        public Customfield14746 customfield_14746 { get; set; }

        [JsonProperty("customfield_10121")]
        public double? CompletedStory { get; set; }
        [JsonProperty("fixVersions")]
        public List<FixVersion> FixVersions { get; set; }
        [JsonProperty("priority")]
        public Priority Priority { get; set; }
        [JsonProperty("status")]
        public Status Status { get; set; }
        public Resolution resolution { get; set; }
    }

    public class Resolution
    {
        public string self { get; set; }
        public string id { get; set; }
        public string description { get; set; }
        public string name { get; set; }
    }
    public class FixVersion
    {
        public string self { get; set; }  
        public string id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        public bool archived { get; set; }
        public bool released { get; set; }
    }

    public class Issue
    {
        public string expand { get; set; }
        public string id { get; set; }
        public string self { get; set; }
        public string key { get; set; }
        [JsonProperty("fields")]
        public Fields Fields { get; set; }
    }

    public class Issuetype
    {
        public string self { get; set; }
        public string id { get; set; }
        public string description { get; set; }
        public string iconUrl { get; set; }
        public string name { get; set; }
        public bool subtask { get; set; }
        public int avatarId { get; set; }
    }

    public class Priority
    {
        public string self { get; set; }
        public string iconUrl { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        public string id { get; set; }
    }

    public class IssueResultModel
    {
        [JsonProperty("expand")]
        public string Expand { get; set; }
        public int startAt { get; set; }
        public int maxResults { get; set; }
        public int total { get; set; }
        [JsonProperty("issues")]
        public List<Issue> Issues { get; set; }
    }

    public class Status
    {
        public string self { get; set; }
        public string description { get; set; }
        public string iconUrl { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        public string id { get; set; }
        [JsonProperty("statusCategory")]
        public StatusCategory StatusCategory { get; set; }
    }

    public class StatusCategory
    {
        public string self { get; set; }
        public int id { get; set; }
        public string key { get; set; }
        public string colorName { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
    }

}
