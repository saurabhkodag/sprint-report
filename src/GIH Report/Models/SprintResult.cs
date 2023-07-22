namespace GIH_Report.Models
{
    using Newtonsoft.Json;

    public class SprintDetails
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        public string endDate { get; set; }
        public string startDate { get; set; }
        public int originBoardId { get; set; }

    }

    public class SprintResult
    {
        
        public bool isLast { get; set; }
        [JsonProperty("values")]
        public List<SprintDetails> Values { get; set; }
    }
}
