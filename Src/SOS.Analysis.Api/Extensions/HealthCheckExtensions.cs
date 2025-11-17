using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SOS.Analysis.Api.HealthChecks;

namespace SOS.Analysis.Api.Extensions;

public static class HealthCheckExtensions
{
    extension(HealthStatus healthStatus)
    {
        /// <summary>
        /// Create health check result
        /// </summary>
        /// <param name="description"></param>
        /// <returns></returns>
        public HealthCheckResult ToHealthCheckResult(string description = null)
        {
            return new HealthCheckResult(healthStatus, description);
        }
    }

    extension(IServiceCollection services)
    {
        public IServiceCollection SetupHealthchecks()
        {
            services.AddHealthChecks()
                .AddCheck<HealthCheck>("CustomHealthCheck", tags: ["k8s"])
                .AddCheck<AggregateHealthCheck>("AggregateHealthCheck", tags: ["Analysis.API"]);

            return services;
        }
    }

    extension(WebApplication app)
    {
        public WebApplication ApplyMapHealthChecks()
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