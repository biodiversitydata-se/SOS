using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.Observation;
using SOS.Observations.Api.Dtos;
using SOS.Observations.Api.Dtos.Filter;
using SOS.Observations.Api.IntegrationTests.Extensions;
using SOS.Observations.Api.IntegrationTests.Fixtures;
using Xunit;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using SOS.Lib.JsonConverters;
using SOS.Observations.Api.Dtos.Enum;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace SOS.Observations.Api.IntegrationTests.IntegrationTests.ObservationsController.SearchEndpoint
{
    [Collection(Collections.ApiIntegrationTestsCollection)]
    public class DateFilterIntegrationTests
    {
        private readonly ApiIntegrationTestFixture _fixture;

        public DateFilterIntegrationTests(ApiIntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Search_for_Otter_with_date_filter()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            SearchFilterDto searchFilter = new SearchFilterDto
            {
                Taxon = new TaxonFilterDto { Ids = new List<int>() { TestData.TaxonIds.Otter }, IncludeUnderlyingTaxa = true },
                Date = new DateFilterDto
                {
                    StartDate = new DateTime(1990, 1, 31, 07, 59, 46),
                    EndDate = new DateTime(2020, 1, 31, 07, 59, 46)
                },
                VerificationStatus = SearchFilterBaseDto.StatusVerificationDto.BothVerifiedAndNotVerified,
                OccurrenceStatus = OccurrenceStatusFilterValuesDto.Present
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearch(null, null, searchFilter, 0, 2);
            var result = response.GetResult<PagedResultDto<Observation>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Records.Count().Should().Be(2, "because the take parameter is 2");
            result.TotalCount.Should().BeGreaterThan(1800, "because there should be more than 1800 observations of otter");
            result.Records.First().Taxon.Id.Should().Be(100077, "because otter has TaxonId=100077");
        }

        [Fact]
        public async Task Search_for_Otter_with_date_filter_save_as_json_file()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            // If you want all fields, then comment the exlude rows in SearchExtensions.ToProjection()
            SearchFilterDto searchFilter = new SearchFilterDto
            {
                Taxon = new TaxonFilterDto { Ids = new List<int>() { TestData.TaxonIds.Otter }, IncludeUnderlyingTaxa = true },
                Date = new DateFilterDto
                {
                    StartDate = new DateTime(1990, 1, 31, 07, 59, 46),
                    EndDate = new DateTime(2020, 1, 31, 07, 59, 46)
                },
                VerificationStatus = SearchFilterBaseDto.StatusVerificationDto.BothVerifiedAndNotVerified,
                OccurrenceStatus = OccurrenceStatusFilterValuesDto.Present
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearch(null, null, searchFilter, 0, 2);
            var result = GetResult<PagedResultDto<Observation>>(response);
            string fileName = @"C:\Temp\SosObservationsExport.json";
            var jsonSerializerOptions = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
            jsonSerializerOptions.Converters.Add(new GeoShapeConverter());
            jsonSerializerOptions.Converters.Add(new GeoLocationConverter());
            string jsonString = JsonSerializer.Serialize(result.Records, jsonSerializerOptions);
            await System.IO.File.WriteAllTextAsync(fileName, jsonString);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Records.Count().Should().Be(2, "because the take parameter is 2");
            result.TotalCount.Should().BeGreaterThan(1800, "because there should be more than 1800 observations of otter");
            result.Records.First().Taxon.Id.Should().Be(100077, "because otter has TaxonId=100077");
        }

        private T GetResult<T>(IActionResult response)
        {
            var okObjectResult = (OkObjectResult)response;
            var jsonSerializerOptions = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
            jsonSerializerOptions.Converters.Add(new GeoShapeConverter());
            jsonSerializerOptions.Converters.Add(new GeoLocationConverter());
            var strJson = JsonSerializer.Serialize(okObjectResult.Value, jsonSerializerOptions);
            var result = JsonSerializer.Deserialize<T>(strJson, jsonSerializerOptions);
            return result;
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Search_Artportalen_observations_by_month_may_and_june()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var searchFilter = new SearchFilterInternalDto
            {
                DataProvider = new DataProviderFilterDto { Ids = new List<int> {1}},
                Taxon = new TaxonFilterDto
                {
                    IncludeUnderlyingTaxa = false,
                    Ids = new []{100008, 100009, 100019, 102928, 102929, 103058, 103059, 205924, 205925,
                        205926, 205927, 232125, 232268, 232269, 233777, 233778, 233813, 233814,
                        233815, 233816, 233837, 233838, 233880, 266225, 266226, 266227, 266228,
                        266229, 266230, 266231, 266232, 266233, 266234, 266235, 266236, 266735,
                        266736, 266737, 266738, 266739, 266740, 266741, 266742, 266743, 266744,
                        266745, 266746, 266747, 266767, 266794, 267093, 267094, 267114, 267295,
                        267296, 1001443, 1001444, 6006072, 6007921, 6052760
                    }
                },
                Date = new DateFilterDto
                {
                    StartDate = new DateTime(2021, 1, 1, 0, 0, 0),
                    EndDate = new DateTime(2021, 12, 31,23,59,59),
                    DateFilterType = DateFilterTypeDto.OverlappingStartDateAndEndDate
                },
                DiffusionStatuses = new [] {DiffusionStatusDto.NotDiffused},
                IncludeRealCount = true,
                DeterminationFilter = SearchFilterBaseDto.SightingDeterminationFilterDto.NotUnsureDetermination,
                NotRecoveredFilter = SearchFilterBaseDto.SightingNotRecoveredFilterDto.NoFilter,
                ExtendedFilter = new ExtendedFilterDto()
                {
                    UsePeriodForAllYears = false,
                    MonthsComparison = ExtendedFilterDto.DateFilterComparisonDto.StartDateOrEndDate,
                    Months = new[] { 5, 6 },
                    TypeFilter = ExtendedFilterDto.SightingTypeFilterDto.DoNotShowMerged,
                    ExcludeVerificationStatusIds = new [] { 50, 13 },
                    UnspontaneousFilter = ExtendedFilterDto.SightingUnspontaneousFilterDto.NoFilter,
                    NotPresentFilter = ExtendedFilterDto.SightingNotPresentFilterDto.DontIncludeNotPresent,
                    SiteIds = new []{ 2245350 }
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearchInternal(null, null, searchFilter, 0, 2);
            var result = response.GetResult<PagedResultDto<Observation>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.TotalCount.Should().Be(98);
        }
    }
}
