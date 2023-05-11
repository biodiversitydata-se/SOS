using Microsoft.Extensions.Logging;

namespace SOS.Blazor.Api.Extensions;

public static class LoggingExtensions
{
    public static WebApplicationBuilder SetupLogging(this WebApplicationBuilder webApplicationBuilder)
    {
        webApplicationBuilder.Logging.ClearProviders();
        webApplicationBuilder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
        webApplicationBuilder.Host.UseNLog();
        return webApplicationBuilder;
    }
}
