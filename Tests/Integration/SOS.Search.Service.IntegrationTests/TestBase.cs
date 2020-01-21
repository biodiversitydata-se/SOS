using Microsoft.Extensions.Configuration;
using SOS.Lib.Configuration.Export;
using SOS.Lib.Configuration.Shared;

namespace SOS.Search.Service.IntegrationTests
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

            var mongoDbConfiguration = config.GetSection("MongoDbConfiguration").Get<MongoDbConfiguration>();
            return mongoDbConfiguration;
        }
    }
}
