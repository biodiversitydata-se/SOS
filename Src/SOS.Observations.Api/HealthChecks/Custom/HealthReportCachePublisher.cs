using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Threading.Tasks;
using System.Threading;

namespace SOS.Observations.Api.HealthChecks.Custom
{
    /// <summary>
    /// This publisher takes a health report and keeps it as "Latest".
    /// Other health checks or endpoints can reuse the latest health report to provide
    /// health check APIs without having the checks executed on each request.
    /// </summary>
    public class HealthReportCachePublisher : IHealthCheckPublisher
    {
        /// <summary>
        /// The latest health report which got published
        /// </summary>
        public static HealthReport Latest { get; set; }

        /// <summary>
        /// Publishes a provided report
        /// </summary>
        /// <param name="report">The result of executing a set of health checks</param>
        /// <param name="cancellationToken">A task which will complete when publishing is complete</param>
        /// <returns></returns>
        public Task PublishAsync(HealthReport report, CancellationToken cancellationToken)
        {
            Latest = report;
            return Task.CompletedTask;
        }
    }
}
