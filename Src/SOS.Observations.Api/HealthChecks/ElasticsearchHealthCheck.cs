﻿using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Processed.Configuration;
using SOS.Lib.Models.Processed.Observation;
using SOS.Observations.Api.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HealthStatus = Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus;

namespace SOS.Observations.Api.HealthChecks
{
    /// <summary>
    /// Health check for Elasticsearch.
    /// </summary>
    public class ElasticsearchHealthCheck : IHealthCheck
    {
        private readonly HealthCheckConfiguration _healthCheckConfiguration;
        private readonly IElasticClientManager _elasticClientManager;
        private readonly ElasticSearchConfiguration _elasticConfiguration;
        private readonly ICache<string, ProcessedConfiguration> _processedConfigurationCache;
        private readonly ILogger<ElasticsearchHealthCheck> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="healthCheckConfiguration"></param>
        /// <param name="elasticClientManager"></param>
        /// <param name="elasticConfiguration"></param>
        /// <param name="processedConfigurationCache"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public ElasticsearchHealthCheck(HealthCheckConfiguration healthCheckConfiguration,
            IElasticClientManager elasticClientManager,
            ElasticSearchConfiguration elasticConfiguration,
            ICache<string, ProcessedConfiguration> processedConfigurationCache,
            ILogger<ElasticsearchHealthCheck> logger)
        {
            _healthCheckConfiguration = healthCheckConfiguration ?? throw new ArgumentNullException(nameof(healthCheckConfiguration));
            _elasticClientManager = elasticClientManager ?? throw new ArgumentNullException(nameof(elasticClientManager));
            _elasticConfiguration = elasticConfiguration ?? throw new ArgumentNullException(nameof(elasticConfiguration));
            _processedConfigurationCache = processedConfigurationCache ?? throw new ArgumentNullException(nameof(processedConfigurationCache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected ElasticsearchClient Client => ClientCount == 1 ? _elasticClientManager.Clients.FirstOrDefault() : _elasticClientManager.Clients[ActiveInstance];
        protected int ClientCount => _elasticClientManager.Clients.Length;
        public byte ActiveInstance => GetConfiguration()?.ActiveInstance ?? 1;

        private ProcessedConfiguration GetConfiguration()
        {
            try
            {
                var processedConfig = _processedConfigurationCache.GetAsync(_processedConfigurationId)?.Result;
                return processedConfig ?? new ProcessedConfiguration { Id = _processedConfigurationId };
            }
            catch
            {
                return default;
            }
        }
        private readonly string _processedConfigurationId = nameof(Observation);


        private async Task<HealthCheckResult> GetActiveClusterHealthCheckResult()
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                var health = await Client.Cluster.HealthAsync();
                if (!health.IsValidResponse)
                {
                    _logger.LogError(new Exception(health.DebugInformation), "Elasticsearch health check failed");
                    return new HealthCheckResult(HealthStatus.Degraded, "Elasticsearch health check failed. client.Cluster.HealthAsync() returned invalid response.");
                }

                sb.Append($"{health.ClusterName}: {health.Status}");

                var message = await GetHeapUsedPercentMessageAsync(Client);

                return new HealthCheckResult(health.Status switch
                {
                    Elastic.Clients.Elasticsearch.HealthStatus.Green => HealthStatus.Healthy,
                    Elastic.Clients.Elasticsearch.HealthStatus.Yellow => HealthStatus.Degraded,
                    _ => HealthStatus.Unhealthy
                }, message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Elasticsearch health check failed");
                return new HealthCheckResult(HealthStatus.Degraded, $"Elasticsearch health check failed. Message: {e.Message}", e);
            }
        }


        private async Task<HealthCheckResult> GetAllClustersHealthCheckResult()
        {
            try
            {
                var clusterStatuses = new List<string>();
                var status = Elastic.Clients.Elasticsearch.HealthStatus.Green;
                foreach (var client in _elasticClientManager.Clients)
                {
                    StringBuilder sb = new StringBuilder();
                    bool isActive = Client == client;
                    var health = await client.Cluster.HealthAsync();
                    if (!health.IsValidResponse)
                    {
                        _logger.LogError(new Exception(health.DebugInformation), "Elasticsearch health check failed");
                        return new HealthCheckResult(HealthStatus.Degraded, "Elasticsearch health check failed. client.Cluster.HealthAsync() returned invalid response.");
                    }

                    if (health.Status > status) status = health.Status;
                    if (isActive)
                        sb.Append($"{health.ClusterName} (active): {health.Status}");
                    else
                        sb.Append($"{health.ClusterName}: {health.Status}");

                    var message = await GetHeapUsedPercentMessageAsync(client);
                    if (!string.IsNullOrEmpty(message))
                    {
                        clusterStatuses.Add(message);
                    }
                }

                string resultMessage = string.Join(", ", clusterStatuses);

                return new HealthCheckResult(status switch
                {
                    Elastic.Clients.Elasticsearch.HealthStatus.Green => HealthStatus.Healthy,
                    Elastic.Clients.Elasticsearch.HealthStatus.Yellow => HealthStatus.Degraded,
                    _ => HealthStatus.Unhealthy
                }, resultMessage);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Elasticsearch health check failed");
                return new HealthCheckResult(HealthStatus.Degraded, $"Elasticsearch health check failed. Message: {e.Message}", e);
            }
        }

        private async Task<string> GetHeapUsedPercentMessageAsync(ElasticsearchClient client)
        {
            
            var nodeStatsResponse = await client.Nodes.StatsAsync();

            // Check if the response is valid
            if (nodeStatsResponse.IsValidResponse)
            {
                var sb = new StringBuilder();
                int percent = 0;
                foreach (var node in nodeStatsResponse.Nodes)
                {
                    percent = Math.Max(percent, (byte)(node.Value.Jvm.Mem.HeapUsedPercent ?? 0));
                }
                sb.Append($" ({percent}% mem usage)");

                return sb.ToString();
            }

            return null;
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
                //var result = await GetAllClustersHealthCheckResult();
                var result = await GetActiveClusterHealthCheckResult();
                return result;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Elasticsearch health check failed");
                return new HealthCheckResult(HealthStatus.Degraded, $"Elasticsearch health check failed. Message: {e.Message}", e);
            }
        }
    }
}