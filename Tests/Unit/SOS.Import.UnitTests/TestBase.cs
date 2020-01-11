using Microsoft.Extensions.Configuration;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Configuration.Process;

namespace SOS.Import.UnitTests
{
    public class TestBase
    {
        protected ImportConfiguration GetImportConfiguration()
        {
            IConfigurationRoot config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .AddUserSecrets<TestBase>()
                .Build();

            ImportConfiguration importConfiguration = config.GetSection(typeof(ImportConfiguration).Name).Get<ImportConfiguration>();
            return importConfiguration;
        }

        protected ProcessConfiguration GetProcessConfiguration()
        {
            IConfigurationRoot config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .AddUserSecrets<TestBase>()
                .Build();

            var processConfiguration = config.GetSection(typeof(ProcessConfiguration).Name).Get<ProcessConfiguration>();
            return processConfiguration;
        }

    }
}
