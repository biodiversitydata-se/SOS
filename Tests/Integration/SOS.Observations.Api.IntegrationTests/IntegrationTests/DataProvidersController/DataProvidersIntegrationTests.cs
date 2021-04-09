using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using SOS.Observations.Api.Dtos;
using SOS.Observations.Api.Dtos.Vocabulary;
using SOS.Observations.Api.IntegrationTests.Extensions;
using SOS.Observations.Api.IntegrationTests.Fixtures;
using Xunit;

namespace SOS.Observations.Api.IntegrationTests.IntegrationTests.DataProvidersController
{
    [Collection(Collections.ApiIntegrationTestsCollection)]
    public class DataProvidersIntegrationTests
    {
        private readonly ApiIntegrationTestFixture _fixture;

        public DataProvidersIntegrationTests(ApiIntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Get_all_data_providers()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.DataProvidersController.GetDataProviders();
            var dataProviders = response.GetResult<List<DataProviderDto>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            dataProviders.Should().NotBeNull();
        }
    }
}