using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using SOS.Status.Web.HttpClients;
using SOS.Status.Web.Managers;
using SOS.Status.Web.Models;

namespace SOS.Status.Web.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddDependencyInjectionServices(this IServiceCollection services, IConfigurationRoot configuration)
    {
        services.AddScoped<AppSettingsState>();
        services.AddHttpClient<SosObservationsApiClient>(client =>
        {
            client.BaseAddress = new Uri(Settings.HttpClientsConfiguration.SosObservationsApi.BaseAddress);
        });
        services.AddHttpClient<SosAdministrationApiClient>(client =>
        {
            client.BaseAddress = new Uri(Settings.HttpClientsConfiguration.SosAdministrationApi.BaseAddress);
        });
        services.AddHttpClient<SosAnalysisApiClient>(client =>
        {
            client.BaseAddress = new Uri(Settings.HttpClientsConfiguration.SosAnalysisApi.BaseAddress);
        });
        services.AddHttpClient<SosElasticsearchProxyClient>(client =>
        {
            client.BaseAddress = new Uri(Settings.HttpClientsConfiguration.SosElasticsearchProxy.BaseAddress);
        });
        services.AddHttpClient<SosDataStewardshipApiClient>(client =>
        {
            client.BaseAddress = new Uri(Settings.HttpClientsConfiguration.SosDataStewardshipApi.BaseAddress);
        });

        services.AddScoped<StatusInfoManager>();
        services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();
        services.AddScoped<TokenAccessor>();
        services.AddScoped<AppState>();
        return services;
    }    
}