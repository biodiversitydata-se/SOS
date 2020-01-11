using Microsoft.Extensions.Configuration;
using SOS.Lib.Configuration.Export;

namespace SOS.Export.IntegrationTests
{
    public class TestBase
    {
        protected ExportConfiguration GetExportConfiguration()
        {
            IConfigurationRoot config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .AddUserSecrets<TestBase>()
                .Build();

            ExportConfiguration exportConfiguration = config.GetSection(typeof(ExportConfiguration).Name).Get<ExportConfiguration>();
            return exportConfiguration;
        }
    }
}
