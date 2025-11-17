using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace SOS.Administration.Api.Extensions;

public static class HealthCheckExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection SetupHealthchecks()
        {
            services.AddHealthChecks().AddCheck<HealthCheck>("CustomHealthCheck");

            return services;
        }
    }

    extension(WebApplication app)
    {
        public WebApplication ApplyMapHealthChecks()
        {
            //app.UseHealthChecks("/healthz");
            app.MapHealthChecks("/healthz").AllowAnonymous();

            return app;
        }
    }
}