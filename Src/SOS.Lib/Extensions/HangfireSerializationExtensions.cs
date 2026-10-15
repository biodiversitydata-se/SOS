using Hangfire;
using Newtonsoft.Json.Converters;
using SOS.Lib.JsonConverters;

namespace SOS.Lib.Extensions;

public static class HangfireSerializationExtensions
{
    extension(IGlobalConfiguration configuration)
    {
        /// <summary>
        /// Configures the same argument converters for job producers, workers and dashboards.
        /// </summary>
        public IGlobalConfiguration UseSosHangfireSerializerSettings() =>
            configuration.UseRecommendedSerializerSettings(settings =>
            {
                settings.Converters.Add(new NetTopologySuite.IO.Converters.GeometryConverter());
                settings.Converters.Add(new StringEnumConverter());
                settings.Converters.Add(new GeoJsonTextPositionConverter());
            });
    }
}
