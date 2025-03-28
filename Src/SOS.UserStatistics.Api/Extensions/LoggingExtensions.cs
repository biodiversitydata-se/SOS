﻿using Microsoft.AspNetCore.Builder;

namespace SOS.UserStatistics.Extensions;

internal static class LoggingExtensions
{
    internal static WebApplicationBuilder SetupLogging(this WebApplicationBuilder webApplicationBuilder)
    {
        webApplicationBuilder.Logging.ClearProviders();
        webApplicationBuilder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
        webApplicationBuilder.Host.UseNLog();
        return webApplicationBuilder;
    }
}
