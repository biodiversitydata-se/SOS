using System;
using Microsoft.Extensions.Configuration;
using SOS.Observations.Api.EndToEndTests.EndToEndTests;
using SOS.TestHelpers;

namespace SOS.Observations.Api.EndToEndTests.Fixtures
{
    public class ApiEndToEndTestFixture : FixtureBase, IDisposable
    {
        public SosApiClient SosApiClient { get; private set; }
        public InstallationEnvironment InstallationEnvironment { get; private set; }

        public ApiEndToEndTestFixture()
        {
            InstallationEnvironment = GetEnvironmentFromAppSettings();
            var apiUrl = GetApiUrl();
            SosApiClient = new SosApiClient(apiUrl);
        }

        private string GetApiUrl()
        {
            var config = GetAppSettings();
            var configPrefix = GetConfigPrefix(InstallationEnvironment);
            var apiUrl = config.GetSection($"{configPrefix}:ApiUrl").Get<string>();
            return apiUrl;
        }

        public void Dispose() { }
    }
}
