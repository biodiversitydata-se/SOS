using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Microsoft.Extensions.Configuration;
using SOS.Observations.Api.IntegrationTests.ApiClients;
using SOS.Observations.Api.IntegrationTests.Configuration;

namespace SOS.Observations.Api.IntegrationTests.Fixtures
{

    public class ApiTestFixture : IDisposable
    {
        public SosApiClient SosApiClient { get; private set; }

        public ApiTestFixture()
        {
            var testConfiguration = GetApiTestConfiguration();
            SosApiClient = new SosApiClient(testConfiguration);
        }

        public void Dispose()
        {
            
        }

        protected ApiTestConfiguration GetApiTestConfiguration()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .AddUserSecrets<TestBase>()
                .Build();

            var apiTestConfiguration = config.GetSection("ApiTestConfiguration").Get<ApiTestConfiguration>();
            return apiTestConfiguration;
        }
    }
}
