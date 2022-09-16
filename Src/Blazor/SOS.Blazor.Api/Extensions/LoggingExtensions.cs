using System.Runtime.CompilerServices;

namespace SOS.Blazor.Api.Extensions
{
    public static class LoggingExtensions
    {
        public static WebApplicationBuilder SetupLogging(this WebApplicationBuilder webApplicationBuilder)
        {
            webApplicationBuilder.Logging.ClearProviders();
            webApplicationBuilder.Logging.AddConsole();

            return webApplicationBuilder;
        }
    }
}
