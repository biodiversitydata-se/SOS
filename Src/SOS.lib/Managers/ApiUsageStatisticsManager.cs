using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
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
        private readonly ILogger<ApiUsageStatisticsManager> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="apiUsageStatisticsRepository"></param>
        /// <param name="applicationInsightsService"></param>
        /// <param name="reportManager"></param>
        /// <param name="logger"></param>
        public ApiUsageStatisticsManager(
            IApiUsageStatisticsRepository apiUsageStatisticsRepository, 
            IApplicationInsightsService applicationInsightsService,
            IReportManager reportManager,
            ILogger<ApiUsageStatisticsManager> logger)
        {
            _apiUsageStatisticsRepository = apiUsageStatisticsRepository ?? throw new ArgumentNullException(nameof(apiUsageStatisticsRepository));
            _applicationInsightsService = applicationInsightsService ?? throw new ArgumentNullException(nameof(applicationInsightsService));
            _reportManager = reportManager ?? throw new ArgumentNullException(nameof(reportManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> HarvestStatisticsAsync()
        {
            try
            {
                await _apiUsageStatisticsRepository.VerifyCollection();
                DateTime dayToProcess = (await GetLastHarvestDate()).AddDays(1);
                while (dayToProcess < DateTime.Now)
                {
                    await ProcessUsageStatisticsForOneDay(dayToProcess);
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

        private async Task ProcessUsageStatisticsForOneDay(DateTime date)
        {
            var usageStatisticsRows = await _applicationInsightsService.GetUsageStatisticsForSpecificDay(date);
            var usageStatisticsEntities = usageStatisticsRows.Select(m => m.ToApiUsageStatistics());
            await _apiUsageStatisticsRepository.AddManyAsync(usageStatisticsEntities);
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
            private const int EndpointColumnIndex = 3;
            private const int RequestCountColumnIndex = 4;
            private const int FailureCountColumnIndex = 5;
            private const int AverageDurationColumnIndex = 6;

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
                int rowIndex = 2;

                using var cursor = await _apiUsageStatisticsRepository.GetAllByCursorAsync();
                while (await cursor.MoveNextAsync())
                {
                    foreach (var row in cursor.Current)
                    {
                        worksheet.Cells[rowIndex, DateColumnIndex].Value = row.Date;
                        worksheet.Cells[rowIndex, MethodColumnIndex].Value = row.Method;
                        worksheet.Cells[rowIndex, EndpointColumnIndex].Value = row.Endpoint;
                        worksheet.Cells[rowIndex, RequestCountColumnIndex].Value = row.RequestCount;
                        worksheet.Cells[rowIndex, FailureCountColumnIndex].Value = row.FailureCount;
                        worksheet.Cells[rowIndex, AverageDurationColumnIndex].Value = row.AverageDuration;

                        rowIndex++;
                    }
                }
            }

            private void FormatColumns(ExcelWorksheet worksheet)
            {
                const int firstDataRow = 2;
                int lastDataRow = MaxNumberOfExcelRows + 1;

                worksheet.Cells[firstDataRow, DateColumnIndex, lastDataRow, DateColumnIndex].Style.Numberformat.Format = "yyyy-MM-dd";
                worksheet.Cells[firstDataRow, RequestCountColumnIndex, lastDataRow, RequestCountColumnIndex].Style.Numberformat.Format = "0";
                worksheet.Cells[firstDataRow, FailureCountColumnIndex, lastDataRow, FailureCountColumnIndex].Style.Numberformat.Format = "0";
                worksheet.Cells[firstDataRow, AverageDurationColumnIndex, lastDataRow, AverageDurationColumnIndex].Style.Numberformat.Format = "0";
            }

            private void AddHeaders(ExcelWorksheet worksheet)
            {
                worksheet.Cells[1, DateColumnIndex].Value = "Date";
                worksheet.Cells[1, MethodColumnIndex].Value = "Method";
                worksheet.Cells[1, EndpointColumnIndex].Value = "Endpoint";
                worksheet.Cells[1, RequestCountColumnIndex].Value = "RequestCount";
                worksheet.Cells[1, FailureCountColumnIndex].Value = "FailureCount";
                worksheet.Cells[1, AverageDurationColumnIndex].Value = "AverageDuration";
                
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