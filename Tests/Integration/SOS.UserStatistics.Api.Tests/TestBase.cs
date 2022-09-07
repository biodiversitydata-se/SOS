namespace SOS.UserStatistics.Api.Tests;

public class TestBase
{
    protected MongoDbConfiguration GetMongoDbConfiguration()
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .AddUserSecrets<TestBase>()
            .Build();

        var mongoDbConfiguration = config.GetSection("ProcessDbConfiguration").Get<MongoDbConfiguration>();
        return mongoDbConfiguration;
    }
}
