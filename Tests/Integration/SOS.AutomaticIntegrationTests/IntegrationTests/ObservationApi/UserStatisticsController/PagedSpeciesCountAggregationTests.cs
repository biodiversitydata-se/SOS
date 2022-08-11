using FizzWare.NBuilder;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.AutomaticIntegrationTests.TestFixtures;
using SOS.AutomaticIntegrationTests.TestDataBuilder;
using SOS.AutomaticIntegrationTests.Extensions;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Statistics;
using SOS.Observations.Api.Dtos;

namespace SOS.AutomaticIntegrationTests.IntegrationTests.ObservationApi.UserStatisticsController
{
    /// <summary>
    /// Tests that should be created.
    ///  - Handling observations with multiple observers.
    ///  - Test caching (The second request should be much faster)
    ///  - Test caching limit (There should be a limit of how much RAM that could be used by the cache)
    ///  - Test skip and take
    ///  - Test Taxon filter
    ///  - Test Year filter
    ///  - Test SpeciesGroup filter
    ///  - Test (AreaType, FeatureId) filter
    ///  - Test SiteId filter
    ///  - Test ProjectId filter
    ///  - Test combination of filter. I.e. (Taxon, Year), (SpeciesGroup, SiteId)
    ///  - Test SortBy
    /// </summary>
    [Collection(Constants.IntegrationTestsCollectionName)]
    public class PagedSpeciesCountAggregationTests
    {
        private readonly IntegrationTestFixture _fixture;

        public PagedSpeciesCountAggregationTests(IntegrationTestFixture fixture)
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
            //var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(16)
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
            var response = await _fixture.UserStatisticsController.PagedSpeciesCountAggregation(query, 0,5);
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

        [Fact]
        [Trait("Category", "AutomaticIntegrationTest")]
        public async Task Test_PagedSpeciesCountAggregation_with_IncludeOtherAreasSpeciesCount()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Create verbatim observations
            //-----------------------------------------------------------------------------------------------------------
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(36)
                .All()
                    .HaveValuesFromPredefinedObservations()
                .TheFirst(11) // 5 taxa
                    .HaveProperties(1,
                        new() { TaxonId = 1, ProvinceId = "P1" }, // 5 taxa in P1
                        new() { TaxonId = 1, ProvinceId = "P1" },
                        new() { TaxonId = 2, ProvinceId = "P1" },
                        new() { TaxonId = 3, ProvinceId = "P1" },
                        new() { TaxonId = 4, ProvinceId = "P1" },
                        new() { TaxonId = 5, ProvinceId = "P1" },
                        new() { TaxonId = 3, ProvinceId = "P2" }, // 2 taxa in P2
                        new() { TaxonId = 3, ProvinceId = "P2" },
                        new() { TaxonId = 5, ProvinceId = "P2" },
                        new() { TaxonId = 1, ProvinceId = "P3" }, // 2 taxa in P3
                        new() { TaxonId = 4, ProvinceId = "P3" })
                .TheNext(9) // 4 taxa
                    .HaveProperties(2,
                        new() { TaxonId = 1, ProvinceId = "P1" }, // 4 taxa in P1
                        new() { TaxonId = 2, ProvinceId = "P1" },
                        new() { TaxonId = 3, ProvinceId = "P1" },
                        new() { TaxonId = 4, ProvinceId = "P1" },
                        new() { TaxonId = 1, ProvinceId = "P2" }, // 3 taxa in P2
                        new() { TaxonId = 2, ProvinceId = "P2" },
                        new() { TaxonId = 3, ProvinceId = "P2" }, 
                        new() { TaxonId = 1, ProvinceId = "P3" }, // 2 taxa in P3
                        new() { TaxonId = 4, ProvinceId = "P3" })
                .TheNext(8) // 3 taxa
                    .HaveProperties(3,
                        new() { TaxonId = 1, ProvinceId = "P1" }, // 3 taxa in P1
                        new() { TaxonId = 2, ProvinceId = "P1" },
                        new() { TaxonId = 2, ProvinceId = "P1" },
                        new() { TaxonId = 3, ProvinceId = "P1" },
                        new() { TaxonId = 3, ProvinceId = "P1" },
                        new() { TaxonId = 3, ProvinceId = "P2" }, // 1 taxa in P2
                        new() { TaxonId = 3, ProvinceId = "P3" }, // 1 taxa in P3
                        new() { TaxonId = 3, ProvinceId = "P3" })
                .TheNext(4) // 2 taxa
                    .HaveProperties(4,
                        new() { TaxonId = 1, ProvinceId = "P1" }, // 2 taxa in P1
                        new() { TaxonId = 2, ProvinceId = "P1" },
                        new() { TaxonId = 1, ProvinceId = "P2" }, // 2 taxa in P2
                        new() { TaxonId = 2, ProvinceId = "P2" })
                .TheNext(4) // 1 taxa
                    .HaveProperties(5,
                        new() { TaxonId = 1, ProvinceId = "P1" }, // 1 taxa in P1
                        new() { TaxonId = 1, ProvinceId = "P1" },
                        new() { TaxonId = 1, ProvinceId = "P1" },
                        new() { TaxonId = 1, ProvinceId = "P4" }) // 1 taxa in P4
                .Build();

