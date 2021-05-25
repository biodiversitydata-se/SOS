using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Nest;
using SOS.Lib.Enums;
using SOS.Observations.Api.Dtos;
using SOS.Observations.Api.Dtos.Filter;
using SOS.Observations.Api.IntegrationTests.Extensions;
using SOS.Observations.Api.IntegrationTests.Fixtures;
using Xunit;

namespace SOS.Observations.Api.IntegrationTests.IntegrationTests.ObservationsController.SearchEndpoint
{
    [Collection(Collections.ApiIntegrationTestsCollection)]
    public class SignalSearchIntegrationTests
    {
        private readonly ApiIntegrationTestFixture _fixture;

        public SignalSearchIntegrationTests(ApiIntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Test_SignalSearch()
        {
            // To test with a specific user, change SOS.Lib.Managers.FilterManager.AddAuthorizationAsync() to use
            // Remove: var user = await _userService.GetUserAsync();
            // Add:    var user = await _userService.GetUserByIdAsync([userId]);

            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var searchFilter = new SignalFilterDto()
            {
                Geographics = new GeographicsFilterDto()
                {
                    //BoundingBox = new LatLonBoundingBoxDto
                    //{
                    //    TopLeft = new LatLonCoordinateDto {Latitude = 90, Longitude = -180},
                    //    BottomRight = new LatLonCoordinateDto {Latitude = -90, Longitude = 180},
                    //},
                    Geometries = new List<IGeoShape>()
                    {
                        new PolygonGeoShape(new List<List<GeoCoordinate>> { new List<GeoCoordinate>
                            {
                                new GeoCoordinate(57.92573, 15.07063),
                                new GeoCoordinate(58.16108, 15.00510),
                                new GeoCoordinate(58.10148, 14.58003),
                                new GeoCoordinate(57.93294, 14.64143),
                                new GeoCoordinate(57.92573, 15.07063)
                            }
                        })
                    },
                    ConsiderObservationAccuracy = false,
                    ConsiderDisturbanceRadius = false
                },
                Taxon = new TaxonFilterBaseDto()
                {
                    Ids = new List<int> { TestData.TaxonIds.Wolf },
                    IncludeUnderlyingTaxa = true,
                    TaxonListIds = new []{ (int)TaxonListId.RedlistedSpecies }
                },
                BirdNestActivityLimit = 10,
                StartDate = new DateTime(2010,1,1)
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.SignalSearchInternalAsync(
                "CountyAdministrationObservation",
                searchFilter,
                false,
                0,
                true);
            var result = response.GetResult<bool>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().BeTrue();
        }
    }

}
