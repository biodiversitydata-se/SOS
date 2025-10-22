using System.Threading.Tasks;
using FluentAssertions;
using SOS.Observations.Api.LiveIntegrationTests.Fixtures;
using System.Threading.Tasks;
using Xunit;

namespace SOS.Observations.Api.LiveIntegrationTests.IntegrationTests.ObservationManager
{
    [Collection(Fixtures.Collections.ApiIntegrationTestsCollection)]
    public class DuplicatesIntegrationTests
    {
        private readonly ApiIntegrationTestFixture _fixture;

        public DuplicatesIntegrationTests(ApiIntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Check_for_public_observation_duplicates()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var duplicates = await _fixture.ObservationManager.TryToGetOccurenceIdDuplicatesAsync(false, 100);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            duplicates.Should().BeEmpty();
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Check_for_sensitive_observation_duplicates()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var duplicates = await _fixture.ObservationManager.TryToGetOccurenceIdDuplicatesAsync(true, 100);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            duplicates.Should().BeEmpty();
        }
    }
}
