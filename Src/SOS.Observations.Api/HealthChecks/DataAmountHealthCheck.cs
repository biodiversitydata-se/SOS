using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SOS.Observations.Api.Managers.Interfaces;
using SOS.Lib.Configuration.ObservationApi;

namespace SOS.Observations.Api.HealthChecks
{
    /// <summary>
    /// Health check by checking number of documents in index 
    /// </summary>
    public class DataAmountHealthCheck : IHealthCheck
    {
        private readonly IObservationManager _observationManager;
        private readonly HealthCheckConfiguration _healthCheckConfiguration;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="observationManager"></param>
        /// <param name="healthCheckConfiguration"></param>
        public DataAmountHealthCheck(IObservationManager observationManager, HealthCheckConfiguration healthCheckConfiguration)
        {
            _observationManager = observationManager ?? throw new ArgumentNullException(nameof(observationManager));
            _healthCheckConfiguration = healthCheckConfiguration ??
                                        throw new ArgumentNullException(nameof(healthCheckConfiguration));
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
                var healthTasks = new[]
                {
                    _observationManager.IndexCountAsync(),
                    _observationManager.IndexCountAsync(true)
                };

                await Task.WhenAll(healthTasks);

                var publicIndexCount = healthTasks[0].Result;
                var protectedIndexCount = healthTasks[1].Result;
                
                // All providers successful
                if (publicIndexCount > _healthCheckConfiguration.PublicObservationCount)
                {
                    if (protectedIndexCount >_healthCheckConfiguration.ProtectedObservationCount)
                    {
                        return new HealthCheckResult(HealthStatus.Healthy, "There is a reasonable amount of observations processed");
                    }

                    return new HealthCheckResult(HealthStatus.Unhealthy, "The protected index seems to have to fem observations");
                }

                return new HealthCheckResult(HealthStatus.Unhealthy, "To few observations processed");
            }
            catch (Exception e)
            {
                return new HealthCheckResult(HealthStatus.Unhealthy, "Data amount health check failed");
            }
        }
    }
}
