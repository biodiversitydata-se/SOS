using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Observations.Api.Configuration;
using SOS.Observations.Api.Repositories.Interfaces;

namespace SOS.Observations.Api.HealthChecks
{
    /// <summary>
    /// Health check by checking number of documents in index 
    /// </summary>
    public class APDbRestoreHealthCheck : IHealthCheck
    {
        private readonly IProcessInfoRepository _processInfoRepository;
        private readonly IProcessedObservationRepository _processedObservationRepository;
        private readonly HealthCheckConfiguration _healthCheckConfiguration;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="processInfoRepository"></param>
        /// <param name="healthCheckConfiguration"></param>
        public APDbRestoreHealthCheck(
            IProcessInfoRepository processInfoRepository,
            IProcessedObservationRepository processedObservationRepository,
            HealthCheckConfiguration healthCheckConfiguration)
        {
            _processInfoRepository = processInfoRepository ?? throw new ArgumentNullException(nameof(processInfoRepository));
            _processedObservationRepository = processedObservationRepository ?? throw new ArgumentNullException(nameof(processedObservationRepository));
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
                var processInfo = await _processInfoRepository.GetAsync(_processedObservationRepository.UniquePublicIndexName);

                var apInfo = processInfo.ProvidersInfo.FirstOrDefault(pi => pi.DataProviderId.Equals(1));

                var regex = new Regex(@"\d\d\d\d-\d\d-\d\d\s\d\d:\d\d:\d\d");
                var match = regex.Match(apInfo?.HarvestNotes ?? string.Empty);
                if (!DateTime.TryParse(match.Value, out var restoreDate))
                {
                    return new HealthCheckResult(HealthStatus.Unhealthy, "Failed to get latest database backup restore date");
                }

                var data = new Dictionary<string, object> {
                    { "Database restore date", match.ToString() }
                };

                if (_healthCheckConfiguration.ApLatestDbBackupHours.Equals(0) || restoreDate >= DateTime.Now.AddHours(_healthCheckConfiguration.ApLatestDbBackupHours * -1))
                {
                    return new HealthCheckResult(
                           HealthStatus.Healthy,
                           $"Database backup up to date",
                           data: data
                     );
                }
                else if (restoreDate >= DateTime.Now.AddHours(_healthCheckConfiguration.ApLatestDbBackupHours * -2))
                {
                    return new HealthCheckResult(
                            HealthStatus.Degraded,
                            $"Database backup should have been updated. Restore date: {restoreDate}",
                            data: data
                        );
                }

                return new HealthCheckResult(
                            HealthStatus.Unhealthy,
                            $"Database backup to old. Restore date: {restoreDate}",
                            data: data
                      );
            }
            catch (Exception e)
            {
                return new HealthCheckResult(HealthStatus.Unhealthy, "Artportalen database backup restore health check failed");
            }
        }
    }
}