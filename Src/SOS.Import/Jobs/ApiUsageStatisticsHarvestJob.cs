using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Import.Harvesters.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Jobs.Import;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Repositories.Verbatim.Interfaces;

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
        [DisplayName("Harvest API usage statistics")]
        public async Task<bool> RunAsync()
        {
            _logger.LogInformation("Start API usage statistics harvest Job");

            var result = await _apiUsageStatisticsManager.HarvestStatisticsAsync();

            _logger.LogInformation($"End API usage statistics harvest Job. Status: {result}");

            return result;
        }
    }
}