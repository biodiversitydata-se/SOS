using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.Extensions.DependencyInjection;
using SOS.Status.Web.Client.Abstractions;
using SOS.Status.Web.Client.Models.BlazorSamples;
using SOS.Status.Web.HttpClients;
using SOS.Status.Web.Services;

namespace SOS.Status.Web.Extensions;

public static class DependencyInjectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddDependencyInjectionServices(IConfigurationRoot configuration)
        {
            services.AddHttpClient<SosObservationsApiClient>(client =>
            {
                client.BaseAddress = new Uri(Settings.HttpClientsConfiguration.SosObservationsApi.BaseAddress);
            }).AddHttpMessageHandler<TokenHandler>();

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