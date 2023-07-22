namespace GIH_Report.Service
{
    using GIH_Report.Constant;
    using GIH_Report.Models;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class JiraService
    {
        public async static Task<T> GetBoardId<T>(HttpClient client, string squadName = "WP Seagull")
        {
            var apiUrl = $"/rest/agile/latest/board?name={squadName}";
            HttpResponseMessage response = await client.GetAsync(apiUrl);
            ThrowIfNotSuccess(response);
            return ParseResult<T>(response);
        }

        public async static Task<T> GetBoardVelocity<T>(HttpClient client, int boardId)
        {
            var apiUrl = $"/rest/greenhopper/1.0/rapid/charts/velocity?rapidViewId={boardId}";
            HttpResponseMessage response = await client.GetAsync(apiUrl);
            ThrowIfNotSuccess(response);
            return ParseResult<T>(response);
        }
        
        public async static Task<T> GetSprint<T>(HttpClient client, int boardId, int startAt)
        {
            var apiUrl = $"/rest/agile/latest/board/{boardId}/sprint?startAt={startAt}&state=closed";
            HttpResponseMessage response = await client.GetAsync(apiUrl);
            ThrowIfNotSuccess(response);
            return ParseResult<T>(response);
        }

        public async static Task<T> GetDefects<T>(HttpClient client, string priority, string startDateOfSprint, string endDateOfSprint,int boardId)
        {
            var apiUrl = $"rest/agile/1.0/board/{boardId}/issue?jql=issueType=Bug AND Priority={priority} AND createdDate >= {startDateOfSprint} AND createdDate <= {endDateOfSprint}";
            
            HttpResponseMessage response = await client.GetAsync(apiUrl);
            ThrowIfNotSuccess(response);
            return ParseResult<T>(response);
        }

        public async static Task<T> GetIssues<T>(HttpClient client, int boardId, int sprint1, int sprint2, int sprint3,int startAt)
        {
            var apiUrl = $"/rest/agile/1.0/board/{boardId}/issue?jql=sprint IN ({sprint1},{sprint2},{sprint3})&startAt={startAt}&fields=key,summary,customfield_10121,status,customfield_14746,issuetype,priority,fixVersions,resolution";
            HttpResponseMessage response = await client.GetAsync(apiUrl);
            ThrowIfNotSuccess(response);
            return ParseResult<T>(response);
        }

        public static T ParseResult<T>(HttpResponseMessage response)
        {
            var content = response.Content.ReadAsStringAsync().Result ?? string.Empty;
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(content);
        }

        private static void ThrowIfNotSuccess(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Request failed with status {response.StatusCode}");
                Console.WriteLine("Press Enter to exit...");
                Console.ReadLine();
                Environment.Exit(0);
            }
        }
    }
}
