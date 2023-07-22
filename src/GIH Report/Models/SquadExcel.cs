
namespace GIH_Report.Models
{
    public class SquadExcel
    {
        public string SquadName { get; set; }
        //public int PreMonth { get; set; }
        public int ProdDefectHigh { get; set; }
        public int ProdDefectMedium { get; set; }
        public int ProdDefectLow { get; set; }
        public int TotalProdDefectMonth { get; set; }
        public double? Replatform { get; set; }
        public double? Maintenance { get; set; }
        public double? NewProductDevelopment { get; set; }
        public double? Research { get; set; }
        public double? ClientCustomization { get; set; }
        public double? ResearchPercent { get; set; }
        public double? ClientCustomizationPercent { get; set; }
        public double? NewProductDevelopmentPercent { get; set; }
        public double? ReplatformPercent { get; set; }
        public double? MaintenancePercent { get; set; }
        public double? CompletedStoryPoint { get; set; }
        public int ProdReleaseCount { get; set; }
        public DateTime SprintDate { get; set; }
    }
}
