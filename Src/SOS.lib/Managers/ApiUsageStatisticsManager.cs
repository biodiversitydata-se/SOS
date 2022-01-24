using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using SOS.Lib.Enums;
using SOS.Lib.Factories;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Resource.Interfaces;
using SOS.Lib.Services.Interfaces;

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
            private const int RequestCountColumnIndex = 13;
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
                worksheet.Cells[1, RequestCountColumnIndex].Value = "RequestCount";
                worksheet.Cells[1, FailureCountColumnIndex].Value = "FailureCount";
                worksheet.Cells[1, AverageDurationColumnIndex].Value = "AverageDuration";
                worksheet.Cells[1, SumResponseCountColumnIndex].Value = "SummaryOfResponseCount";

                // Format style by columns in first row
                using (var range = worksheet.Cells[1, 1, 1, AverageDurationColumnIndex])
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