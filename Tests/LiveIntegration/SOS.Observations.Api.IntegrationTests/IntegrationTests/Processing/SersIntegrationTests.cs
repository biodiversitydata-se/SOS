using FluentAssertions;
using SOS.Observations.Api.IntegrationTests.Fixtures;
using Xunit;
using SOS.Lib.Helpers;
using System.Collections.Generic;
using SOS.Lib.Factories;
using System.Threading.Tasks;

namespace SOS.Observations.Api.IntegrationTests.IntegrationTests.TaxonManager
{
    [Collection(Collections.ApiIntegrationTestsCollection)]
    public class SersIntegrationTests
    {
        private readonly ApiIntegrationTestFixture _fixture;

        public SersIntegrationTests(ApiIntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task TestMaxId()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------            
            var maxId = await _fixture.SersObservationVerbatimRepository.GetMaxIdAsync();
            
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            maxId.Should().BeGreaterThan(1);
        }  
    }
}