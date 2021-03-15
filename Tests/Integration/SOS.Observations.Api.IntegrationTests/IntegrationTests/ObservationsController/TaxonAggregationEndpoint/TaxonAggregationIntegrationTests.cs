using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using SOS.Observations.Api.Dtos;
using SOS.Observations.Api.Dtos.Enum;
using SOS.Observations.Api.Dtos.Filter;
using SOS.Observations.Api.IntegrationTests.Extensions;
using SOS.Observations.Api.IntegrationTests.Fixtures;
using Xunit;

namespace SOS.Observations.Api.IntegrationTests.IntegrationTests.ObservationsController.TaxonAggregationEndpoint
{
    [Collection(Collections.ApiIntegrationTestsCollection)]
    public class TaxonAggregationIntegrationTests
    {
        private readonly ApiIntegrationTestFixture _fixture;

        public TaxonAggregationIntegrationTests(ApiIntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task TaxonAggregation()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var searchFilter = new SearchFilterAggregationDto()
            {
                Date = new DateFilterDto()
                {
                    StartDate = new DateTime(1990, 1, 31, 07, 59, 46),
                    EndDate = new DateTime(2020, 1, 31, 07, 59, 46)
                },
                OnlyValidated = false,
                OccurrenceStatus = OccurrenceStatusFilterValuesDto.Present
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.TaxonAggregationAsync(searchFilter, 0, 100);
            var result = response.GetResult<PagedResultDto<TaxonAggregationItemDto>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.TotalCount.Should().BeGreaterThan(30000, "There are observations on more than 30 000 taxa");
            result.Records.First().ObservationCount.Should().BeGreaterThan(100000,
                "The taxon with most observations has more than 100 000 observations");
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task TaxonAggregation_with_boundingbox()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var searchFilter = new SearchFilterAggregationDto()
            {
                Date = new DateFilterDto()
                {
                    StartDate = new DateTime(1990, 1, 31, 07, 59, 46),
                    EndDate = new DateTime(2020, 1, 31, 07, 59, 46)
                },
                OnlyValidated = false,
                OccurrenceStatus = OccurrenceStatusFilterValuesDto.Present
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.TaxonAggregationAsync(
                searchFilter,
                0,
                500,
                17.9296875,
                59.355596110016315,
                18.28125,
                59.17592824927137);
            var result = response.GetResult<PagedResultDto<TaxonAggregationItemDto>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.TotalCount.Should().BeGreaterThan(7500, "There are observations on more than 7 500 taxa inside the bounding box");
            result.Records.First().ObservationCount.Should().BeGreaterThan(2500,
                "The taxon with most observations inside the bounding box has more than 2 500 observations");
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task TaxonAggregation_paging_with_different_take_size_works_correctly_when_not_using_area_filter()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var searchFilter = new SearchFilterAggregationDto()
            {
                Taxon = new TaxonFilterDto()
                {
                    IncludeUnderlyingTaxa = true,
                    TaxonIds = new []{ 5000001 }
                },
                OccurrenceStatus = OccurrenceStatusFilterValuesDto.BothPresentAndAbsent
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act - take=100
            //-----------------------------------------------------------------------------------------------------------
            int skip = 0;
            int take = 100;
            var dictionaryTakeSize100 = new Dictionary<int, int>();
            var duplicateKeysTakeSize100 = new List<int>();
            do
            {
                var response = await _fixture.ObservationsController.TaxonAggregationAsync(searchFilter, skip, take);
                var result = response.GetResult<PagedResultDto<TaxonAggregationItemDto>>();
                foreach (var record in result.Records)
                {
                    if (dictionaryTakeSize100.ContainsKey(record.TaxonId))
                        duplicateKeysTakeSize100.Add(record.TaxonId);
                    else
                        dictionaryTakeSize100.Add(record.TaxonId, record.ObservationCount);
                }
                skip += take;
            } while (skip < 1000);
            int takeSize100Sum = dictionaryTakeSize100.Values.Sum();

            //-----------------------------------------------------------------------------------------------------------
            // Act - take=1000
            //-----------------------------------------------------------------------------------------------------------
            skip = 0;
            take = 1000;
            var dictionaryTakeSize1000 = new Dictionary<int, int>();
            var duplicateKeysTakeSize1000 = new List<int>();
            do
            {
                var response = await _fixture.ObservationsController.TaxonAggregationAsync(searchFilter, skip, take);
                var result = response.GetResult<PagedResultDto<TaxonAggregationItemDto>>();
                foreach (var record in result.Records)
                {
                    if (dictionaryTakeSize1000.ContainsKey(record.TaxonId))
                        duplicateKeysTakeSize1000.Add(record.TaxonId);
                    else
                        dictionaryTakeSize1000.Add(record.TaxonId, record.ObservationCount);
                }
                skip += take;
            } while (skip < 1000);
            int takeSize1000Sum = dictionaryTakeSize1000.Values.Sum();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            duplicateKeysTakeSize100.Count.Should().Be(0);
            duplicateKeysTakeSize1000.Count.Should().Be(0);
            dictionaryTakeSize100.Keys.Should().BeEquivalentTo(dictionaryTakeSize1000.Keys);
            takeSize100Sum.Should().Be(takeSize1000Sum);
        }

        /// <summary>
        /// When ShardSize is set to default value in ProcessedObservationRepository.GetTaxonAggregationAsync()
        /// this test will fail.
        ///
        /// When ShardSize is set to Size*10 this test will succeed. But setting ShardSize to Size*10 results in more
        /// resources being used in Elasticsearh to calculate the result. The correct way to do paging in aggregations
        /// is to use composite aggregations like in ProcessedObservationRepository.GetPageGeoTileTaxaAggregationAsync().
        /// https://www.elastic.co/guide/en/elasticsearch/reference/current/search-aggregations-bucket-composite-aggregation.html
        /// </summary>
        /// <returns></returns>
        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task TaxonAggregation_paging_with_different_take_size_works_incorrectly_when_using_area_filter()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var searchFilter = new SearchFilterAggregationDto()
            {
                Areas = new[]
                {
                    new AreaFilterDto() { AreaType = AreaTypeDto.BirdValidationArea, FeatureId = "1" }
                },
                OccurrenceStatus = OccurrenceStatusFilterValuesDto.BothPresentAndAbsent
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act - take=100
            //-----------------------------------------------------------------------------------------------------------
            int skip = 0;
            int take = 100;
            var dictionaryTakeSize100 = new Dictionary<int, int>();
            var duplicateKeysTakeSize100 = new List<int>();
            do
            {
                var response = await _fixture.ObservationsController.TaxonAggregationAsync(searchFilter, skip, take);
                var result = response.GetResult<PagedResultDto<TaxonAggregationItemDto>>();
                foreach (var record in result.Records)
                {
                    if (dictionaryTakeSize100.ContainsKey(record.TaxonId))
                        duplicateKeysTakeSize100.Add(record.TaxonId);
                    else
                        dictionaryTakeSize100.Add(record.TaxonId, record.ObservationCount);
                }
                skip += take;
            } while (skip < 1000);
            int takeSize100Sum = dictionaryTakeSize100.Values.Sum();

            //-----------------------------------------------------------------------------------------------------------
            // Act - take=1000
            //-----------------------------------------------------------------------------------------------------------
            skip = 0;
            take = 1000;
            var dictionaryTakeSize1000 = new Dictionary<int, int>();
            var duplicateKeysTakeSize1000 = new List<int>();
            do
            {
                var response = await _fixture.ObservationsController.TaxonAggregationAsync(searchFilter, skip, take);
                var result = response.GetResult<PagedResultDto<TaxonAggregationItemDto>>();
                foreach (var record in result.Records)
                {
                    if (dictionaryTakeSize1000.ContainsKey(record.TaxonId))
                        duplicateKeysTakeSize1000.Add(record.TaxonId);
                    else
                        dictionaryTakeSize1000.Add(record.TaxonId, record.ObservationCount);
                }
                skip += take;
            } while (skip < 1000);
            int takeSize1000Sum = dictionaryTakeSize1000.Values.Sum();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            duplicateKeysTakeSize100.Count.Should().Be(0);
            duplicateKeysTakeSize1000.Count.Should().Be(0);
            dictionaryTakeSize100.Keys.Should().BeEquivalentTo(dictionaryTakeSize1000.Keys);
            takeSize100Sum.Should().Be(takeSize1000Sum);
        }

    }
}
