﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SOS.Lib.Configuration.Shared;
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
        private readonly IConfiguration _configuration;
        private readonly IProcessedObservationRepository _processedObservationRepository;

        /// <summary>
        /// Constructor
        /// </summary>
        public DependenciesHealthCheck(IConfiguration configuration,
            IProcessedObservationRepository processedObservationRepository)
        {
            _configuration = configuration;
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
                    var processedDbConfiguration = _configuration.GetSection("ProcessDbConfiguration").Get<MongoDbConfiguration>();
                    var elasticConfiguration = _configuration.GetSection("SearchDbConfiguration").Get<ElasticSearchConfiguration>();
                    var identityServerConfiguration = _configuration.GetSection("IdentityServer").Get<IdentityServerConfiguration>();
                    var hangfireConfiguration = _configuration.GetSection("HangfireDbConfiguration").Get<HangfireDbConfiguration>();
                    var userServiceConfiguration = _configuration.GetSection("UserServiceConfiguration").Get<UserServiceConfiguration>();
                    var esClusterIndex = Math.Min(elasticConfiguration.Clusters.Count() - 1, esActiveIndex);

                    var dependencies = new List<(string Title, string Value)>
                {
                    ("MongoDb", $"{processedDbConfiguration.Hosts.First().Name} ({processedDbConfiguration.DatabaseName})"),
                    ("Hangfire", $"{hangfireConfiguration.Hosts.First().Name} ({hangfireConfiguration.DatabaseName})"),
                    ("Elasticsearch", $"{elasticConfiguration.Clusters.ElementAt(esClusterIndex).Hosts.First()} ({observationIndexName})"),
                    ("ArtdataUserService",$"{userServiceConfiguration.BaseAddress}"),
                    ("IdentityServer", $"{identityServerConfiguration.Authority}")
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