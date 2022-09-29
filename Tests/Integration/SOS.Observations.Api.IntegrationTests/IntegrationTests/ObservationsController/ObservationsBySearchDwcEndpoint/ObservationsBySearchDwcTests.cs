using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using SOS.Lib.JsonConverters;
using SOS.Lib.Models.Processed.Observation;
using SOS.Observations.Api.Dtos;
using SOS.Observations.Api.Dtos.Enum;
using SOS.Observations.Api.Dtos.Filter;
using SOS.Observations.Api.Dtos.Observation;
using SOS.Observations.Api.IntegrationTests.Extensions;
using SOS.Observations.Api.IntegrationTests.Fixtures;
using Xunit;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace SOS.Observations.Api.IntegrationTests.IntegrationTests.ObservationsController.ObservationsBySearchDwcEndpoint
{
    [Collection(Collections.ApiIntegrationTestsCollection)]
    public class ObservationsBySearchDwcTests
    {
        private readonly ApiIntegrationTestFixture _fixture;

        public ObservationsBySearchDwcTests(ApiIntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Search_for_Plantae()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearchDwc(null, 
                null, 
                "Plantae", 
                null,
                null,
                null,                
                null,
                null,
                null,
                null,
                null,
                null,
                "en-GB",
                false,
                0,
                10);

            var result = response.GetResult<IEnumerable<DarwinCoreOccurrenceDto>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Count().Should().Be(10, "because the take parameter is 10");
            var obs = result.First();
            obs.OccurrenceID.Should().NotBeNullOrEmpty();
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Search_by_scientificName()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearchDwc(null,
                null,
                null,
                null,
                null,
                "Tussilago farfara",
                null,
                null,
                null,
                null,
                null,
                null,
                "en-GB",
                false,
                0,
                10);

            var result = response.GetResult<IEnumerable<DarwinCoreOccurrenceDto>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Count().Should().Be(10, "because the take parameter is 10");
            var obs = result.First();
            obs.OccurrenceID.Should().NotBeNullOrEmpty();
        }
    }
}
