namespace GIH_Report.Models
{
    public class SprintVelocity
    {
        public List<Sprint> Sprints { get; set; }
        public Dictionary<string, VelocityStatEntry> VelocityStatEntries { get; set; }
    }

    public class Sprint
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string State { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
    }

    public class VelocityStatEntry
    {
        public EstimatedData Estimated { get; set; }
        public CompletedData Completed { get; set; }
    }

    public class EstimatedData
    {
        public float Value { get; set; }
        public string Text { get; set; }
    }

    public class CompletedData
    {
        public float Value { get; set; }
        public string Text { get; set; }
    }

}
