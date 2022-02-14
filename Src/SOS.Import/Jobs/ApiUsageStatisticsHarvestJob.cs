using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Lib.Jobs.Import;
using SOS.Lib.Managers.Interfaces;

namespace SOS.Import.Jobs
{
    /// <summary>
    ///     API usage statistics harvest.
    /// </summary>
    public class ApiUsageStatisticsHarvestJob : IApiUsageStatisticsHarvestJob
    {
        private readonly IApiUsageStatisticsManager _apiUsageStatisticsManager;
        private readonly ILogger<ApiUsageStatisticsHarvestJob> _logger;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="apiUsageStatisticsManager"></param>
        /// <param name="logger"></param>
        public ApiUsageStatisticsHarvestJob(
            IApiUsageStatisticsManager apiUsageStatisticsManager,
            ILogger<ApiUsageStatisticsHarvestJob> logger)
        {
            _apiUsageStatisticsManager = apiUsageStatisticsManager ?? throw new ArgumentNullException(nameof(apiUsageStatisticsManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<bool> RunHarvestStatisticsAsync()
        {
            _logger.LogInformation("Start API usage statistics harvest Job");

            var result = await _apiUsageStatisticsManager.HarvestStatisticsAsync();

            _logger.LogInformation($"End API usage statistics harvest Job. Status: {result}");

            return result;
        }

        /// <inheritdoc />
        public async Task<bool> RunCreateExcelFileReportAsync(string reportId, string createdBy)
        {
            _logger.LogInformation("Start Create API usage statistics Excel file Job");

            var result = await _apiUsageStatisticsManager.CreateExcelFileReportAsync(reportId, createdBy);

            _logger.LogInformation($"End Create API usage statistics Excel file Job. Status: {result}");

            return result;
        }
    }
}