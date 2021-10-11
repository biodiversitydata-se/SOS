using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Nest;
using SOS.Lib.Models.Processed.Observation;
using SOS.Observations.Api.Dtos;
using SOS.Observations.Api.Dtos.Filter;
using SOS.Observations.Api.IntegrationTests.Extensions;
using SOS.Observations.Api.IntegrationTests.Fixtures;
using Xunit;

namespace SOS.Observations.Api.IntegrationTests.IntegrationTests.ObservationsController.SearchEndpoint
{
    [Collection(Collections.ApiIntegrationTestsCollection)]
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
                    Geometries = new List<IGeoShape> { new PointGeoShape(new GeoCoordinate(58.01221, 14.96721)) },
                    MaxDistanceFromPoint = 5000,
                    //ConsiderObservationAccuracy = true
                    ConsiderObservationAccuracy = false
                },
                ValidationStatus = SearchFilterBaseDto.StatusValidationDto.BothValidatedAndNotValidated,
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
                    ConsiderObservationAccuracy = true
                },
                ValidationStatus = SearchFilterBaseDto.StatusValidationDto.BothValidatedAndNotValidated,
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