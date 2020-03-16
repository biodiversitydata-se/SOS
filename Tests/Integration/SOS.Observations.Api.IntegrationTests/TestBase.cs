using Microsoft.Extensions.Configuration;
using SOS.Lib.Configuration.Shared;

namespace SOS.Observations.Api.IntegrationTests
{
    public class TestBase
    {
        protected MongoDbConfiguration GetMongoDbConfiguration()
        {
            IConfigurationRoot config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .AddUserSecrets<TestBase>()
                .Build();

            var mongoDbConfiguration = config.GetSection("ProcessedDbConfiguration").Get<MongoDbConfiguration>();
            return mongoDbConfiguration;
        }
    }
}
