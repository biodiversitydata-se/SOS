using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SOS.Lib.Repositories.Resource.Interfaces;

namespace SOS.Observations.Api.HealthChecks
{
    /// <summary>
    /// Health check by checking number of documents in index 
    /// </summary>
    public class ApplicationInsightstHealthCheck : IHealthCheck
    {
        private readonly IApiUsageStatisticsRepository _apiUsageStatisticsRepository;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="apiUsageStatisticsManager"></param>
        public ApplicationInsightstHealthCheck(IApiUsageStatisticsRepository apiUsageStatisticsRepository)
        {
            _apiUsageStatisticsRepository = apiUsageStatisticsRepository ?? throw new ArgumentNullException(nameof(apiUsageStatisticsRepository));
        }

        /// <summary>
        /// Make health check
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                var latestHarvestDate = await _apiUsageStatisticsRepository.GetLatestHarvestDate();

                if (latestHarvestDate.HasValue)
                {
                    var lastHarvestMessage = $"Last ApplicationInsights harvest: {latestHarvestDate.Value.ToShortDateString()}";
                    if ((DateTime.Now - latestHarvestDate.Value).Days > 90)
                    {
                        return new HealthCheckResult(HealthStatus.Unhealthy, lastHarvestMessage);
                    }

                    if ((DateTime.Now - latestHarvestDate.Value).Days > 75)
                    {
                        return new HealthCheckResult(HealthStatus.Degraded, lastHarvestMessage);
                    }

                    return new HealthCheckResult(HealthStatus.Healthy, lastHarvestMessage);
                }

                return new HealthCheckResult(HealthStatus.Unhealthy, "No ApplicationInsights harvest has been done");
            }
            catch (Exception e)
            {
                return new HealthCheckResult(HealthStatus.Unhealthy, "ApplicationInsights health check failed");
            }
        }
    }
}