            await _fixture.ProcessAndAddUserObservationToElasticSearch(verbatimObservations);
            var query = new SpeciesCountUserStatisticsQuery
            {
                AreaType = AreaType.Province,
                IncludeOtherAreasSpeciesCount = true
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.UserStatisticsController.PagedSpeciesCountAggregation(
                query,
                0, 
                5,
                useCache: true);
            var result = response.GetResultObject<PagedResultDto<UserStatisticsItem>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------            
            var expected = new List<UserStatisticsItem>
            {
                new() { UserId = 1, SpeciesCount = 5, ObservationCount = 11, AreaCounts = new List<AreaSpeciesCount> { new("P1", 5), new("P2", 2), new("P3", 2) }},
                new() { UserId = 2, SpeciesCount = 4, ObservationCount = 9, AreaCounts = new List<AreaSpeciesCount> { new("P1", 4), new ("P2", 3), new ("P3", 2) }},
                new() { UserId = 3, SpeciesCount = 3, ObservationCount = 8, AreaCounts = new List<AreaSpeciesCount> { new("P1", 3), new ("P2", 1), new ("P3", 1) }},
                new() { UserId = 4, SpeciesCount = 2, ObservationCount = 4, AreaCounts = new List<AreaSpeciesCount> { new("P1", 2), new ("P2", 2) }},
                new() { UserId = 5, SpeciesCount = 1, ObservationCount = 4, AreaCounts = new List<AreaSpeciesCount> { new("P1", 1), new ("P4", 1) }},
            };

            result.Records.Should().BeEquivalentTo(expected);
        }

        [Fact]
        [Trait("Category", "AutomaticIntegrationTest")]
        public async Task Test_PagedSpeciesCountAggregation_sort_by_specific_area()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Create verbatim observations
            //-----------------------------------------------------------------------------------------------------------
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(36)
                .All()
                    .HaveValuesFromPredefinedObservations()
                .TheFirst(11) // 5 taxa
                    .HaveProperties(1,
                        new() { TaxonId = 1, ProvinceId = "P1" }, // 5 taxa in P1
                        new() { TaxonId = 1, ProvinceId = "P1" },
                        new() { TaxonId = 2, ProvinceId = "P1" },
                        new() { TaxonId = 3, ProvinceId = "P1" },
                        new() { TaxonId = 4, ProvinceId = "P1" },
                        new() { TaxonId = 5, ProvinceId = "P1" },
                        new() { TaxonId = 3, ProvinceId = "P2" }, // 2 taxa in P2
                        new() { TaxonId = 3, ProvinceId = "P2" },
                        new() { TaxonId = 5, ProvinceId = "P2" },
                        new() { TaxonId = 1, ProvinceId = "P3" }, // 2 taxa in P3
                        new() { TaxonId = 4, ProvinceId = "P3" })
                .TheNext(9) // 4 taxa
                    .HaveProperties(2,
                        new() { TaxonId = 1, ProvinceId = "P1" }, // 4 taxa in P1
                        new() { TaxonId = 2, ProvinceId = "P1" },
                        new() { TaxonId = 3, ProvinceId = "P1" },
                        new() { TaxonId = 4, ProvinceId = "P1" },
                        new() { TaxonId = 1, ProvinceId = "P2" }, // 3 taxa in P2
                        new() { TaxonId = 2, ProvinceId = "P2" },
                        new() { TaxonId = 3, ProvinceId = "P2" },
                        new() { TaxonId = 1, ProvinceId = "P3" }, // 2 taxa in P3
                        new() { TaxonId = 4, ProvinceId = "P3" })
                .TheNext(8) // 3 taxa
                    .HaveProperties(3,
                        new() { TaxonId = 1, ProvinceId = "P1" }, // 3 taxa in P1
                        new() { TaxonId = 2, ProvinceId = "P1" },
                        new() { TaxonId = 2, ProvinceId = "P1" },
                        new() { TaxonId = 3, ProvinceId = "P1" },
                        new() { TaxonId = 3, ProvinceId = "P1" },
                        new() { TaxonId = 3, ProvinceId = "P2" }, // 1 taxa in P2
                        new() { TaxonId = 3, ProvinceId = "P3" }, // 1 taxa in P3
                        new() { TaxonId = 3, ProvinceId = "P3" })
                .TheNext(4) // 2 taxa
                    .HaveProperties(4,
                        new() { TaxonId = 1, ProvinceId = "P1" }, // 2 taxa in P1
                        new() { TaxonId = 2, ProvinceId = "P1" },
                        new() { TaxonId = 1, ProvinceId = "P2" }, // 2 taxa in P2
                        new() { TaxonId = 2, ProvinceId = "P2" })
                .TheNext(4) // 1 taxa
                    .HaveProperties(5,
                        new() { TaxonId = 1, ProvinceId = "P1" }, // 1 taxa in P1
                        new() { TaxonId = 1, ProvinceId = "P1" },
                        new() { TaxonId = 1, ProvinceId = "P1" },
                        new() { TaxonId = 1, ProvinceId = "P4" }) // 1 taxa in P4
                .Build();

