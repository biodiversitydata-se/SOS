using Microsoft.Extensions.Configuration;
using SOS.Lib.Configuration.Export;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Configuration.Shared;

namespace SOS.Process.IntegrationTests
{
    public class TestBase
    {
        protected ProcessConfiguration GetProcessConfiguration()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .AddUserSecrets<TestBase>()
                .Build();

            var processConfiguration = config.GetSection(nameof(ProcessConfiguration)).Get<ProcessConfiguration>();
            return processConfiguration;
        }

        protected MongoDbConfiguration GetProcessDbConfiguration()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .AddUserSecrets<TestBase>()
                .Build();

            var exportConfiguration = config.GetSection("ProcessDbConfiguration").Get<MongoDbConfiguration>();
            return exportConfiguration;
        }

        protected ExportConfiguration GetExportConfiguration()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .AddUserSecrets<TestBase>()
                .Build();

            var exportConfiguration = config.GetSection(nameof(ExportConfiguration)).Get<ExportConfiguration>();
            return exportConfiguration;
        }

        protected MongoDbConfiguration GetVerbatimDbConfiguration()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .AddUserSecrets<TestBase>()
                .Build();

            var exportConfiguration = config.GetSection("VerbatimDbConfiguration").Get<MongoDbConfiguration>();
            return exportConfiguration;
        }

        protected ElasticSearchConfiguration GetElasticConfiguration()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .AddUserSecrets<TestBase>()
                .Build();

            var elasticConfiguration = config.GetSection("SearchDbConfiguration")
                .Get<ElasticSearchConfiguration>();
            return elasticConfiguration;
        }

        protected ArtportalenConfiguration GetArtportalenConfiguration()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .AddUserSecrets<TestBase>()
                .Build();

            var artportalenConfiguration = config.GetSection("ArtportalenConfiguration")
                .Get<ArtportalenConfiguration>();
            return artportalenConfiguration;
        }
    }
}