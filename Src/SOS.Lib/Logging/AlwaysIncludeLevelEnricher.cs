using Serilog.Core;
using Serilog.Events;

namespace SOS.Lib.Logging;

public class AlwaysIncludeLevelEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var level = logEvent.Level == LogEventLevel.Information
            ? "Info"
            : logEvent.Level.ToString();

        logEvent.AddOrUpdateProperty(
            propertyFactory.CreateProperty("Level", level));
    }
}
