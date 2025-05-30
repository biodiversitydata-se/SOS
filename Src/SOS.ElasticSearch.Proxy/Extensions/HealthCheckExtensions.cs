namespace SOS.ElasticSearch.Proxy.Extensions
{
    public static class HealthCheckExtensions
    {        
        public static IServiceCollection SetupHealthchecks(this IServiceCollection services)
        {
            services.AddHealthChecks().AddCheck<HealthCheck>("CustomHealthCheck");

            return services;
        }
    }
}