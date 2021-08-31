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
                ValidationStatus = SearchFilterBaseDto.StatusValidationDto.BothValidatedAndNotValidated,
                OccurrenceStatus = OccurrenceStatusFilterValuesDto.Present
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearch(null, searchFilter, 0, 2);
            var result = response.GetResult<PagedResultDto<Observation>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Records.Count().Should().Be(2, "because the take parameter is 2");
            result.TotalCount.Should().BeGreaterThan(3500, "because there should be more than 3500 observations of otter");
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
                ValidationStatus = SearchFilterBaseDto.StatusValidationDto.BothValidatedAndNotValidated,
                OccurrenceStatus = OccurrenceStatusFilterValuesDto.Present
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearch(null, searchFilter, 0, 2);
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
            result.TotalCount.Should().BeGreaterThan(3500, "because there should be more than 3500 observations of otter");
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
    }
}
