using Microsoft.Extensions.Diagnostics.HealthChecks;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Observations.Api.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SOS.Observations.Api.HealthChecks;

/// <summary>
/// Health check by checking number of documents in index 
/// </summary>
public class IncrementalHarvestHealthCheck : IHealthCheck
{
    private readonly IProcessInfoRepository _processInfoRepository;
    private readonly IProcessedObservationRepository _processedObservationRepository;
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="processInfoRepository"></param>
    /// <param name="processedObservationRepository"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public IncrementalHarvestHealthCheck(
        IProcessInfoRepository processInfoRepository,
        IProcessedObservationRepository processedObservationRepository)
    {
        _processInfoRepository = processInfoRepository ?? throw new ArgumentNullException(nameof(processInfoRepository));
        _processedObservationRepository = processedObservationRepository ?? throw new ArgumentNullException(nameof(processedObservationRepository));
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
            var processInfo = await _processInfoRepository.GetAsync(_processedObservationRepository.UniquePublicIndexName);
            if (processInfo == null) return new HealthCheckResult(HealthStatus.Unhealthy, "ProcessInfo=null.");

            var latestIncrementalHarvest = processInfo.ProvidersInfo.Max(pi => pi.LatestIncrementalEnd);
            if (!latestIncrementalHarvest.HasValue)
            {
                return new HealthCheckResult(
                        HealthStatus.Degraded,
                        "No incremental harvest has run"
                    );
            }

            var data = new Dictionary<string, object> {
                { "Latest incrementa harvest", latestIncrementalHarvest.Value.ToString("yyyy-MM-dd hh:mm:ss") }
            };
            if (latestIncrementalHarvest.Value < DateTime.UtcNow.AddHours(-1))
            {
                return new HealthCheckResult(
                        HealthStatus.Degraded,
                        "More than 1h since incremental harvest last run",
                        data: data
                    );
            }

            return new HealthCheckResult(
                      HealthStatus.Healthy,
                      $"Incremental harvest up to date",
                      data: data
                ); 
        }
        catch (Exception)
        {
            return new HealthCheckResult(HealthStatus.Unhealthy, "Incrementa harvest health check failed");
        }
    }
}