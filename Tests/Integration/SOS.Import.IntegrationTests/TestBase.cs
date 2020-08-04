using Microsoft.Extensions.Configuration;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Configuration.Shared;

namespace SOS.Import.IntegrationTests
{
    public class TestBase
    {
        protected ImportConfiguration GetImportConfiguration()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .AddUserSecrets<TestBase>()
                .Build();

            var importConfiguration = config.GetSection(nameof(ImportConfiguration)).Get<ImportConfiguration>();
            return importConfiguration;
        }

        protected MongoDbConfiguration GetVerbatimDbConfiguration()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .AddUserSecrets<TestBase>()
                .Build();

            var verbatimDbConfiguration = config.GetSection("ApplicationSettings")
                .GetSection("VerbatimDbConfiguration").Get<MongoDbConfiguration>();
            return verbatimDbConfiguration;
        }
    }
}