﻿using FluentAssertions;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.Observation;
using SOS.Observations.Api.Dtos.Enum;
using SOS.Observations.Api.LiveIntegrationTests.Extensions;
using SOS.Observations.Api.LiveIntegrationTests.Fixtures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SOS.Observations.Api.LiveIntegrationTests.IntegrationTests.ObservationsController.ObservationByIdEndpoint
{
    [Collection(Collections.ApiIntegrationTestsCollection)]
    public class GetObservationByIdInternalTests
    {
        private readonly ApiIntegrationTestFixture _fixture;

        public GetObservationByIdInternalTests(ApiIntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task GetObservationById()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------            
            string occurrenceId = "urn:lsid:artportalen.se:Sighting:80354827";

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.GetObservationByIdInternal(null,
                null,
                occurrenceId,
                null,
                OutputFieldSet.All,
                "sv-SE",
                false,
                false);

            var result = response.GetResult<object>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().NotBeNull();
        }
    }
}