            await _fixture.ProcessAndAddUserObservationToElasticSearch(verbatimObservations);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.UserStatisticsController.PagedSpeciesCountAggregation(
                new SpeciesCountUserStatisticsQuery
                {
                    AreaType = AreaType.Province,
                    IncludeOtherAreasSpeciesCount = true
                },
                0,
                5,
                sortBy: "P2");
            var result = response.GetResultObject<PagedResultDto<UserStatisticsItem>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------            
            var expected = new List<UserStatisticsItem>
            {
                new() { UserId = 2, SpeciesCount = 4, ObservationCount = 9, AreaCounts = new List<AreaSpeciesCount> { new("P1", 4), new ("P2", 3), new ("P3", 2) }},
                new() { UserId = 1, SpeciesCount = 5, ObservationCount = 11, AreaCounts = new List<AreaSpeciesCount> { new("P1", 5), new("P2", 2), new("P3", 2) }},
                new() { UserId = 4, SpeciesCount = 2, ObservationCount = 4, AreaCounts = new List<AreaSpeciesCount> { new("P1", 2), new ("P2", 2) }},
                new() { UserId = 3, SpeciesCount = 3, ObservationCount = 8, AreaCounts = new List<AreaSpeciesCount> { new("P1", 3), new ("P2", 1), new ("P3", 1) }},
                //new() { UserId = 5, SpeciesCount = 1, ObservationCount = 4 }, // This user doesn't have any observations in P2, so it is excluded in the result
            };
            result.Records.Should().BeEquivalentTo(expected);

        }

