using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace SOS.Administration.Api.Extensions
{
    public static class HealthCheckExtensions
    {        
        public static IServiceCollection SetupHealthchecks(this IServiceCollection services)
        {
            services.AddHealthChecks().AddCheck<HealthCheck>("CustomHealthCheck");

            return services;
        }

        public static WebApplication ApplyMapHealthChecks(this WebApplication app)
        {            
            //app.UseHealthChecks("/healthz");
            app.MapHealthChecks("/healthz");

            return app;
        }
    }
}