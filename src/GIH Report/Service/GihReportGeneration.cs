namespace Gih_Report.Service
{
    using GIH_Report.Models;
    using Newtonsoft.Json;
    using GIH_Report.Authenticate;
    using GIH_Report.Service;
    using ClosedXML.Excel;
    using System.Globalization;
    using GIH_Report.Constant;

    public class GihReportGeneration
    {
        /// <summary>
        /// get GIH Report
        /// </summary>
        /// <returns></returns>
        public async Task ProccessGIHReportGeneration()
        {
            string filePath = "date.txt";
            // Check if current month and date match the saved value
            bool isMatching = CheckIfCurrentDateMatches(filePath);

            if (isMatching)
            {
                Console.WriteLine("Current month data is already fetched in excel");
            }
            else
            {
                SaveCurrentDate(filePath);
                Console.WriteLine("Current month and date do not match the saved value.");
                var lastMonthDate = GetLastMonthDate();

                var client = Authenticate.AuthenticateUser();

                string jsonString = File.ReadAllText("SquadName.json");
                List<SquadInformation> _allSquadName = JsonConvert.DeserializeObject<List<SquadInformation>>(jsonString);

                string jsonSheetConfig = File.ReadAllText("ExcelConfiguration.json");
                List<ExcelConfig> sheetConfigs = JsonConvert.DeserializeObject<List<ExcelConfig>>(jsonSheetConfig);

                string ExcelName = CreateEmptyExcelFile(sheetConfigs);

                foreach (var squadName in _allSquadName)
                {
                    Console.WriteLine("\n Starting to fetch details for {0}", squadName.ExcelName);

                    var boardDetails = await JiraService.GetBoardId<SquadResult>(client, squadName.Name);
                    if (boardDetails.values.Count == 0)
                    {
                        Console.WriteLine("\n Board with squad name - {0} does not exist", squadName.ExcelName);
                    }
                    var boardId = boardDetails.values.Select(b => b.Id).DefaultIfEmpty(0).Min();

                    var LatestSprints = GetLatestSprints(client, boardId, lastMonthDate);

                    float CompletedStoryPoint = await UpdateStoryExcelReturnStoryPoint(squadName.ExcelName, client, LatestSprints, ExcelName, sheetConfigs.First(), boardId, lastMonthDate);

                    await UpdateSquadExcel(squadName.ExcelName, client, LatestSprints, ExcelName, sheetConfigs.Skip(1).First(), lastMonthDate, boardId, CompletedStoryPoint);
                    Console.WriteLine("\n Completed collecting details for {0}", squadName.ExcelName);
                }

                Console.Write("\n Excel created successfully in output folder");
            }
        }

        /// <summary>
        /// get sprint details by latest order
        /// </summary>  
        /// <param name="client"></param>
        /// <param name="boardId"></param>
        /// <param name="sprintCount"></param>
        /// <returns></returns>
        private List<SprintDetails> GetLatestSprints(HttpClient client, int boardId, string lastMonthDate)
        {
            string[] parts = lastMonthDate.Split('-');
            int month = int.Parse(parts[0]);
            int year = int.Parse(parts[1]);
            bool contFetchingList = true;
            int startAt = 0;
            List<SprintDetails> closedSprintDetails = new List<SprintDetails>();
            do
            {
                SprintResult sprintList = JiraService.GetSprint<SprintResult>(client, boardId, startAt).Result;
                sprintList.Values
                    .Where(sprint => sprint.originBoardId == boardId && int.Parse(DateTime.Parse(sprint.endDate.ToString()).ToString("MM")) == month && int.Parse(DateTime.Parse(sprint.endDate.ToString()).ToString("yyyy")) == year)
                    .ToList()
                    .ForEach(sprint => closedSprintDetails.Add(sprint));

                if (sprintList.isLast)
                {
                    contFetchingList = false;
                }
                else
                {
                    startAt += 50;
                }
            } while (contFetchingList);

            closedSprintDetails.Reverse();
            return closedSprintDetails;
        }

        /// <summary>
        /// update excel with story data
        /// </summary>
        /// <param name="squad"></param>
        /// <param name="client"></param>
        /// <param name="SprintDetails"></param>
        /// <param name="excelName"></param>
        /// <param name="excelConfig"></param>
        /// <returns></returns>
        private async Task<float> UpdateStoryExcelReturnStoryPoint(string squadName, HttpClient client, List<SprintDetails> SprintDetails, string excelName, ExcelConfig excelConfig, int boardid, string lastMonthDate)
        {
            string[] parts = lastMonthDate.Split('-');
            int month = int.Parse(parts[0]);
            float CompletedStoryPoint = 0;
            var SprintVelocity = await JiraService.GetBoardVelocity<SprintVelocity>(client, boardid);
            var SprintVelocityMonth = SprintVelocity.Sprints.Where(sprint => DateTime.ParseExact(sprint.EndDate, "dd/MMM/yy h:mm tt", CultureInfo.InvariantCulture).Month == month);

            foreach (var sprint in SprintVelocityMonth)
            {
                using (var workbook = new XLWorkbook(excelName))
                {
                    IXLWorksheet worksheet = workbook.Worksheet(excelConfig.SheetName);

                    int lastRow = worksheet.LastRowUsed().RowNumber();
                    var newRow = worksheet.Row(lastRow + 1);
                    VelocityStatEntry sprintVelocity = SprintVelocity.VelocityStatEntries[sprint.Id.ToString()];

                    DateTime date = DateTime.ParseExact(sprint.EndDate.ToString(), "dd/MMM/yy h:mm tt", CultureInfo.InvariantCulture);
                    CompletedStoryPoint += sprintVelocity.Completed.Value;
                    int predictability = 0;
                    if (sprintVelocity.Completed.Value != 0 && sprintVelocity.Estimated.Value != 0)
                    {
                        predictability = (int)(sprintVelocity.Completed.Value / sprintVelocity.Estimated.Value * 100);
                    }
                    int[] cellNums = new int[] { 3, 4, 6, 7, 8, 11, 12, 13 };
                    string[] cellValues = new string[] { squadName, sprint.Name, sprintVelocity.Estimated.Value.ToString(), sprintVelocity.Completed.Value.ToString(), predictability.ToString(), date.ToString("MMM-yy"), "Q" + ((date.Month - 1) / 3 + 1).ToString("D"), date.ToString("yyyy") };
                    for (int i = 0; i < cellNums.Length; i++)
                    {
                        newRow.Cell(cellNums[i]).Value = cellValues[i];
                    }
                    newRow.Cell(9).FormulaR1C1 = $"=ROUND((R{newRow.RowNumber()}C7/R{newRow.RowNumber()}C5)*{100}, 0)";
                    workbook.Save();
                }
            }
            return CompletedStoryPoint;
        }
        
        /// <summary>
        /// update excel with story data
        /// </summary>
        /// <param name="squad"></param>
        /// <param name="client"></param>
        /// <param name="SprintDetails"></param>
        /// <param name="excelName"></param>
        /// <param name="excelConfig"></param>
        /// <returns></returns>
        
        private async Task UpdateSquadExcel(string squadName, HttpClient client, List<SprintDetails> SprintDetails, string excelName, ExcelConfig excelConfig, string lastMonthDate, int boardId,float CompletedStoryPoint)
        {
            SquadExcel squadExcelData = new SquadExcel();
            using (var workbook = new XLWorkbook(excelName))
            {

                List<Issue> SprintIssues = new List<Issue>();
                List<int> SprintId = new List<int>();
                int currentSprintId=0;
                foreach (var sprint in SprintDetails)
                {
                    SprintId.Add(sprint.Id);
                    currentSprintId = sprint.Id;
                }
                while (SprintId.Count < 3)
                {
                    SprintId.Add(currentSprintId);
                }
                bool contFetchingList = true;
                int startAt = 0;
                List<IssueResultModel> closedSprintDetails = new List<IssueResultModel>();
                do
                {
                    IssueResultModel sprintList = JiraService.GetIssues<IssueResultModel>(client, boardId, SprintId[0], SprintId[1], SprintId[2], startAt).Result;
                    sprintList.Issues
                        .Where(CurrentSprintIssues => CurrentSprintIssues.Fields?.resolution?.name == "Done" || CurrentSprintIssues.Fields?.resolution?.name == "Closed" || CurrentSprintIssues.Fields?.resolution?.name == "Fixed" || CurrentSprintIssues.Fields?.resolution?.name == "Working as Designed")
                        .ToList()
                        .ForEach(CurrentSprintIssues => SprintIssues.Add(CurrentSprintIssues));
                    
                    if (sprintList.Issues.Count ==0)
                    {
                        contFetchingList = false;
                    }
                    else
                    {
                        startAt += 50;
                    }
                } while (contFetchingList);

                CalculateSquadCols(squadExcelData, SprintIssues);
                CreateSquadCols(client, squadExcelData, workbook, excelConfig, squadName, lastMonthDate, boardId, CompletedStoryPoint);
            }
        }
        /// <summary>
        /// Creating a excel for the squad
        /// </summary>
        /// <param name="squadExcelData"></param>
        /// <param name="workbook"></param>
        /// <param name="excelConfig"></param>
        /// <param name="squadName"></param>
        private async void CreateSquadCols(HttpClient client, SquadExcel squadExcelData, XLWorkbook workbook, ExcelConfig excelConfig, string squadName, string lastMonthDate, int boardId, float CompletedStoryPoint)
        {
            double? researchPercent = squadExcelData.Research / CompletedStoryPoint * 100;
            if (researchPercent.HasValue)
            {
                squadExcelData.ResearchPercent = Math.Round(researchPercent ?? 0, 1);
            }
            double? clientCustomizationPercent = squadExcelData.ClientCustomization / CompletedStoryPoint * 100;
            if (clientCustomizationPercent.HasValue)
            {
                squadExcelData.ClientCustomizationPercent = Math.Round(clientCustomizationPercent ?? 0, 1);
            }
            double? newProductDevelopmentPercent = squadExcelData.NewProductDevelopment / CompletedStoryPoint * 100;
            if (newProductDevelopmentPercent.HasValue)
            {
                squadExcelData.NewProductDevelopmentPercent = Math.Round(newProductDevelopmentPercent ?? 0, 1);
            }
            double? replatformPercent = squadExcelData.Replatform / CompletedStoryPoint * 100;
            if (replatformPercent.HasValue)
            {
                squadExcelData.ReplatformPercent = Math.Round(replatformPercent ?? 0, 1);
            }
            double? maintenancePercent = squadExcelData.Maintenance / CompletedStoryPoint * 100;
            if (maintenancePercent.HasValue)
            {
                squadExcelData.MaintenancePercent = Math.Round(maintenancePercent ?? 0, 1);
            }
            IXLWorksheet worksheet = workbook.Worksheet(excelConfig.SheetName);
            
            string[] parts = lastMonthDate.Split('-');
            int month = int.Parse(parts[0]);
            int year = int.Parse(parts[1]);
            DateTime startDate = new DateTime(year, month, 1);
            var endDateOfSprint = startDate.AddMonths(1).AddDays(-1).ToString("yyyy-MM-dd");
            var startDateOfSprint = startDate.ToString("yyyy-MM-dd");
            List<int> monthDefect = new List<int>();
            foreach (var priority in Constants.Priority)
            {
                monthDefect.Add(JiraService.GetDefects<MonthDefects>(client, priority, startDateOfSprint, endDateOfSprint, boardId).Result.total);
            }
            squadExcelData.TotalProdDefectMonth = monthDefect[0] + monthDefect[1] + monthDefect[2];
            int lastRow = worksheet.LastRowUsed().RowNumber();
            var newRow = worksheet.Row(lastRow + 1);
            int[] cellNumsInner = new int[] { 3, 4, 5, 6, 7, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 };
            string[] cellValuesInner = new string[] { squadName.ToString(), monthDefect[0].ToString(), monthDefect[1].ToString(), monthDefect[2].ToString(), squadExcelData.ProdReleaseCount.ToString(), startDate.ToString("MMM") + "-" + startDate.ToString("yy"), "Q" + ((startDate.Month - 1) / 3 + 1).ToString("D"), startDate.Year.ToString(), squadExcelData.Research.ToString(), squadExcelData.ResearchPercent.ToString(), squadExcelData.ClientCustomization.ToString(), squadExcelData.ClientCustomizationPercent.ToString(), squadExcelData.NewProductDevelopment.ToString(), squadExcelData.NewProductDevelopmentPercent.ToString(), squadExcelData.Replatform.ToString(), squadExcelData.ReplatformPercent.ToString(), squadExcelData.Maintenance.ToString(), squadExcelData.MaintenancePercent.ToString(), CompletedStoryPoint.ToString() };
            for (int i = 0; i < cellNumsInner.Length; i++)
            {
                newRow.Cell(cellNumsInner[i]).Value = cellValuesInner[i];
            }
            workbook.Save();
        }

        /// <summary>
        /// calculate value for the squad 
        /// </summary>
        /// <param name="squadExcelData"></param>
        /// <param name="SprintIssues"></param>
        /// <param name="currMonth"></param>
        /// <param name="preMonth"></param>
        private void CalculateSquadCols(SquadExcel squadExcelData, List<Issue> SprintIssues)
        {
            var priorityCounts = new Dictionary<string, int>();
            foreach (var issue in SprintIssues)
            {
                List<string> priorityNames = new List<string>();
                
                if (issue.Fields?.customfield_14746?.value != null)
                {
                    priorityNames.Add(issue.Fields.customfield_14746.value);
                }
                foreach (var priorityName in priorityNames)
                {
                    
                    switch (priorityName)
                    {
                        case "Research":
                            squadExcelData.Research = (squadExcelData.Research??0) + (issue.Fields?.CompletedStory??0);
                            break;
                        case "Client Customization":
                            squadExcelData.ClientCustomization = (squadExcelData.ClientCustomization??0) + (issue.Fields?.CompletedStory??0);
                            break;
                        case "New Product Development":
                            squadExcelData.NewProductDevelopment = (squadExcelData.NewProductDevelopment??0) + (issue.Fields?.CompletedStory??0);
                            break;
                        case "Replatform":
                            squadExcelData.Replatform = (squadExcelData.Replatform??0) + (issue.Fields?.CompletedStory??0);
                            break;
                        case "Maintenance":
                            squadExcelData.Maintenance = (squadExcelData.Maintenance??0) + (issue.Fields?.CompletedStory??0);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Create an empty excel
        /// </summary>
        /// <param name="sheetConfigs"></param>
        /// <returns></returns>
        private static string CreateEmptyExcelFile(List<ExcelConfig> sheetConfigs)
        {
            string ExcelSheetName = $@"output\GIH_Report.xlsx";

            if (File.Exists(ExcelSheetName))
            {
                return ExcelSheetName;
            }

            using (var workbook = new XLWorkbook())
            {
                foreach (var config in sheetConfigs)
                {
                    var worksheet = workbook.Worksheets.Add(config.SheetName);

                    for (int i = 0; i < config.Column.Count(); i++)
                    {
                        worksheet.Cell(1, i + 1).Value = config.Column[i].Name;
                    }

                    workbook.SaveAs(ExcelSheetName);
                }
            }
            return ExcelSheetName;
        }
        static void SaveCurrentDate(string filePath)
        {
            var monthAndDate = GetLastMonthDate();
            File.WriteAllText(filePath, monthAndDate);
        }

        static bool CheckIfCurrentDateMatches(string filePath)
        {
            if (File.Exists(filePath))
            {
                string savedMonthAndDate = File.ReadAllText(filePath);
                string currentMonthAndDate = GetLastMonthDate();
                return savedMonthAndDate == currentMonthAndDate;
            }

            return false;
        }

        static private string GetLastMonthDate()
        {
            DateTime currentDate = DateTime.Now;
            DateTime lastMonth = currentDate.AddMonths(-1);
            string lastMonthString = lastMonth.ToString("MM-yyyy");

            if (currentDate.Month == 1)
            {
                lastMonthString = lastMonth.AddYears(-1).ToString("MM-yyyy");
            }
            return lastMonthString;
        }
    }
}
