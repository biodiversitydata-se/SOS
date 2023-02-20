namespace SOS.DataStewardship.Api.Extensions;

internal static class HealthcheckExtensions
{
   /// <summary>
   /// Configure health checks
   /// </summary>
   /// <param name="webApplicationBuilder"></param>
    internal static void SetupHealthChecks(this WebApplicationBuilder webApplicationBuilder, MongoDbConfiguration processedDbConfiguration)
    {
        webApplicationBuilder.Services.AddHealthChecks()
            .AddTcpHealthCheck(s => s.AddressFamily = System.Net.Sockets.AddressFamily.NetBios)
            .AddMongoDb(processedDbConfiguration.GetConnectionString(), tags: new[] { "database", "mongodb" });
    }
}
