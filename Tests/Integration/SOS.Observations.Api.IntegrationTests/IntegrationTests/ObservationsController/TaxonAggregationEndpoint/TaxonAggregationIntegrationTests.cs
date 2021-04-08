using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            var response = await _fixture.ObservationsController.TaxonAggregation(searchFilter, 0, 100);
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
            var response = await _fixture.ObservationsController.TaxonAggregation(
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

        /// <summary>
        /// Get 1000 records by paging and compare with getting all records.
        /// </summary>
        /// <returns></returns>
        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task TaxonAggregation_paging_first_1000_records_gives_same_result_as_take_1000_records_from_all_records()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var searchFilter = new SearchFilterAggregationDto()
            {
                Areas = new[]
                {
                    new AreaFilterDto { AreaType = AreaTypeDto.BirdValidationArea, FeatureId = "1" }
                },
                OccurrenceStatus = OccurrenceStatusFilterValuesDto.BothPresentAndAbsent
            };

            //-----------------------------------------------------------------------------------------------------------
            // Warmup for benchmark result.
            //-----------------------------------------------------------------------------------------------------------
            await _fixture.ObservationsController.TaxonAggregation(searchFilter, 0, 10);

            //-----------------------------------------------------------------------------------------------------------
            // Act - Paging. Get 100 in each page.
            //-----------------------------------------------------------------------------------------------------------
            var spPaging = Stopwatch.StartNew();
            int skip = 0;
            int take = 100;
            var dictionaryTakeSize100 = new Dictionary<int, int>();
            var duplicateKeysTakeSize100 = new List<int>();
            do
            {
                var response = await _fixture.ObservationsController.TaxonAggregation(searchFilter, skip, take);
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
            spPaging.Stop();

            //-----------------------------------------------------------------------------------------------------------
            // Act - Get all records in one request.
            //-----------------------------------------------------------------------------------------------------------
            var spGetAll = Stopwatch.StartNew();
            var getAllResponse = await _fixture.ObservationsController.TaxonAggregation(searchFilter, null, null);
            var getAllResult = getAllResponse.GetResult<PagedResultDto<TaxonAggregationItemDto>>();
            var dictionaryTake1000FromAll = new Dictionary<int, int>();
            var duplicateKeysTake1000FromAll = new List<int>();
            foreach (var record in getAllResult.Records.Take(1000))
            {
                if (dictionaryTake1000FromAll.ContainsKey(record.TaxonId))
                    duplicateKeysTake1000FromAll.Add(record.TaxonId);
                else
                    dictionaryTake1000FromAll.Add(record.TaxonId, record.ObservationCount);
            }
            int take1000FromAllSum = dictionaryTake1000FromAll.Values.Sum();
            spGetAll.Stop();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            duplicateKeysTakeSize100.Count.Should().Be(0);
            duplicateKeysTake1000FromAll.Count.Should().Be(0);
            dictionaryTakeSize100.Keys.Should().BeEquivalentTo(dictionaryTake1000FromAll.Keys);
            takeSize100Sum.Should().Be(take1000FromAllSum);
            spGetAll.ElapsedMilliseconds.Should().BeLessThan(spPaging.ElapsedMilliseconds,
                "because getting all in one request is faster than paging 10 times.");
        }
    }
}
