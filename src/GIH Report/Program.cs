
namespace JiraRestApiExample
{
    using Gih_Report.Service;
    class Program
    {
         static async Task Main(string[] args)
        {
            GihReportGeneration gIHReportGeneration = new GihReportGeneration();
            await gIHReportGeneration.ProccessGIHReportGeneration();
        }

    }
}
