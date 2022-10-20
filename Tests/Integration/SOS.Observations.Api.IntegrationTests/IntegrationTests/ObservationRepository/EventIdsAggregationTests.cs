using System.Threading.Tasks;
using FluentAssertions;
using SOS.Observations.Api.IntegrationTests.Fixtures;
using Xunit;
using SOS.Lib.Models.Search.Filters;
using System.Collections.Generic;
using System.Linq;

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
        public async Task Get_EventIds_for_datastewardship_dataset()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            SearchFilter searchFilter = new SearchFilter(0)
            {
                DataStewardshipDatasetIds = new List<string> { "ArtportalenDataHost - Dataset Bats" }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var eventIds = await _fixture.ProcessedObservationRepository.GetAggregationItemsAsync(searchFilter, "event.eventId");
            var eventIdsAll = await _fixture.ProcessedObservationRepository.GetAllAggregationItemsAsync(searchFilter, "event.eventId");
            
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            eventIds.Should().NotBeNull();
            eventIds.Count().Should().Be(eventIdsAll.Count());
        }


        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Get_allOccurrenceIds_for_datastewardship_dataset()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            SearchFilter searchFilter = new SearchFilter(0)
            {
                DataStewardshipDatasetIds = new List<string> { "ArtportalenDataHost - Dataset Bats" }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var occurrenceIds = await _fixture.ProcessedObservationRepository.GetAggregationItemsAsync(searchFilter, "occurrence.occurrenceId");
            var occurrenceIdsAll = await _fixture.ProcessedObservationRepository.GetAllAggregationItemsAsync(searchFilter, "occurrence.occurrenceId");
            

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            occurrenceIds.Should().NotBeNull();
            occurrenceIds.Count().Should().Be(occurrenceIdsAll.Count());
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Get_allOccurrenceIds_for_eventId()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            SearchFilter searchFilter = new SearchFilter(0)
            {
                EventIds = new List<string> { "urn:lsid:swedishlifewatch.se:dataprovider:Artportalen:event:10002293427000658739" }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var occurrenceIds = await _fixture.ProcessedObservationRepository.GetAllAggregationItemsAsync(searchFilter, "occurrence.occurrenceId");
            var occurrenceIds2 = await _fixture.ProcessedObservationRepository.GetAggregationItemsAsync(searchFilter, "occurrence.occurrenceId");

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            occurrenceIds.Should().NotBeNull();
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Get_EventOccurrenceIds_for_datastewardship_dataset()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            SearchFilter searchFilter = new SearchFilter(0)
            {
                DataStewardshipDatasetIds = new List<string> { "ArtportalenDataHost - Dataset Bats" }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var eventOccurrenceIds = await _fixture.ProcessedObservationRepository.GetEventOccurrenceItemsAsync(searchFilter);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            eventOccurrenceIds.Should().NotBeNull();
        }
    }
}