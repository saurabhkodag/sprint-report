namespace GIH_Report.Models
{
    using Newtonsoft.Json;

    public class SquadResult
    {
        [JsonProperty("maxResults")]
        public int TotalResults { get; set; }
        [JsonProperty("startAt")]
        public int CurrentStartingIndex { get; set; }
        [JsonProperty("total")]
        public int CurrentEndingIndex { get; set; }
        [JsonProperty("isLast")]
        public bool isLast { get; set; }
        [JsonProperty("values")]
        public List<SquadDetails> values { get; set; }
    }

    public class SquadDetails {

        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
