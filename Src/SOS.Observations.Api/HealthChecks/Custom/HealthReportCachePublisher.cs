using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using HealthChecks.UI.Client;
using System.Collections.Generic;
using System.Collections.ObjectModel;

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
        public static HealthReport LatestAll { get; set; }

        /// <summary>
        /// The latest health report which got published withosut wfs entry
        /// </summary>
        public static HealthReport LatestNoWfs { get; set; }

        /// <summary>
        /// The latest health report which got published with only wfs entry
        /// </summary>
        public static HealthReport LatestOnlyWfs { get; set; }

        /// <summary>
        /// Publishes a provided report
        /// </summary>
        /// <param name="report">The result of executing a set of health checks</param>
        /// <param name="cancellationToken">A task which will complete when publishing is complete</param>
        /// <returns></returns>
        public Task PublishAsync(HealthReport report, CancellationToken cancellationToken)
        {
            LatestAll = report;

            var noWfsEntries = report?.Entries?.Where(e => !e.Key.Equals("WFS"));

            if (noWfsEntries?.Any() ?? false)
            {
                var dictionaryEntries = new Dictionary<string, HealthReportEntry>(noWfsEntries);
                LatestNoWfs = new HealthReport(new ReadOnlyDictionary<string, HealthReportEntry>(dictionaryEntries), new System.TimeSpan(dictionaryEntries.Sum(e => e.Value.Duration.Ticks)) );
            }

            var wfsEntries = report?.Entries?.Where(e => e.Key.Equals("WFS"));

            if (wfsEntries?.Any() ?? false)
            {
                var dictionaryEntries = new Dictionary<string, HealthReportEntry>(wfsEntries);
                LatestOnlyWfs = new HealthReport(new ReadOnlyDictionary<string, HealthReportEntry>(dictionaryEntries), new System.TimeSpan(dictionaryEntries.Sum(e => e.Value.Duration.Ticks)));
            }

            return Task.CompletedTask;
        }
    }
}
