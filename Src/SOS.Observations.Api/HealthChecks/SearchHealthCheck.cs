using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace SOS.Observations.Api.HealthChecks
{
    public class SearchHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default(CancellationToken))
        {
            var healthCheckResultHealthy = false;

            if (healthCheckResultHealthy)
            {
                return Task.FromResult(
                    HealthCheckResult.Healthy("A healthy result."));
            }

            return Task.FromResult(
                new HealthCheckResult(HealthStatus.Degraded,
                    "An unhealthy result."));

            return Task.FromResult(
                new HealthCheckResult(context.Registration.FailureStatus,
                "An unhealthy result."));
        }
    }
}
