using System.Threading.Tasks;
using FluentAssertions;
using SOS.Observations.Api.IntegrationTests.Fixtures;
using Xunit;
using SOS.Lib.Models.Search.Filters;

namespace SOS.Observations.Api.IntegrationTests.IntegrationTests.ObservationRepository
{
    [Collection(Collections.ApiIntegrationTestsCollection)]
    public class EventIdsAggregationTests
    {
        private readonly ApiIntegrationTestFixture _fixture;

        public EventIdsAggregationTests(ApiIntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Get_eventIds_for_datastewardship_dataset()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            SearchFilter searchFilter = new SearchFilter(0)
            {
                DataStewardshipDatasetId = "ArtportalenDataHost - Dataset Bats"
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var eventIds = await _fixture.ProcessedObservationRepository.GetEventIdsAsync(searchFilter);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            eventIds.Should().NotBeNull();
        }
    }
}