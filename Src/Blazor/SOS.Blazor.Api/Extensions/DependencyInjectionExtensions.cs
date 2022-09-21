using Microsoft.AspNetCore.Authentication;

namespace SOS.Blazor.Api.Extensions;

public static class DependencyInjectionExtensions
{
    public static WebApplicationBuilder SetupDependencies(this WebApplicationBuilder webApplicationBuilder)
    {
        // Sosclients
        webApplicationBuilder.Services.AddScoped<ISosClient, SosClient>();
        webApplicationBuilder.Services.AddScoped<ISosUserStatisticsClient, SosUserStatisticsClient>();

        // Httpclients
        webApplicationBuilder.Services.AddHttpClient("SosClient", client =>
        {
            client.BaseAddress = new Uri(webApplicationBuilder.Configuration.GetSection("SosApiUrl").Value);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });
        webApplicationBuilder.Services.AddHttpClient("SosUserStatisticsClient", client =>
        {
            client.BaseAddress = new Uri(webApplicationBuilder.Configuration.GetSection("SosUserStatisticsApiUrl").Value);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        return webApplicationBuilder;
    }
}
