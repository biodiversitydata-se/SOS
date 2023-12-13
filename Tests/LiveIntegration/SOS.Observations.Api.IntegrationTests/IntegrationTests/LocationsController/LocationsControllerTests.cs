using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using SOS.Shared.Api.Dtos.Enum;
using SOS.Shared.Api.Dtos.Filter;
using SOS.Shared.Api.Dtos.Location;
using SOS.Observations.Api.LiveIntegrationTests.Extensions;
using SOS.Observations.Api.LiveIntegrationTests.Fixtures;
using Xunit;

namespace SOS.Observations.Api.LiveIntegrationTests.IntegrationTests.LocationsController
{
    [Collection(Collections.ApiIntegrationTestsCollection)]
    public class LocationsControllerTests
    {
        private readonly ApiIntegrationTestFixture _fixture;

        public LocationsControllerTests(ApiIntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Search_for_locations_in_Uppsala_municipality()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            GeographicsFilterDto searchFilter = new GeographicsFilterDto
            {
                Areas = new[]{new AreaFilterDto
                {
                    AreaType = AreaTypeDto.Municipality,
                    FeatureId = "380"
                }}
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.LocationsController.SearchAsync(searchFilter);
            var result = response.GetResult<IEnumerable<LocationSearchResultDto>>().ToList();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().OnlyContain(l => l.Municipality == "Uppsala");
        }
    }
}
