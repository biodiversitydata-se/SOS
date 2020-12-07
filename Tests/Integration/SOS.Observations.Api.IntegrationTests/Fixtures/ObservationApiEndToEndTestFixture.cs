using System;
using Microsoft.Extensions.Configuration;
using SOS.Observations.Api.IntegrationTests.EndToEndTests;
using SOS.TestHelpers;

namespace SOS.Observations.Api.IntegrationTests.Fixtures
{
    public class ObservationApiEndToEndTestFixture : FixtureBase, IDisposable
    {
        public SosApiClient SosApiClient { get; private set; }
        public InstallationEnvironment InstallationEnvironment { get; private set; }

        public ObservationApiEndToEndTestFixture()
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