        [Fact]
        [Trait("Category", "AutomaticIntegrationTest")]
        public async Task Test_PagedSpeciesCountAggregation_filter_by_specific_area()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Create verbatim observations
            //-----------------------------------------------------------------------------------------------------------
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(36)
                .All()
                    .HaveValuesFromPredefinedObservations()
                .TheFirst(11) // 5 taxa
                    .HaveProperties(1,
                        new() { TaxonId = 1, ProvinceId = "P1" }, // 5 taxa in P1
                        new() { TaxonId = 1, ProvinceId = "P1" },
                        new() { TaxonId = 2, ProvinceId = "P1" },
                        new() { TaxonId = 3, ProvinceId = "P1" },
                        new() { TaxonId = 4, ProvinceId = "P1" },
                        new() { TaxonId = 5, ProvinceId = "P1" },
                        new() { TaxonId = 3, ProvinceId = "P2" }, // 2 taxa in P2
                        new() { TaxonId = 3, ProvinceId = "P2" },
                        new() { TaxonId = 5, ProvinceId = "P2" },
                        new() { TaxonId = 1, ProvinceId = "P3" }, // 2 taxa in P3
                        new() { TaxonId = 4, ProvinceId = "P3" })
                .TheNext(9) // 4 taxa
                    .HaveProperties(2,
                        new() { TaxonId = 1, ProvinceId = "P1" }, // 4 taxa in P1
                        new() { TaxonId = 2, ProvinceId = "P1" },
                        new() { TaxonId = 3, ProvinceId = "P1" },
                        new() { TaxonId = 4, ProvinceId = "P1" },
                        new() { TaxonId = 1, ProvinceId = "P2" }, // 3 taxa in P2
                        new() { TaxonId = 2, ProvinceId = "P2" },
                        new() { TaxonId = 3, ProvinceId = "P2" },
                        new() { TaxonId = 1, ProvinceId = "P3" }, // 2 taxa in P3
                        new() { TaxonId = 4, ProvinceId = "P3" })
                .TheNext(8) // 3 taxa
                    .HaveProperties(3,
                        new() { TaxonId = 1, ProvinceId = "P1" }, // 3 taxa in P1
                        new() { TaxonId = 2, ProvinceId = "P1" },
                        new() { TaxonId = 2, ProvinceId = "P1" },
                        new() { TaxonId = 3, ProvinceId = "P1" },
                        new() { TaxonId = 3, ProvinceId = "P1" },
                        new() { TaxonId = 3, ProvinceId = "P2" }, // 1 taxa in P2
                        new() { TaxonId = 3, ProvinceId = "P3" }, // 1 taxa in P3
                        new() { TaxonId = 3, ProvinceId = "P3" })
                .TheNext(4) // 2 taxa
                    .HaveProperties(4,
                        new() { TaxonId = 1, ProvinceId = "P1" }, // 2 taxa in P1
                        new() { TaxonId = 2, ProvinceId = "P1" },
                        new() { TaxonId = 1, ProvinceId = "P2" }, // 2 taxa in P2
                        new() { TaxonId = 2, ProvinceId = "P2" })
                .TheNext(4) // 1 taxa
                    .HaveProperties(5,
                        new() { TaxonId = 1, ProvinceId = "P1" }, // 1 taxa in P1
                        new() { TaxonId = 1, ProvinceId = "P1" },
                        new() { TaxonId = 1, ProvinceId = "P1" },
                        new() { TaxonId = 1, ProvinceId = "P4" }) // 1 taxa in P4
                .Build();

            await _fixture.ProcessAndAddUserObservationToElasticSearch(verbatimObservations);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.UserStatisticsController.PagedSpeciesCountAggregation(
                new SpeciesCountUserStatisticsQuery
                {
                    AreaType = AreaType.Province,
                    FeatureId = "P2"
                },
                0,
                5);
            var result = response.GetResultObject<PagedResultDto<UserStatisticsItem>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------            
            var expected = new List<UserStatisticsItem>
            {
                new() { UserId = 2, SpeciesCount = 3, ObservationCount = 3 },
                new() { UserId = 1, SpeciesCount = 2, ObservationCount = 3 },
                new() { UserId = 4, SpeciesCount = 2, ObservationCount = 2 },
                new() { UserId = 3, SpeciesCount = 1, ObservationCount = 1 }
                //new() { UserId = 5, SpeciesCount = 0, ObservationCount = 0 } // This user doesn't have any observations in P2, so it is excluded in the result
            };
            result.Records.Should().BeEquivalentTo(expected);
        }

        [Fact(Skip="Run on demand because the test takes about 1 minute to run")]
        [Trait("Category", "AutomaticIntegrationTest")]
        public async Task Test_PagedSpeciesCount_with_large_amount_of_data()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Create verbatim observations
            //-----------------------------------------------------------------------------------------------------------
            const int maxNrBuckets = 65536; // This is the max number of buckets (in this case users) in Elasticsearch
            const int nrUsers = maxNrBuckets-1000;
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(nrUsers)
                .All()
                    .HaveValuesFromPredefinedObservations()
                .Build();

            for (var i = 0; i < nrUsers; i++)
            {
                var obs = verbatimObservations[i];
                int userId = i;
                obs.ObserversInternal = new List<UserInternal> {new() {Id = userId, UserServiceUserId = userId}};
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
                    .HaveProperties(nrUsers+1,
                        new() { TaxonId = 1 },
                        new() { TaxonId = 2 },
                        new() { TaxonId = 3 },
                        new() { TaxonId = 4 })
                .TheNext(5) // 5 observations , 3 taxa
                    .HaveProperties(nrUsers+2,
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
            var response = await _fixture.UserStatisticsController.PagedSpeciesCountAggregation(query, 0, 5, useCache:false);
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