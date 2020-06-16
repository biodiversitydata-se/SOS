using Microsoft.Extensions.Configuration;
using SOS.Lib.Configuration.Export;
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

            var processConfiguration = config.GetSection(typeof(ProcessConfiguration).Name).Get<ProcessConfiguration>();
            return processConfiguration;
        }

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

        protected ElasticSearchConfiguration GetElasticConfiguration()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .AddUserSecrets<TestBase>()
                .Build();

            var elasticConfiguration = config.GetSection("ProcessConfiguration").GetSection("SearchDbConfiguration")
                .Get<ElasticSearchConfiguration>();
            return elasticConfiguration;
        }
    }
}