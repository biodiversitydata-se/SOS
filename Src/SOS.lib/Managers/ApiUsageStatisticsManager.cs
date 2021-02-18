using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Lib.Extensions;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Repositories.Resource.Interfaces;
using SOS.Lib.Services.Interfaces;

namespace SOS.Lib.Managers
{
    public class ApiUsageStatisticsManager : IApiUsageStatisticsManager
    {
        private readonly IApiUsageStatisticsRepository _apiUsageStatisticsRepository;
        private readonly IApplicationInsightsService _applicationInsightsService;
        private readonly ILogger<ApiUsageStatisticsManager> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="apiUsageStatisticsRepository"></param>
        /// <param name="applicationInsightsService"></param>
        /// <param name="logger"></param>
        public ApiUsageStatisticsManager(
            IApiUsageStatisticsRepository apiUsageStatisticsRepository, 
            IApplicationInsightsService applicationInsightsService, 
            ILogger<ApiUsageStatisticsManager> logger)
        {
            _apiUsageStatisticsRepository = apiUsageStatisticsRepository ?? throw new ArgumentNullException(nameof(apiUsageStatisticsRepository));
            _applicationInsightsService = applicationInsightsService ?? throw new ArgumentNullException(nameof(applicationInsightsService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> HarvestStatisticsAsync()
        {
            await _apiUsageStatisticsRepository.VerifyCollection();
            DateTime dayToProcess = (await GetLastHarvestDate()).AddDays(1);
            while (dayToProcess < DateTime.Now)
            {
                await ProcessUsageStatisticsForOneDay(dayToProcess);
                dayToProcess = dayToProcess.AddDays(1);
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
    }
}