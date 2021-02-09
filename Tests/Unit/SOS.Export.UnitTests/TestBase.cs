using Microsoft.Extensions.Configuration;
using SOS.Lib.Configuration.Export;

namespace SOS.Export.UnitTests
{
    public class TestBase
    {
        protected ExportConfiguration GetExportConfiguration()
        {
            return null;
            /*
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .AddUserSecrets<TestBase>()
                .Build();

            var exportConfiguration = config.GetSection(typeof(ExportConfiguration).Name).Get<ExportConfiguration>();
            return exportConfiguration;*/
        }
    }
}