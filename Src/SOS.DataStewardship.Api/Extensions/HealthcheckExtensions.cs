using SOS.DataStewardship.Api.HealthChecks;

namespace SOS.DataStewardship.Api.Extensions;

internal static class HealthcheckExtensions
{
    extension(WebApplicationBuilder webApplicationBuilder)
    {
        /// <summary>
        /// Configure health checks
        /// </summary>
        /// <param name="processedDbConfiguration"></param>
        internal void SetupHealthChecks(MongoDbConfiguration processedDbConfiguration)
        {
            webApplicationBuilder.Services.AddHealthChecks()
                // .AddMongoDb(processedDbConfiguration.GetConnectionString(), tags: new[] { "database", "mongodb" }) not working with MongoDbdriver 3.0.0
                .AddCheck<DatasetHealthCheck>("Dataset", tags: new[] { "database", "elasticsearch", "query" })
                .AddCheck<EventHealthCheck>("Event", tags: new[] { "database", "elasticsearch", "query" })
                .AddCheck<OccurrenceHealthCheck>("Occurrence", tags: new[] { "database", "elasticsearch", "query" });
        }
    }
}
