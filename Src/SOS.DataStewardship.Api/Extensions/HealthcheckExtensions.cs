using SOS.DataStewardship.Api.HealthChecks;

namespace SOS.DataStewardship.Api.Extensions;

internal static class HealthcheckExtensions
{
    /// <summary>
    /// Configure health checks
    /// </summary>
    /// <param name="webApplicationBuilder"></param>
    /// <param name="processedDbConfiguration"></param>
    internal static void SetupHealthChecks(this WebApplicationBuilder webApplicationBuilder, MongoDbConfiguration processedDbConfiguration)
    {
        webApplicationBuilder.Services.AddHealthChecks()
            .AddMongoDb(processedDbConfiguration.GetConnectionString(), tags: new[] { "database", "mongodb" })
            .AddCheck<DatasetHealthCheck>("Dataset", tags: new[] { "database", "elasticsearch", "query" })
            .AddCheck<EventHealthCheck>("Event", tags: new[] { "database", "elasticsearch", "query" })
            .AddCheck<OccurrenceHealthCheck>("Occurrence", tags: new[] { "database", "elasticsearch", "query" });
    }
}
