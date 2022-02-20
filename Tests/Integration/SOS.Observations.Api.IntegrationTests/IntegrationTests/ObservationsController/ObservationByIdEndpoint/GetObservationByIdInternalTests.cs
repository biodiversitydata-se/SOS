using FluentAssertions;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.Observation;
using SOS.Observations.Api.Dtos.Enum;
using SOS.Observations.Api.IntegrationTests.Extensions;
using SOS.Observations.Api.IntegrationTests.Fixtures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SOS.Observations.Api.IntegrationTests.IntegrationTests.ObservationsController.ObservationByIdEndpoint
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
        public async Task Get_observation_by_using_Artportalen_fetch_mode()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------            
            string occurrenceId = "urn:lsid:artportalen.se:Sighting:67611684";

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.GetObservationByIdInternal(null, 
                null, 
                occurrenceId, 
                null, 
                OutputFieldSet.Minimum, 
                "sv-SE", 
                false, 
                ArtportalenFetchModeDto.Artportalen);
            
            var result = response.GetResult<object>();            

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().NotBeNull();            
        }
    }
}
