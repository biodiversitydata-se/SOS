using System;
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
                var healthTasks = new[]
                {
                    _observationManager.CheckForOccurenceIdDuplicatesAsync(true, false),
                    _observationManager.CheckForOccurenceIdDuplicatesAsync(true, true),
                    _observationManager.CheckForOccurenceIdDuplicatesAsync(false, false),
                    _observationManager.CheckForOccurenceIdDuplicatesAsync(false, true)
                };

                await Task.WhenAll(healthTasks);

                var activePublicIndexHasDuplicates = healthTasks[0].Result;
                var activePublicprotectedIndexHasDuplicates = healthTasks[1].Result;
                var inActivePublicIndexHasDuplicates = healthTasks[2].Result;
                var inActivePublicprotectedIndexHasDuplicates = healthTasks[3].Result;

                var errors = new StringBuilder();
                if (activePublicIndexHasDuplicates)
                {
                    errors.Append("Duplicates found in active public index");
                }

                if (activePublicprotectedIndexHasDuplicates)
                {
                    errors.Append("Duplicates found in active protected index");
                }

                if (inActivePublicIndexHasDuplicates)
                {
                    errors.Append("Duplicates found in inactive public index");
                }

                if (inActivePublicprotectedIndexHasDuplicates)
                {
                    errors.Append("Duplicates found in inactive protected index");
                }

                if (errors.Length != 0)
                {
                    return new HealthCheckResult(HealthStatus.Unhealthy, string.Join(", ", errors));
                }

                return new HealthCheckResult(HealthStatus.Healthy, "No duplicate observations found");
            }
            catch (Exception e)
            {
                return new HealthCheckResult(HealthStatus.Unhealthy, "Duplicate health check failed");
            }
        }
    }
}
