using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SOS.Observations.Api.Managers.Interfaces;

namespace SOS.Observations.Api.HealthChecks
{
    /// <summary>
    /// Health check for generated DwC-A files.
    /// </summary>
    public class DwcaHealthCheck : IHealthCheck
    {
        private readonly IBlobStorageManager _blobStorageManager;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="blobStorageManager"></param>
        public DwcaHealthCheck(IBlobStorageManager blobStorageManager)
        {
            _blobStorageManager = blobStorageManager ?? throw new ArgumentNullException(nameof(blobStorageManager));
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
                const int maxDaysOld = 10;
                var files = (await _blobStorageManager.GetExportFilesAsync()).ToArray();
                if (!files.Any())
                {
                    return new HealthCheckResult(HealthStatus.Unhealthy, "All DwC-A files are missing.");
                }

                var artportalenFile = files.FirstOrDefault(m => m.Name == "Artportalen.dwca.zip");
                if (artportalenFile == null)
                {
                    return new HealthCheckResult(HealthStatus.Degraded, "Artportalen DwC-A file is missing.");
                }
                if (artportalenFile.Created <= DateTime.Now.Subtract(TimeSpan.FromDays(maxDaysOld)))
                {
                    return new HealthCheckResult(HealthStatus.Degraded, $"Artportalen DwC-A file is too old. Last created: {artportalenFile.Created}.");
                }

                var sosFile = files.FirstOrDefault(m => m.Name == "sos.dwca.zip");
                if (sosFile == null)
                {
                    return new HealthCheckResult(HealthStatus.Degraded, "SOS DwC-A file is missing.");
                }
                if (sosFile.Created <= DateTime.Now.Subtract(TimeSpan.FromDays(maxDaysOld)))
                {
                    return new HealthCheckResult(HealthStatus.Degraded, $"SOS DwC-A file is too old. Last created: {sosFile.Created}.");
                }

                return new HealthCheckResult(HealthStatus.Healthy, "Artportalen and SOS DwC-A files are up to date.");
            }
            catch (Exception e)
            {
                return new HealthCheckResult(HealthStatus.Unhealthy, "DwC-A health check failed");
            }
        }
    }
}