using Microsoft.Extensions.Configuration;
using SOS.Lib.Configuration.Process;

namespace SOS.Process.UnitTests
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
    }
}
