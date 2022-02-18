using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace SOS.Observations.Api.Extensions
{
    public static class HealthCheckExtensions
    {
        /// <summary>
        /// Create health check result
        /// </summary>
        /// <param name="healthStatus"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        public static HealthCheckResult ToHealthCheckResult(this HealthStatus healthStatus, string description = null)
        {
            return new HealthCheckResult(healthStatus, description);
        }
    }
}