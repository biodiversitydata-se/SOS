using Microsoft.Extensions.Configuration;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Configuration.Shared;

namespace SOS.Process.IntegrationTests
{
    public class TestBase
    {
        protected ProcessConfiguration GetProcessConfiguration()
        {
            IConfigurationRoot config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .AddUserSecrets<TestBase>()
                .Build();

            ProcessConfiguration processConfiguration = config.GetSection(typeof(ProcessConfiguration).Name).Get<ProcessConfiguration>();
            return processConfiguration;
        }
        protected ElasticSearchConfiguration GetElasticConfiguration()
        {
            IConfigurationRoot config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .AddUserSecrets<TestBase>()
                .Build();

            var elasticConfiguration = config.GetSection("ProcessConfiguration").GetSection("SearchDbConfiguration").Get<ElasticSearchConfiguration>();            
            return elasticConfiguration;
        }
    }
}
