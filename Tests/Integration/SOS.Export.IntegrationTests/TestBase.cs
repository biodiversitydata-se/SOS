using Microsoft.Extensions.Configuration;
using SOS.Lib.Configuration.Export;
using SOS.Lib.Configuration.Shared;

namespace SOS.Export.IntegrationTests
{
    public class TestBase
    {
        protected ExportConfiguration GetExportConfiguration()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .AddUserSecrets<TestBase>()
                .Build();

            var exportConfiguration = config.GetSection(typeof(ExportConfiguration).Name).Get<ExportConfiguration>();
            return exportConfiguration;
        }

        protected MongoDbConfiguration GetProcessDbConfiguration()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .AddUserSecrets<TestBase>()
                .Build();

            var exportConfiguration = config.GetSection("ApplicationSettings").GetSection("ProcessDbConfiguration").Get<MongoDbConfiguration>();
            return exportConfiguration;
        }

        protected ElasticSearchConfiguration GetElasticConfiguration()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .AddUserSecrets<TestBase>()
                .Build();

            var elasticConfiguration = config.GetSection("ApplicationSettings").GetSection("SearchDbConfiguration")
                .Get<ElasticSearchConfiguration>();
            return elasticConfiguration;
        }
    }
}