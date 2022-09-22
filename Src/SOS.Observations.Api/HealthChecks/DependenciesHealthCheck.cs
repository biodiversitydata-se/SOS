using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SOS.Lib.Configuration.Shared;

namespace SOS.Observations.Api.HealthChecks
{
    /// <summary>
    /// Get information about what dependencies are used.
    /// </summary>
    public class DependenciesHealthCheck : IHealthCheck
    {
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Constructor
        /// </summary>
        public DependenciesHealthCheck(IConfiguration configuration)
        {
            _configuration = configuration;
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
                var processedDbConfiguration = _configuration.GetSection("ProcessDbConfiguration").Get<MongoDbConfiguration>();
                var elasticConfiguration = _configuration.GetSection("SearchDbConfiguration").Get<ElasticSearchConfiguration>();
                var identityServerConfiguration = _configuration.GetSection("IdentityServer").Get<IdentityServerConfiguration>();
                var hangfireConfiguration = _configuration.GetSection("HangfireDbConfiguration").Get<HangfireDbConfiguration>();
                var userServiceConfiguration = _configuration.GetSection("UserServiceConfiguration").Get<UserServiceConfiguration>();

                var sb = new StringBuilder();
                sb.AppendLine($"MongoDb: \"{processedDbConfiguration.Hosts.First().Name}\" ({processedDbConfiguration.DatabaseName})");
                sb.AppendLine($"Hangfire: \"{hangfireConfiguration.Hosts.First().Name}\" ({hangfireConfiguration.DatabaseName})");
                sb.AppendLine($"Elasticsearch: \"{elasticConfiguration.Clusters.First()}\" ({elasticConfiguration.IndexPrefix})");
                sb.AppendLine($"ArtdataUserService: \"{userServiceConfiguration.BaseAddress}\"");
                sb.AppendLine($"IdentityServer: \"{identityServerConfiguration.Authority}\"");

                return new HealthCheckResult(HealthStatus.Healthy, sb.ToString());
            }
            catch (Exception e)
            {
                return new HealthCheckResult(HealthStatus.Healthy, "Reading configuration file failed");
            }
        }
    }
}