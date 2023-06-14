using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Services.Interfaces;
using SOS.Observations.Api.Configuration;

namespace SOS.Observations.Api.HealthChecks
{
    /// <summary>
    /// Elasticsearch Proxy Health check. 
    /// </summary>
    public class ElasticsearchProxyHealthCheck : IHealthCheck
    {
        private readonly IHttpClientService _httpClientService;
        private readonly ElasticSearchConfiguration _elasticConfiguration;
        private readonly HealthCheckConfiguration _healthCheckConfiguration;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="httpClientService"></param>
        /// <param name="elasticConfiguration"></param>
        /// <param name="healthCheckConfiguration"></param>
        public ElasticsearchProxyHealthCheck(IHttpClientService httpClientService, 
            ElasticSearchConfiguration elasticConfiguration,
            HealthCheckConfiguration healthCheckConfiguration)
        {
            _httpClientService = httpClientService ?? throw new ArgumentNullException(nameof(httpClientService));
            _elasticConfiguration = elasticConfiguration ?? throw new ArgumentNullException(nameof(elasticConfiguration));
            _healthCheckConfiguration = healthCheckConfiguration ?? throw new ArgumentNullException(nameof(healthCheckConfiguration));
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
                var baseUri = new Uri(_healthCheckConfiguration.ElasticsearchProxyUrl);
                var url = new Uri(baseUri, "sos-observation/_search");
                var headerData = new Dictionary<string, string>();
                headerData.Add("username", _elasticConfiguration.UserName);
                headerData.Add("password", _elasticConfiguration.Password);

                System.Text.Json.JsonElement response = await _httpClientService.PostDataAsync<dynamic>(
                    url, new
                    {
                        size = 5,
                        query = new
                        {
                            match_all = new {}
                        },
                        from = 0
                    },
                    headerData,
                    "application/json");

                var hits = response
                    .GetProperty("hits")
                    .GetProperty("hits").GetArrayLength();

                if (hits != 5)
                {
                    return new HealthCheckResult(HealthStatus.Unhealthy, "Failed to get 5 observations");
                }
                
                return new HealthCheckResult(HealthStatus.Healthy, null);
            }
            catch (Exception)
            {
                return new HealthCheckResult(HealthStatus.Unhealthy, "Elasticsearch proxy health check failed");
            }
        }
    }
}
