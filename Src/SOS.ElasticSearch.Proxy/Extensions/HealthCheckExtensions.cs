namespace SOS.ElasticSearch.Proxy.Extensions;

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
}