using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.Observation;
using SOS.Observations.Api.Dtos;
using SOS.Observations.Api.Dtos.Filter;
using SOS.Observations.Api.IntegrationTests.Fixtures;
using Xunit;

namespace SOS.Observations.Api.IntegrationTests.IntegrationTests.ObservationManager
{
    [Collection(Collections.ApiIntegrationTestsCollection)]
    public class DuplicatesIntegrationTests
    {
        private readonly ApiIntegrationTestFixture _fixture;

        public DuplicatesIntegrationTests(ApiIntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Check_for_public_observation_duplicates()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var duplicates = await _fixture.ObservationManager.TryToGetOccurenceIdDuplicatesAsync(false, 100);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            duplicates.Should().BeEmpty();
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Check_for_sensitive_observation_duplicates()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var duplicates = await _fixture.ObservationManager.TryToGetOccurenceIdDuplicatesAsync(true, 100);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            duplicates.Should().BeEmpty();
        }
    }
}
