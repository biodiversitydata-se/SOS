using FizzWare.NBuilder;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.AutomaticIntegrationTests.TestFixtures;
using SOS.AutomaticIntegrationTests.TestDataBuilder;
using SOS.AutomaticIntegrationTests.Extensions;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Statistics;

namespace SOS.AutomaticIntegrationTests.IntegrationTests.ObservationApi.UserStatisticsController
{
    [Collection(Constants.IntegrationTestsCollectionName)]
    public class SpeciesCountAggregationTests
    {
        private readonly IntegrationTestFixture _fixture;

        public SpeciesCountAggregationTests(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        [Trait("Category", "AutomaticIntegrationTest")]
        public async Task SpeciesCountAggregationTest()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Create verbatim observations
            //-----------------------------------------------------------------------------------------------------------
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(18)
                .All()
                    .HaveValuesFromPredefinedObservations()
                .TheFirst(5)
                    .HaveObserversWithId(1)
                .TheNext(4)
                    .HaveObserversWithId(2)
                .TheNext(4)
                    .HaveObserversWithId(3)
                .TheNext(2)
                    .HaveObserversWithId(4)
                .TheLast(1)
                    .HaveObserversWithId(5)
                .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);

            //-----------------------------------------------------------------------------------------------------------
            // Act - Get observation by occurrenceId
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.UserStatisticsController.SpeciesCountAggregation();
            var result = response.GetResultObject<IEnumerable<UserStatisticsItem>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------            
            var expected = new List<UserStatisticsItem>
            {
                new() {UserId = 1, SpeciesCount = 5},
                new() {UserId = 2, SpeciesCount = 4},
                new() {UserId = 3, SpeciesCount = 4},
                new() {UserId = 4, SpeciesCount = 2},
                new() {UserId = 5, SpeciesCount = 1}
            };

            result.Should().BeEquivalentTo(expected);
        }
    }
}