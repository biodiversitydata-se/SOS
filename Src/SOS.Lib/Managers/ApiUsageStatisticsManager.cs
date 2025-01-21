using Microsoft.Extensions.Logging;
using MongoDB.Driver.GeoJsonObjectModel;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using SOS.Lib.Enums;
using SOS.Lib.Factories;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Resource.Interfaces;
using SOS.Lib.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace SOS.Lib.Managers
{
    public class ApiUsageStatisticsManager : IApiUsageStatisticsManager
    {
        private readonly IApiUsageStatisticsRepository _apiUsageStatisticsRepository;
        private readonly IApplicationInsightsService _applicationInsightsService;
        private readonly IReportManager _reportManager;
        private IApiManagementUserService _apiManagementUserService;
        private readonly IUserService _userService;
        private readonly ILogger<ApiUsageStatisticsManager> _logger;
        const int WfsAvgDownload = 100; // WFS doesn't log observation count.
        const int ObsAvgDownload = 100; // used when the calculation were wrong in some endpoints before 2024-10-15.

        private HashSet<string> _observationDetailEndpoints = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Observations/ObservationsBySearchInternal",
            "Observations/ObservationsBySearch",
            "Observations/ObservationsBySearchDwc",
            "Observations/GetObservationByIdInternal [id]",
            "Observations/GetObservationById [id]",
            "Observations/ObservationByIdDwc [id]",
            "Exports/DownloadGeoJson",
            "Exports/DownloadGeoJsonInternal",
            "Exports/DownloadExcel",
            "Exports/DownloadExcelInternal",
            "Exports/DownloadCsv",
            "Exports/DownloadCsvInternal",
            "Exports/DownloadDwC",
            "Exports/DownloadDwCInternal",
            "Exports/Order/GeoJson",
            "Exports/Order/GeoJsonInternal",
            "Exports/Order/Excel",
            "Exports/Order/ExcelInternal",
            "Exports/Order/Csv",
            "Exports/Order/CsvInternal",
            "Exports/Order/DwC",
            "Exports/Order/DwCInternal"
        };

        private HashSet<string> _wfsEndpoints = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "sos-observation/_search"
        };

        private HashSet<string> _observationEndpointsWithWrongCountBefore20241015 = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Observations/ObservationsBySearchInternal",
            "Observations/ObservationsBySearch",
            "Observations/ObservationsBySearchDwc"
        };

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="apiUsageStatisticsRepository"></param>
        /// <param name="applicationInsightsService"></param>
        /// <param name="reportManager"></param>
        /// <param name="apiManagementUserService"></param>
        /// <param name="userService"></param>
        /// <param name="logger"></param>
        public ApiUsageStatisticsManager(
            IApiUsageStatisticsRepository apiUsageStatisticsRepository,
            IApplicationInsightsService applicationInsightsService,
            IReportManager reportManager,
            IApiManagementUserService apiManagementUserService,
            IUserService userService,
            ILogger<ApiUsageStatisticsManager> logger)
        {
            _apiUsageStatisticsRepository = apiUsageStatisticsRepository ?? throw new ArgumentNullException(nameof(apiUsageStatisticsRepository));
            _applicationInsightsService = applicationInsightsService ?? throw new ArgumentNullException(nameof(applicationInsightsService));
            _reportManager = reportManager ?? throw new ArgumentNullException(nameof(reportManager));
            _apiManagementUserService = apiManagementUserService ??
                                        throw new ArgumentNullException(nameof(apiManagementUserService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> HarvestStatisticsAsync()
        {
            try
            {
                await _apiUsageStatisticsRepository.VerifyCollection();
                var dayToProcess = (await GetLastHarvestDate()).AddDays(1).Date;

                var usageStatisticsFactory = new UsageStatisticsFactory(_apiManagementUserService, _userService);

                while (dayToProcess < DateTime.Now.Date)
                {
                    await ProcessUsageStatisticsForOneDay(dayToProcess, usageStatisticsFactory);
                    dayToProcess = dayToProcess.AddDays(1);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Harvest API usage statistics failed.");
                return false;
            }

            return true;
        }

        public async Task<bool> CreateExcelFileReportAsync(string reportId, string createdBy)
        {
            try
            {
                var excelWriter = new ApiUsageStatisticsExcelWriter(_apiUsageStatisticsRepository);
                var excelFile = await excelWriter.CreateStatisticsExcelFile();
                var report = new Report(reportId)
                {
                    Type = ReportType.ApiUsageStatistics,
                    Name = "API usage statistics",
                    FileExtension = "xlsx",
                    CreatedBy = createdBy ?? "",
                    FileSizeInKb = excelFile.Length / 1024
                };

                await _reportManager.AddReportAsync(report, excelFile);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Create API usage statistics Excel file failed.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Process one day of data
        /// </summary>
        /// <param name="date"></param>
        /// <param name="usageStatisticsFactory"></param>
        /// <returns></returns>
        private async Task ProcessUsageStatisticsForOneDay(DateTime date, UsageStatisticsFactory usageStatisticsFactory)
        {
            _logger.LogInformation($"Start processing Application Insights statistics for date:{date.ToShortDateString()}");
            var nrRows = 0;
            var usageStatisticsRows = await _applicationInsightsService.GetUsageStatisticsForSpecificDayAsync(date);
            if (usageStatisticsRows != null)
            {
                var usageStatisticsEntities = await usageStatisticsFactory.CastToApiUsageStatisticsAsync(usageStatisticsRows);
                await _apiUsageStatisticsRepository.AddManyAsync(usageStatisticsEntities);

                nrRows = usageStatisticsEntities.Count();
            }

            _logger.LogInformation($"End processing Application Insights statistics for date:{date.ToShortDateString()}. Number of rows added: {nrRows}");
        }

        /// <summary>
        /// Get the last harvest date. Will return 91 days back if no entry in database.
        /// </summary>
        /// <returns></returns>
        public async Task<DateTime> GetLastHarvestDate()
        {
            DateTime? latestHarvestDate = await _apiUsageStatisticsRepository.GetLatestHarvestDate();
            if (latestHarvestDate.HasValue) return latestHarvestDate.Value;
            return DateTime.Now.AddDays(-91).Date;
        }

        public async Task<byte[]> CreateRequestStatisticsSummaryExcelFileAsync(DateTime fromDate, DateTime toDate)
        {
            var statisticsByDate = await CalculateRequestStatisticsAsync(fromDate, toDate);
            var summaryExcelWriter = new ApiUsageStatisticsSummaryExcelWriter();
            byte[] excelFile = await summaryExcelWriter.CreateExcelFileAsync(statisticsByDate);
            return excelFile;
        }

        public async Task<Dictionary<string, RequestStatistics>> CalculateRequestStatisticsAsync(DateTime fromDate, DateTime toDate)
        {
            fromDate = new DateTime(fromDate.Year, fromDate.Month, 1);
            toDate = new DateTime(toDate.Year, toDate.Month, DateTime.DaysInMonth(toDate.Year, toDate.Month), 23, 59, 59);
            DateTime current = new DateTime(fromDate.Year, fromDate.Month, DateTime.DaysInMonth(fromDate.Year, fromDate.Month), 23, 59, 59);
            var result = new Dictionary<string, RequestStatistics>();
            long downloadCount = 0;
            using var cursor = await _apiUsageStatisticsRepository.GetAllByCursorAsync();
            while (await cursor.MoveNextAsync())
            {
                foreach (ApiUsageStatistics row in cursor.Current)
                {
                    // Loop through each month
                    while (current <= toDate)
                    {
                        if (row.Date <= current)
                        {
                            DateTime startBetweenDate = new DateTime(current.Year, current.Month, 1);
                            string untilDateKey = $"Until {current:yyyy-MM-dd}";
                            string betweenDateKey = $"{startBetweenDate:yyyy-MM-dd} to {current:yyyy-MM-dd}";
                            if (!result.ContainsKey(untilDateKey))
                                result.Add(untilDateKey, new RequestStatistics());
                            if (!result.ContainsKey(betweenDateKey))
                                result.Add(betweenDateKey, new RequestStatistics());

                            // Observations
                            if (_observationDetailEndpoints.Contains(row.Endpoint))
                            {
                                downloadCount += row.SumResponseCount;

                                result[untilDateKey].ObsevationsRequestCount += row.RequestCount;
                                result[untilDateKey].ObservationsDownloadCount += GetObservationCount(row);

                                string domain = GetDomain(row);
                                if (!result[untilDateKey].StatisticsByDomain.ContainsKey(domain))
                                    result[untilDateKey].StatisticsByDomain.Add(domain, new RequestCountTuple());

                                result[untilDateKey].StatisticsByDomain[domain].RequestCount += row.RequestCount;
                                result[untilDateKey].StatisticsByDomain[domain].DownloadCount += GetObservationCount(row);
                            }

                            if (row.Date >= startBetweenDate && _observationDetailEndpoints.Contains(row.Endpoint))
                            {
                                result[betweenDateKey].ObsevationsRequestCount += row.RequestCount;
                                result[betweenDateKey].ObservationsDownloadCount += GetObservationCount(row);

                                string domain = GetDomain(row);
                                if (!result[betweenDateKey].StatisticsByDomain.ContainsKey(domain))
                                    result[betweenDateKey].StatisticsByDomain.Add(domain, new RequestCountTuple());

                                result[betweenDateKey].StatisticsByDomain[domain].RequestCount += row.RequestCount;
                                result[betweenDateKey].StatisticsByDomain[domain].DownloadCount += GetObservationCount(row);
                            }

                            // WFS
                            if (_wfsEndpoints.Contains(row.Endpoint))
                            {                                
                                result[untilDateKey].WfsRequestCount += row.RequestCount;                                
                                result[untilDateKey].WfsDownloadCount += GetObservationCount(row);
                            }

                            if (row.Date >= startBetweenDate && _wfsEndpoints.Contains(row.Endpoint))
                            {                                
                                result[betweenDateKey].WfsRequestCount += row.RequestCount;                                
                                result[betweenDateKey].WfsDownloadCount += GetObservationCount(row);
                            }
                        }

                        // Move to the last day of the next month
                        current = current.AddDays(1);
                        current = new DateTime(current.Year, current.Month, DateTime.DaysInMonth(current.Year, current.Month), 23, 59, 59);
                    }

                    current = new DateTime(fromDate.Year, fromDate.Month, DateTime.DaysInMonth(fromDate.Year, fromDate.Month), 23, 59, 59);
                }               
            }

            FixWrongObservationCount(result);

            // Sort the domain results
            foreach (var pair in result)
            {
                pair.Value.StatisticsByDomain = pair.Value.StatisticsByDomain.OrderByDescending(x => x.Value.RequestCount).ToDictionary(x => x.Key, x => x.Value);
            }

            return result;
        }

        private void FixWrongObservationCount(Dictionary<string, RequestStatistics> result)
        {
            // WFS logging didn't work between 2024-12-01 and 2025-01-16.
            if (result.TryGetValue("2024-12-01 to 2024-12-31", out var december2024))
            {
                december2024.WfsRequestCount += 700000;
                december2024.WfsDownloadCount += 700000 * WfsAvgDownload;
            }

            if (result.TryGetValue("Until 2024-12-31", out var untilDecember2024))
            {
                untilDecember2024.WfsRequestCount += 700000;
                untilDecember2024.WfsDownloadCount += 700000 * WfsAvgDownload;
            }

            if (result.TryGetValue("2025-01-01 to 2025-01-31", out var january2025))
            {
                january2025.WfsRequestCount += 350000;
                january2025.WfsDownloadCount += 350000 * WfsAvgDownload;
            }

            if (result.TryGetValue("Until 2025-01-31", out var untiljanuary2025))
            {
                untiljanuary2025.WfsRequestCount += 350000;
                untiljanuary2025.WfsDownloadCount += 350000 * WfsAvgDownload;
            }

        }


        private string GetDomain(ApiUsageStatistics row)
        {
            if (!string.IsNullOrEmpty(row.UserEmailDomain)) return row.UserEmailDomain;
            if (!string.IsNullOrEmpty(row.ApiManagementUserEmailDomain)) return row.ApiManagementUserEmailDomain;
            if (!string.IsNullOrEmpty(row.RequestingSystem) && row.RequestingSystem.ToLower() != "na" && row.RequestingSystem.ToLower() != "n/a")
                return row.RequestingSystem;
            return "Unknown";
        }

        private DateTime _wrongCountDate = new DateTime(2024, 10, 15);
        private long GetObservationCount(ApiUsageStatistics row)
        {            
            if (row.Date < _wrongCountDate && _observationEndpointsWithWrongCountBefore20241015.Contains(row.Endpoint))
            {                
                return row.RequestCount * ObsAvgDownload;
            }

            if (_observationDetailEndpoints.Contains(row.Endpoint))
            {                
                return row.SumResponseCount;
            }

            if (_wfsEndpoints.Contains(row.Endpoint))
            {                
                return row.SumResponseCount == 0 ? row.RequestCount * WfsAvgDownload : row.SumResponseCount;
            }

            return 0;
        }

        public class RequestStatistics
        {
            public long ObsevationsRequestCount { get; set; }
            public long ObservationsDownloadCount { get; set; }
            public long WfsRequestCount { get; set; }
            public long WfsDownloadCount {get; set; }
            public long ObsevationsRequestCountIncludingWfsCount => ObsevationsRequestCount + WfsRequestCount;
            public long ObservationsDownloadCountIncludingWfsCount => ObservationsDownloadCount + WfsDownloadCount;            
            public Dictionary<string, RequestCountTuple> StatisticsByDomain { get; set; } = new Dictionary<string, RequestCountTuple>();
        }

        public class RequestCountTuple
        {
            public long RequestCount { get; set; }
            public long DownloadCount { get; set; }
        }

        public class ApiUsageStatisticsSummaryExcelWriter
        {
            public async Task<byte[]> CreateExcelFileAsync(Dictionary<string, RequestStatistics> statisticsByDate)
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using var package = new ExcelPackage();
                if (statisticsByDate != null && statisticsByDate.Count > 0)
                {
                    foreach (var item in statisticsByDate)
                    {
                        CreateWorksheet(package, item.Key, item.Value);
                    }
                }
                else
                {
                    var sheet = package.Workbook.Worksheets.Add("Info");
                    sheet.Cells[1, 1].Value = "There were no data. You are probably using a Test environment.";
                }
                
                byte[] excelBytes = await package.GetAsByteArrayAsync();
                return excelBytes;
            }

            private ExcelWorksheet CreateWorksheet(ExcelPackage package, string title, RequestStatistics requestStatistics)
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(title);
                worksheet.Cells[1, 1].Value = title;
                worksheet.Cells[1, 1].Style.Font.Bold = true;
                worksheet.Cells[1, 1].Style.Font.UnderLine = true;

                worksheet.Cells[2, 1].Value = "# Download requests";
                worksheet.Cells[2, 1].Style.Font.Bold = true;
                worksheet.Cells[2, 2].Value = requestStatistics.ObsevationsRequestCount;
                worksheet.Cells[2, 2].Style.Numberformat.Format = "#,##0";

                worksheet.Cells[3, 1].Value = "# WFS requests";
                worksheet.Cells[3, 1].Style.Font.Bold = true;
                worksheet.Cells[3, 2].Value = requestStatistics.WfsRequestCount;
                worksheet.Cells[3, 2].Style.Numberformat.Format = "#,##0";

                worksheet.Cells[4, 1].Value = "# Sum requests";
                worksheet.Cells[4, 1].Style.Font.Bold = true;
                worksheet.Cells[4, 2].Value = requestStatistics.ObsevationsRequestCountIncludingWfsCount;
                worksheet.Cells[4, 2].Style.Numberformat.Format = "#,##0";

                worksheet.Cells[5, 1].Value = "# Observations downloaded";
                worksheet.Cells[5, 1].Style.Font.Bold = true;
                worksheet.Cells[5, 2].Value = requestStatistics.ObservationsDownloadCount;
                worksheet.Cells[5, 2].Style.Numberformat.Format = "#,##0";

                worksheet.Cells[6, 1].Value = "# WFS observations downloaded";
                worksheet.Cells[6, 1].Style.Font.Bold = true;
                worksheet.Cells[6, 2].Value = requestStatistics.WfsDownloadCount;
                worksheet.Cells[6, 2].Style.Numberformat.Format = "#,##0";

                worksheet.Cells[7, 1].Value = "# Sum observations downloaded";
                worksheet.Cells[7, 1].Style.Font.Bold = true;
                worksheet.Cells[7, 2].Value = requestStatistics.ObservationsDownloadCountIncludingWfsCount;
                worksheet.Cells[7, 2].Style.Numberformat.Format = "#,##0";

                worksheet.Cells[9, 1].Value = "Domain/Application";
                worksheet.Cells[9, 1].Style.Font.Bold = true;
                worksheet.Cells[9, 2].Value = "# Requests";
                worksheet.Cells[9, 2].Style.Font.Bold = true;
                worksheet.Cells[9, 3].Value = "# Obs Downloads";
                worksheet.Cells[9, 3].Style.Font.Bold = true;
                int row = 10;
                foreach (var pair in requestStatistics.StatisticsByDomain)
                {
                    worksheet.Cells[row, 1].Value = pair.Key;                    
                    worksheet.Cells[row, 2].Value = pair.Value.RequestCount;
                    worksheet.Cells[row, 2].Style.Numberformat.Format = "#,##0";
                    worksheet.Cells[row, 3].Value = pair.Value.DownloadCount;
                    worksheet.Cells[row, 3].Style.Numberformat.Format = "#,##0";

                    row++;
                }

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                return worksheet;
            }            
        }

        public class ApiUsageStatisticsExcelWriter
        {
            private readonly IApiUsageStatisticsRepository _apiUsageStatisticsRepository;
            private const int MaxNumberOfExcelRows = 100000; // The maximun number of rows in a excel sheet.
            private const int DateColumnIndex = 1;
            private const int MethodColumnIndex = 2;
            private const int BaseEndpointColumnIndex = 3;
            private const int EndpointColumnIndex = 4;
            private const int AccountIdColumnIndex = 5;
            private const int AzureApiNameColumnIndex = 6;
            private const int AzureApiEmailColumnIndex = 7;
            private const int AzureApiEmailDomainColumnIndex = 8;
            private const int UserIdColumnIndex = 9;
            private const int UserNameColumnIndex = 10;
            private const int UserEmailColumnIndex = 11;
            private const int UserEmailDomainColumnIndex = 12;
            private const int RequestingSystemColumnIndex = 13;
            private const int RequestCountColumnIndex = 14;
            private const int FailureCountColumnIndex = 15;
            private const int AverageDurationColumnIndex = 16;
            private const int SumResponseCountColumnIndex = 17;

            public ApiUsageStatisticsExcelWriter(IApiUsageStatisticsRepository apiUsageStatisticsRepository)
            {
                _apiUsageStatisticsRepository = apiUsageStatisticsRepository ?? throw new ArgumentNullException(nameof(apiUsageStatisticsRepository));
            }

            public async Task<byte[]> CreateStatisticsExcelFile()
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using var package = new ExcelPackage();
                var sheet = package.Workbook.Worksheets.Add("API Usage Statistics");
                AddHeaders(sheet);
                FormatColumns(sheet);
                await AddData(sheet);

                byte[] excelBytes = await package.GetAsByteArrayAsync();
                return excelBytes;
            }

            private async Task AddData(ExcelWorksheet worksheet)
            {
                var rowIndex = 2;

                using var cursor = await _apiUsageStatisticsRepository.GetAllByCursorAsync();
                while (await cursor.MoveNextAsync())
                {

                    foreach (var row in cursor.Current)
                    {
                        worksheet.Cells[rowIndex, DateColumnIndex].Value = row.Date;
                        worksheet.Cells[rowIndex, MethodColumnIndex].Value = row.Method;
                        worksheet.Cells[rowIndex, BaseEndpointColumnIndex].Value = row.BaseEndpoint;
                        worksheet.Cells[rowIndex, EndpointColumnIndex].Value = row.Endpoint;
                        worksheet.Cells[rowIndex, AccountIdColumnIndex].Value = row.AccountId;
                        worksheet.Cells[rowIndex, AzureApiNameColumnIndex].Value = row.ApiManagementUserName;
                        worksheet.Cells[rowIndex, AzureApiEmailColumnIndex].Value = row.ApiManagementUserEmail;
                        worksheet.Cells[rowIndex, AzureApiEmailDomainColumnIndex].Value = row.ApiManagementUserEmailDomain;
                        worksheet.Cells[rowIndex, UserIdColumnIndex].Value = row.UserId;
                        worksheet.Cells[rowIndex, UserNameColumnIndex].Value = row.UserName;
                        worksheet.Cells[rowIndex, UserEmailColumnIndex].Value = row.UserEmail;
                        worksheet.Cells[rowIndex, UserEmailDomainColumnIndex].Value = row.UserEmailDomain;
                        worksheet.Cells[rowIndex, RequestCountColumnIndex].Value = row.RequestCount;
                        worksheet.Cells[rowIndex, FailureCountColumnIndex].Value = row.FailureCount;
                        worksheet.Cells[rowIndex, AverageDurationColumnIndex].Value = row.AverageDuration;
                        worksheet.Cells[rowIndex, SumResponseCountColumnIndex].Value = row.SumResponseCount;
                        worksheet.Cells[rowIndex, RequestingSystemColumnIndex].Value = row.RequestingSystem;

                        rowIndex++;
                    }
                }
            }

            private void FormatColumns(ExcelWorksheet worksheet)
            {
                const int firstDataRow = 2;
                var lastDataRow = MaxNumberOfExcelRows + 1;

                worksheet.Cells[firstDataRow, DateColumnIndex, lastDataRow, DateColumnIndex].Style.Numberformat.Format = "yyyy-MM-dd";
                worksheet.Cells[firstDataRow, RequestCountColumnIndex, lastDataRow, RequestCountColumnIndex].Style.Numberformat.Format = "0";
                worksheet.Cells[firstDataRow, FailureCountColumnIndex, lastDataRow, FailureCountColumnIndex].Style.Numberformat.Format = "0";
                worksheet.Cells[firstDataRow, AverageDurationColumnIndex, lastDataRow, AverageDurationColumnIndex].Style.Numberformat.Format = "0";
            }

            private void AddHeaders(ExcelWorksheet worksheet)
            {
                worksheet.Cells[1, DateColumnIndex].Value = "Date";
                worksheet.Cells[1, MethodColumnIndex].Value = "Method";
                worksheet.Cells[1, BaseEndpointColumnIndex].Value = "BaseEndpoint";
                worksheet.Cells[1, EndpointColumnIndex].Value = "Endpoint";
                worksheet.Cells[1, AccountIdColumnIndex].Value = "AccountId";
                worksheet.Cells[1, AzureApiNameColumnIndex].Value = "AzureApiName";
                worksheet.Cells[1, AzureApiEmailColumnIndex].Value = "AzureApiEmail";
                worksheet.Cells[1, AzureApiEmailDomainColumnIndex].Value = "AzureApiEmailDomain";
                worksheet.Cells[1, UserIdColumnIndex].Value = "UserId";
                worksheet.Cells[1, UserNameColumnIndex].Value = "UserName";
                worksheet.Cells[1, UserEmailColumnIndex].Value = "UserEmail";
                worksheet.Cells[1, UserEmailDomainColumnIndex].Value = "UserEmailDomain";
                worksheet.Cells[1, RequestingSystemColumnIndex].Value = "RequestingSystem";
                worksheet.Cells[1, RequestCountColumnIndex].Value = "RequestCount";
                worksheet.Cells[1, FailureCountColumnIndex].Value = "FailureCount";
                worksheet.Cells[1, AverageDurationColumnIndex].Value = "AverageDuration";
                worksheet.Cells[1, SumResponseCountColumnIndex].Value = "ObservationCount";

                // Format style by columns in first row
                using (var range = worksheet.Cells[1, 1, 1, SumResponseCountColumnIndex])
                {
                    range.Style.Font.Bold = false;
                    range.Style.Font.Color.SetColor(Color.FromArgb(255, 255, 255));
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(79, 129, 189));
                }
            }
        }
    }
}