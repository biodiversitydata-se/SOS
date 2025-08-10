using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using SOS.Status.Web.Client.Abstractions;
using SOS.Status.Web.Client.Models.BlazorSamples;
using SOS.Status.Web.HttpClients;
using SOS.Status.Web.Services;

namespace SOS.Status.Web.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddDependencyInjectionServices(this IServiceCollection services, IConfigurationRoot configuration)
    {
        services.AddHttpClient<SosObservationsApiClient>(client =>
        {
            client.BaseAddress = new Uri(Settings.HttpClientsConfiguration.SosObservationsApi.BaseAddress);
        }).AddHttpMessageHandler<TokenHandler>();
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