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
        public async Task TestSpeciesCountAggregation()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Create verbatim observations
            //-----------------------------------------------------------------------------------------------------------
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(20)
                .All()
                    .HaveValuesFromPredefinedObservations()
                .TheFirst(6) // 6 observations, 5 taxa
                    .HaveProperties(1,
                        new() { TaxonId = 1 },
                        new() { TaxonId = 1 },
                        new() { TaxonId = 2 },
                        new() { TaxonId = 3 },
                        new() { TaxonId = 4 },
                        new() { TaxonId = 5 })
                .TheNext(4) // 4 observations , 4 taxa
                    .HaveProperties(2, 
                        new() { TaxonId = 1 }, 
                        new() { TaxonId = 2 }, 
                        new() { TaxonId = 3 }, 
                        new() { TaxonId = 4 })
                .TheNext(5) // 5 observations , 3 taxa
                    .HaveProperties(3,
                        new() { TaxonId = 1 },
                        new() { TaxonId = 2 },
                        new() { TaxonId = 2 },
                        new() { TaxonId = 3 },
                        new() { TaxonId = 3 })
                .TheNext(2) // 2 observations , 2 taxa
                    .HaveProperties(4,
                        new() { TaxonId = 1 },
                        new() { TaxonId = 2 })
                .TheNext(3) // 3 observations , 1 taxa
                    .HaveProperties(5,
                        new() { TaxonId = 1 },
                        new() { TaxonId = 1 },
                        new() { TaxonId = 1 })
                .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
            var query = new SpeciesCountUserStatisticsQuery();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.UserStatisticsController.SpeciesCountAggregation(query);
            var result = response.GetResultObject<IEnumerable<UserStatisticsItem>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------            
            var expected = new List<UserStatisticsItem>
            {
                new() { UserId = 1, SpeciesCount = 5 },
                new() { UserId = 2, SpeciesCount = 4 },
                new() { UserId = 3, SpeciesCount = 3 },
                new() { UserId = 4, SpeciesCount = 2 },
                new() { UserId = 5, SpeciesCount = 1 }
            };

            result.Should().BeEquivalentTo(expected);
        }
    }
}