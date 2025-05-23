using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SOS.Analysis.Api.HealthChecks;

namespace SOS.Analysis.Api.Extensions
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

        public static IServiceCollection SetupHealthchecks(this IServiceCollection services)
        {
            services.AddHealthChecks()
                .AddCheck<HealthCheck>("CustomHealthCheck", tags: ["k8s"])
                .AddCheck<AggregateHealthCheck>("AggregateHealthCheck", tags: ["Analysis.API"]);

            return services;
        }

        public static WebApplication ApplyMapHealthChecks(this WebApplication app)
        {
            app.MapHealthChecks("/healthz", new HealthCheckOptions()
            {
                Predicate = r => r.Tags.Contains("k8s")
            });
            app.MapHealthChecks("/health", new HealthCheckOptions()
            {
                ResponseWriter = (context, _) => UIResponseWriter.WriteHealthCheckUIResponse(context, _)
            });

            return app;
        }
    }
}