using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SOS.Observations.Api.Managers.Interfaces;

namespace SOS.Observations.Api.HealthChecks
{
    /// <summary>
    /// Health check of data providers
    /// </summary>
    public class DataProviderHealthCheck : IHealthCheck
    {

        private readonly IDataProviderManager _dataProviderManager;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dataProviderManager"></param>
        public DataProviderHealthCheck(IDataProviderManager dataProviderManager)
        {
            _dataProviderManager = dataProviderManager ?? throw new ArgumentNullException(nameof(dataProviderManager));
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
                var dataProviders = await _dataProviderManager.GetDataProvidersAsync(false, "en-GB");
                
                var providerCount = 0;
                var emlCount = 0;
                foreach (var dataProvider in dataProviders)
                {
                    providerCount++;
                    var file = await _dataProviderManager.GetEmlFileAsync(dataProvider.Id);

                    if (file?.Any() ?? false)
                    {
                        emlCount++;
                    }
                }

                if (providerCount.Equals(emlCount))
                {
                    return new HealthCheckResult(HealthStatus.Healthy, "All providers have an EML file");
                }

                if (emlCount > providerCount * 0.9)
                {
                    return new HealthCheckResult(HealthStatus.Degraded, "Almost all providers have an EML file");
                }

                return new HealthCheckResult(HealthStatus.Unhealthy, "To many providers are missing an EML file");
            }
            catch (Exception)
            {
                return new HealthCheckResult(HealthStatus.Unhealthy, "Data provider health check failed");
            }
        }
    }
}
