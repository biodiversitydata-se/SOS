using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using SOS.Shared.Api.Dtos.Observation;
using SOS.Observations.Api.LiveIntegrationTests.Extensions;
using SOS.Observations.Api.LiveIntegrationTests.Fixtures;
using Xunit;

namespace SOS.Observations.Api.LiveIntegrationTests.IntegrationTests.ObservationsController.ObservationsBySearchDwcEndpoint
{
    [Collection(Fixtures.Collections.ApiIntegrationTestsCollection)]
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
