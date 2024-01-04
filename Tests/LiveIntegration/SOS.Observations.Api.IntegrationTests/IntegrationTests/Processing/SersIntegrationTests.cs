using FluentAssertions;
using SOS.Observations.Api.LiveIntegrationTests.Fixtures;
using System.Threading.Tasks;
using Xunit;

namespace SOS.Observations.Api.LiveIntegrationTests.IntegrationTests.Processing
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