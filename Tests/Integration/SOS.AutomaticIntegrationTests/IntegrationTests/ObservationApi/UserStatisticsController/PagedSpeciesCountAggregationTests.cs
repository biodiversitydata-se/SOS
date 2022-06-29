using FizzWare.NBuilder;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
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
            var response = await _fixture.UserStatisticsController.PagedSpeciesCountAggregation(query, 0,5);
            var result = response.GetResultObject<PagedResultDto<UserStatisticsItem>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------            
            var expected = new List<UserStatisticsItem>
            {
                new() {UserId = 1, SpeciesCount = 5},
                new() {UserId = 2, SpeciesCount = 4},
                new() {UserId = 3, SpeciesCount = 3},
                new() {UserId = 4, SpeciesCount = 2},
                new() {UserId = 5, SpeciesCount = 1}
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

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
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
                new() { UserId = 1, SpeciesCount = 5, SpeciesCountByFeatureId = new Dictionary<string, int> {{"P1", 5}, {"P2", 2}, {"P3", 2}} },
                new() { UserId = 2, SpeciesCount = 4, SpeciesCountByFeatureId = new Dictionary<string, int> {{"P1", 4}, {"P2", 3}, {"P3", 2}} },
                new() { UserId = 3, SpeciesCount = 3, SpeciesCountByFeatureId = new Dictionary<string, int> {{"P1", 3}, {"P2", 1}, {"P3", 1}} },
                new() { UserId = 4, SpeciesCount = 2, SpeciesCountByFeatureId = new Dictionary<string, int> {{"P1", 2}, {"P2", 2}} },
                new() { UserId = 5, SpeciesCount = 1, SpeciesCountByFeatureId = new Dictionary<string, int> {{"P1", 1}, {"P4", 1}} }
            };

            result.Records.Should().BeEquivalentTo(expected);
        }

        [Fact(Skip="Implementation in controller is not ready yet. Because this test creates a lots of documents it should probably only be run on demand.")]
        [Trait("Category", "AutomaticIntegrationTest")]
        public async Task Test_ElasticsearchBucketSize65535IsNoProblem()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Create verbatim observations
            //-----------------------------------------------------------------------------------------------------------
            const int UsersCount = 100000;
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(UsersCount)
                .All()
                    .HaveValuesFromPredefinedObservations()
                    .HaveUniqueUserId()
                .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
            var query = new SpeciesCountUserStatisticsQuery();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.UserStatisticsController.PagedSpeciesCountAggregation(query, 0, UsersCount);
            var result = response.GetResultObject<PagedResultDto<UserStatisticsItem>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------            
            result.TotalCount.Should().Be(UsersCount);
            result.Records.Count().Should().Be(UsersCount);
            result.Records.Should().OnlyHaveUniqueItems();
        }
    }
}