using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using SOS.Observations.Api.Dtos.Enum;
using SOS.Observations.Api.Dtos.Filter;
using SOS.Observations.Api.Dtos.Location;
using SOS.Observations.Api.IntegrationTests.Extensions;
using SOS.Observations.Api.IntegrationTests.Fixtures;
using Xunit;

namespace SOS.Observations.Api.IntegrationTests.IntegrationTests.LocationsController
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
                Areas = new []{new AreaFilterDto
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
