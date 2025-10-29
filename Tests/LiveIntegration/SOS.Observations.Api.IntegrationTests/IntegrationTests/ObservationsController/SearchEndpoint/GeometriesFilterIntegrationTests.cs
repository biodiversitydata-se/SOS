using System.Threading.Tasks;
using FluentAssertions;
using SOS.Lib.Models.Processed.Observation;
using SOS.Shared.Api.Dtos;
using SOS.Shared.Api.Dtos.Enum;
using SOS.Shared.Api.Dtos.Filter;
using SOS.Observations.Api.LiveIntegrationTests.Extensions;
using SOS.Observations.Api.LiveIntegrationTests.Fixtures;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using NetTopologySuite.Geometries;

namespace SOS.Observations.Api.LiveIntegrationTests.IntegrationTests.ObservationsController.SearchEndpoint
{
    [Collection(Fixtures.Collections.ApiIntegrationTestsCollection)]
    public class GeometriesFilterIntegrationTests
    {
        private readonly ApiIntegrationTestFixture _fixture;

        public GeometriesFilterIntegrationTests(ApiIntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Search_with_point_geometry_filter()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            SearchFilterDto searchFilter = new SearchFilterDto
            {
                Taxon = new TaxonFilterDto { Ids = new List<int> { TestData.TaxonIds.Otter }, IncludeUnderlyingTaxa = true },
                Geographics = new GeographicsFilterDto
                {
                    Geometries = new List<Geometry> { new Point(14.96721, 58.01221) },
                    MaxDistanceFromPoint = 5000,
                    //ConsiderObservationAccuracy = true
                    ConsiderObservationAccuracy = false
                },
                VerificationStatus = StatusVerificationDto.BothVerifiedAndNotVerified,
                OccurrenceStatus = OccurrenceStatusFilterValuesDto.Present
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearch(null, null, searchFilter, 0, 2);
            var result = response.GetResult<PagedResultDto<Observation>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Records.First().Taxon.VernacularName.Should().Be("utter", "because otter has the swedish vernacular name 'utter'");
            result.Records.First().Location.Municipality.Name.Should().Be("Tranås", "because the Area search is limited to Tranås municipality");
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Search_with_polygon_geometry_filter()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            SearchFilterDto searchFilter = new SearchFilterDto
            {
                Taxon = new TaxonFilterDto { Ids = new List<int> { TestData.TaxonIds.Otter }, IncludeUnderlyingTaxa = true },
                Geographics = new GeographicsFilterDto
                {
                    Geometries = new List<Geometry>()
                    {
                        new Polygon(new LinearRing(
                                [
                                    new Coordinate(15.07063, 57.92573),
                                    new Coordinate(15.00510, 58.16108),
                                    new Coordinate(14.58003, 58.10148),
                                    new Coordinate(14.64143, 57.93294),
                                    new Coordinate(15.07063, 57.92573)
                                ]
                            )
                        )
                    },
                    ConsiderObservationAccuracy = true
                },
                VerificationStatus = StatusVerificationDto.BothVerifiedAndNotVerified,
                OccurrenceStatus = OccurrenceStatusFilterValuesDto.Present
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearch(null, null, searchFilter, 0, 2);
            var result = response.GetResult<PagedResultDto<Observation>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Records.First().Taxon.VernacularName.Should().Be("utter", "because otter has the swedish vernacular name 'utter'");
        }

    }
}