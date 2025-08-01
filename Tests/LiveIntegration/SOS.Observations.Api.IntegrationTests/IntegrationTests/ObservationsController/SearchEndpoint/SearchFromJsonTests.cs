﻿using FluentAssertions;
using SOS.Lib.JsonConverters;
using SOS.Lib.Models.Processed.Observation;
using SOS.Observations.Api.LiveIntegrationTests.Extensions;
using SOS.Observations.Api.LiveIntegrationTests.Fixtures;
using SOS.Shared.Api.Dtos;
using SOS.Shared.Api.Dtos.Filter;
using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Xunit;

using JsonSerializer = System.Text.Json.JsonSerializer;

namespace SOS.Observations.Api.LiveIntegrationTests.IntegrationTests.ObservationsController.SearchEndpoint
{
    [Collection(Collections.ApiIntegrationTestsCollection)]
    public class SearchFromJsonTests
    {
        private readonly ApiIntegrationTestFixture _fixture;
        
        public SearchFromJsonTests(ApiIntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        private readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions()
        {
            Converters = {
                    new NetTopologySuite.IO.Converters.GeoJsonConverterFactory(),
                    new JsonStringEnumConverter()
                },
            PropertyNameCaseInsensitive = true
        };

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Search_with_json_filter()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            string strFilter = await System.IO.File.ReadAllTextAsync(@"C:\Temp\sos-search-filter.json");
            var filter = JsonSerializer.Deserialize<SearchFilterDto>(strFilter, _jsonSerializerOptions);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearch(null, null, filter, 0, 2);
            var result = response.GetResult<PagedResultDto<Observation>>();
            
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Records.Count().Should().Be(2, "because the take parameter is 2");
        }


    }
}
