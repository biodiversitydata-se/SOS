using Microsoft.Extensions.Diagnostics.HealthChecks;
using SOS.Observations.Api.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SOS.Observations.Api.HealthChecks
{
    /// <summary>
    /// Get information about what dependencies are used.
    /// </summary>
    public class DependenciesHealthCheck : IHealthCheck
    {
        private readonly IProcessedObservationRepository _processedObservationRepository;

        /// <summary>
        /// Constructor
        /// </summary>
        public DependenciesHealthCheck(IProcessedObservationRepository processedObservationRepository)
        {
            _processedObservationRepository = processedObservationRepository;
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
            return await Task.Run(() =>
            {
                try
                {
                    int esActiveIndex = _processedObservationRepository.ActiveInstance;
                    var observationIndexName = _processedObservationRepository.PublicIndexName;
                    var processedDbConfiguration = Settings.ProcessDbConfiguration;
                    var elasticConfiguration = Settings.SearchDbConfiguration;
                    var hangfireConfiguration = Settings.HangfireDbConfiguration;
                    var userServiceConfiguration = Settings.UserServiceConfiguration;
                    var esClusterIndex = Math.Min(elasticConfiguration.Clusters.Count() - 1, esActiveIndex);

                    var dependencies = new List<(string Title, string Value)>
                {
                    ("MongoDb", $"{processedDbConfiguration.Hosts.First().Name} ({processedDbConfiguration.DatabaseName})"),
                    ("Hangfire", $"{hangfireConfiguration.Hosts.First().Name} ({hangfireConfiguration.DatabaseName})"),
                    ("Elasticsearch", $"{elasticConfiguration.Clusters.ElementAt(esClusterIndex).Hosts.First()} ({observationIndexName})"),
                    ("ArtdataUserService",$"{userServiceConfiguration.BaseAddress}")
                };

                    string str = string.Join(", ", dependencies.Select(m => $"**{m.Title}**: [{m.Value}]"));
                    return new HealthCheckResult(HealthStatus.Healthy, str);
                }
                catch (Exception)
                {
                    return new HealthCheckResult(HealthStatus.Healthy, "Reading configuration file failed");
                }
            });
        }
    }
}