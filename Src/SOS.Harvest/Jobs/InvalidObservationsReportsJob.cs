using Microsoft.Extensions.Logging;
using SOS.Lib.Jobs.Import;
using SOS.Lib.Managers.Interfaces;

namespace SOS.Harvest.Jobs
{
    /// <summary>
    ///     Invalid observations report job.
    /// </summary>
    public class InvalidObservationsReportsJob : IInvalidObservationsReportsJob
    {
        private readonly IInvalidObservationsManager _invalidObservationsManager;
        private readonly ILogger<ApiUsageStatisticsHarvestJob> _logger;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="apiUsageStatisticsManager"></param>
        /// <param name="logger"></param>
        public InvalidObservationsReportsJob(
            IInvalidObservationsManager invalidObservationsManager,
            ILogger<ApiUsageStatisticsHarvestJob> logger)
        {
            _invalidObservationsManager = invalidObservationsManager ?? throw new ArgumentNullException(nameof(invalidObservationsManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }        

        /// <inheritdoc />
        public async Task<bool> RunCreateExcelFileReportAsync(string reportId, int dataProviderId, string createdBy)
        {
            _logger.LogInformation("Start Create API usage statistics Excel file Job");

            var result = await _invalidObservationsManager.CreateExcelFileReportAsync(reportId, dataProviderId, createdBy);

            _logger.LogInformation($"End Create API usage statistics Excel file Job. Status: {result}");

            return result;
        }
    }
}