﻿using Microsoft.Extensions.Diagnostics.HealthChecks;
using SOS.Lib.Repositories.Resource.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

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
        /// <param name="apiUsageStatisticsRepository"></param>
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
                    if ((DateTime.Now - latestHarvestDate.Value).Days > 30)
                    {
                        return new HealthCheckResult(HealthStatus.Degraded, lastHarvestMessage);
                    }

                    if ((DateTime.Now - latestHarvestDate.Value).Days > 25)
                    {
                        return new HealthCheckResult(HealthStatus.Degraded, lastHarvestMessage);
                    }

                    return new HealthCheckResult(HealthStatus.Healthy, lastHarvestMessage);
                }

                return new HealthCheckResult(HealthStatus.Degraded, "No ApplicationInsights harvest has been done");
            }
            catch (Exception)
            {
                return new HealthCheckResult(HealthStatus.Degraded, "ApplicationInsights health check failed");
            }
        }
    }
}
