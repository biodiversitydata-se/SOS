using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SOS.Observations.Api.Managers.Interfaces;

namespace SOS.Observations.Api.HealthChecks
{
    /// <summary>
    /// Health check by checking number of documents in index 
    /// </summary>
    public class DuplicateHealthCheck : IHealthCheck
    {
        private readonly IObservationManager _observationManager;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="observationManager"></param>
        public DuplicateHealthCheck(IObservationManager observationManager)
        {
            _observationManager = observationManager ?? throw new ArgumentNullException(nameof(observationManager));
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
                const int maxItems = 20;
                var healthTasks = new[]
                {
                    _observationManager.TryToGetOccurenceIdDuplicatesAsync(false, maxItems),
                    _observationManager.TryToGetOccurenceIdDuplicatesAsync(true, maxItems),
                };

                await Task.WhenAll(healthTasks);

                var activePublicIndexDuplicates = healthTasks[0].Result;
                var activePublicprotectedIndexDuplicates = healthTasks[1].Result;
                bool unhealthy = false;
                bool degraded = false;

                var errors = new StringBuilder();
                if (activePublicIndexDuplicates?.Any() ?? false)
                {
                    errors.Append($"Duplicates found in active public index: { string.Join(", ", activePublicIndexDuplicates) }...");
                    unhealthy = true;
                }

                if (activePublicprotectedIndexDuplicates?.Any() ?? false)
                {
                    errors.Append($"Duplicates found in active protected index: { string.Join(", ", activePublicprotectedIndexDuplicates) }...");
                    unhealthy = true;
                }

                if (unhealthy)
                {
                    return new HealthCheckResult(HealthStatus.Degraded, string.Join(", ", errors));
                }
                if (degraded)
                {
                    return new HealthCheckResult(HealthStatus.Degraded, string.Join(", ", errors));
                }

                return new HealthCheckResult(HealthStatus.Healthy, "No duplicate observations found");
            }
            catch (Exception)
            {
                return new HealthCheckResult(HealthStatus.Degraded, "Duplicate health check failed");
            }
        }
    }
}
