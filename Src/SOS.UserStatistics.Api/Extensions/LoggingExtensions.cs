namespace SOS.UserStatistics.Extensions;

internal static class LoggingExtensions
{
    internal static ILoggingBuilder SetupLogging(this ILoggingBuilder loggingBuilder)
    {
        loggingBuilder.ClearProviders();
        loggingBuilder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
        return loggingBuilder;
    }
}
