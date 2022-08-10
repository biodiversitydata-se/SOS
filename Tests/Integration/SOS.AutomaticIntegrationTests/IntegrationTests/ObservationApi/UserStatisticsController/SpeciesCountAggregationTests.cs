using FizzWare.NBuilder;
using FluentAssertions;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.AutomaticIntegrationTests.TestFixtures;
using SOS.AutomaticIntegrationTests.TestDataBuilder;
using SOS.AutomaticIntegrationTests.Extensions;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Statistics;
using SOS.Observations.Api.Dtos;

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
        public async Task Test_PagedSpeciesCountAggregation()
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

            await _fixture.ProcessAndAddUserObservationToElasticSearch(verbatimObservations);
            var query = new SpeciesCountUserStatisticsQuery();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.UserStatisticsController.SpeciesCountAggregation(query, 0, 5);
            var result = response.GetResultObject<PagedResultDto<UserStatisticsItem>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------            
            var expected = new List<UserStatisticsItem>
            {
                new() {UserId = 1, SpeciesCount = 5, ObservationCount = 6},
                new() {UserId = 2, SpeciesCount = 4, ObservationCount = 4},
                new() {UserId = 3, SpeciesCount = 3, ObservationCount = 5},
                new() {UserId = 4, SpeciesCount = 2, ObservationCount = 2},
                new() {UserId = 5, SpeciesCount = 1, ObservationCount = 3}
            };

            result.Records.Should().BeEquivalentTo(expected);
        }

        [Fact(Skip = "Run on demand because the test takes about 1 minute to run")]
        [Trait("Category", "AutomaticIntegrationTest")]
        public async Task Test_SpeciesCount_with_large_amount_of_data()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Create verbatim observations
            //-----------------------------------------------------------------------------------------------------------
            const int nrUsers = 100000; // When using composite aggregation we can handle more than 65536 users.
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(nrUsers)
                .All()
                    .HaveValuesFromPredefinedObservations()
                .Build();

            for (var i = 0; i < nrUsers; i++)
            {
                var obs = verbatimObservations[i];
                int userId = i;
                obs.ObserversInternal = new List<UserInternal> { new() { Id = userId, UserServiceUserId = userId } };
                obs.TaxonId = 1;
            }

            var verbatimObservations2 = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(15)
                .All()
                    .HaveValuesFromPredefinedObservations()
                .TheFirst(6) // 6 observations, 5 taxa
                    .HaveProperties(nrUsers,
                        new() { TaxonId = 1 },
                        new() { TaxonId = 1 },
                        new() { TaxonId = 2 },
                        new() { TaxonId = 3 },
                        new() { TaxonId = 4 },
                        new() { TaxonId = 5 })
                .TheNext(4) // 4 observations , 4 taxa
                    .HaveProperties(nrUsers + 1,
                        new() { TaxonId = 1 },
                        new() { TaxonId = 2 },
                        new() { TaxonId = 3 },
                        new() { TaxonId = 4 })
                .TheNext(5) // 5 observations , 3 taxa
                    .HaveProperties(nrUsers + 2,
                        new() { TaxonId = 1 },
                        new() { TaxonId = 2 },
                        new() { TaxonId = 2 },
                        new() { TaxonId = 3 },
                        new() { TaxonId = 3 })
                .Build();

            await _fixture.ProcessAndAddUserObservationToElasticSearch(verbatimObservations.Union(verbatimObservations2));
            Thread.Sleep(5000);
            var query = new SpeciesCountUserStatisticsQuery();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.UserStatisticsController.SpeciesCountAggregation(query, 0, 5, useCache: true);
            var result = response.GetResultObject<PagedResultDto<UserStatisticsItem>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------            
            var expected = new List<UserStatisticsItem>
            {
                new() {UserId = nrUsers, SpeciesCount = 5, ObservationCount = 6},
                new() {UserId = nrUsers+1, SpeciesCount = 4, ObservationCount = 4},
                new() {UserId = nrUsers+2, SpeciesCount = 3, ObservationCount = 5},
                new() {UserId = 0, SpeciesCount = 1, ObservationCount = 1},
                new() {UserId = 1, SpeciesCount = 1, ObservationCount = 1}
            };

            result.Records.Should().BeEquivalentTo(expected);
        }
    }
}