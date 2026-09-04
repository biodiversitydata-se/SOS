using SOS.Status.Web.Client.Abstractions;
using SOS.Status.Web.Client.Models.BlazorSamples;
using SOS.Status.Web.HttpClients;
using SOS.Status.Web.Services;
using SOS.Api.Client.DependencyInjection;

namespace SOS.Status.Web.Extensions;

public static class DependencyInjectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddDependencyInjectionServices(IConfigurationRoot configuration)
        {            
            services.AddSosWebClients(options =>
            {
                options.ObservationsApiBaseAddress = new Uri(Settings.HttpClientsConfiguration.SosObservationsApi.BaseAddress);
                options.AnalysisApiBaseAddress = new Uri(Settings.HttpClientsConfiguration.SosAnalysisApi.BaseAddress);
                options.SystemName = "SOS Status";
                options.ApplicationIdentifier = "PrintObs";
                options.RequestLoggingLevel = LogLevel.Information;
            });

            // Use this when you want to use a specific token handler or other custom configuration for the HttpClient.
            //services.AddSosClients(options =>
            //{
            //    options.ObservationsApiBaseAddress = new Uri(Settings.HttpClientsConfiguration.SosObservationsApi.BaseAddress);
            //},
            //httpClientBuilder =>
            //{                
            //    httpClientBuilder.AddStandardResilienceHandler();
            //    httpClientBuilder.AddHttpMessageHandler<TokenHandler>();
            //});

            // Use this when no standard resilience handler is configured        
            //var sosObservationsHttpClientBuilder = services.AddHttpClient<SosObservationsApiClient>(client =>
            //{
            //    client.BaseAddress = new Uri(Settings.HttpClientsConfiguration.SosObservationsApi.BaseAddress);
            //});
            //sosObservationsHttpClientBuilder.AddStandardResilienceHandler(o =>
            //{
            //    o.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(240);
            //    o.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(240);
            //    o.AttemptTimeout.Timeout = TimeSpan.FromSeconds(120);
            //    o.Retry.MaxDelay = TimeSpan.FromSeconds(120);
            //});
            //sosObservationsHttpClientBuilder.AddHttpMessageHandler<TokenHandler>();

            services.AddHttpClient<SosAdministrationApiClient>(client =>
            {
                client.BaseAddress = new Uri(Settings.HttpClientsConfiguration.SosAdministrationApi.BaseAddress);
            }).AddHttpMessageHandler<TokenHandler>();
            services.AddHttpClient<SosAnalysisApiClient>(client =>
            {
                client.BaseAddress = new Uri(Settings.HttpClientsConfiguration.SosAnalysisApi.BaseAddress);
            }).AddHttpMessageHandler<TokenHandler>();
            services.AddHttpClient<SosElasticsearchProxyClient>(client =>
            {
                client.BaseAddress = new Uri(Settings.HttpClientsConfiguration.SosElasticsearchProxy.BaseAddress);
            }).AddHttpMessageHandler<TokenHandler>();
            services.AddHttpClient<SosDataStewardshipApiClient>(client =>
            {
                client.BaseAddress = new Uri(Settings.HttpClientsConfiguration.SosDataStewardshipApi.BaseAddress);
            }).AddHttpMessageHandler<TokenHandler>();
            services.AddHttpClient<IpifyClient>(client =>
            {
                client.BaseAddress = new Uri("https://api.ipify.org");
                client.Timeout = TimeSpan.FromSeconds(10);
            });

            services.AddScoped<AppState>();
            services.AddScoped<StatusInfoService>();
            services.AddScoped<IStatusInfoService, StatusInfoService>();
            services.AddScoped<ObservationSearchService>();
            services.AddScoped<IObservationSearchService, ObservationSearchService>();
            services.AddScoped<TaxonDiagramService>();
            services.AddScoped<ITaxonDiagramService, TaxonDiagramService>();
            services.AddScoped<TokenHandler>();
            services.AddHttpContextAccessor();
            return services;
        }
    }
}
