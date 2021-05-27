using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.Observation;
using SOS.Observations.Api.Dtos;
using SOS.Observations.Api.Dtos.Filter;
using SOS.Observations.Api.IntegrationTests.Extensions;
using SOS.Observations.Api.IntegrationTests.Fixtures;
using Xunit;

namespace SOS.Observations.Api.IntegrationTests.IntegrationTests.ObservationsController.GeoJson
{
    [Collection(Collections.ApiIntegrationTestsCollection)]
    public class PublicSpeciesGeoJsonIntegrationTests
    {
        private readonly ApiIntegrationTestFixture _fixture;

        public PublicSpeciesGeoJsonIntegrationTests(ApiIntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Get_GeoJson_with_Point_as_geometry()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var searchFilter = new SearchFilterInternalDto
            {
                ValidationStatus = SearchFilterBaseDto.StatusValidationDto.BothValidatedAndNotValidated,
                OccurrenceStatus = OccurrenceStatusFilterValuesDto.Present,
                Geographics = new GeographicsFilterDto
                {
                    Areas = new List<AreaFilterDto>
                    {
                        TestData.Areas.TranasMunicipality
                    },
                    ConsiderObservationAccuracy = true
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearchInternal(
                "",
                searchFilter,
                0,
                1000,
                "",
                SearchSortOrder.Asc,
                false,
                "sv-SE",
                false,
                OutputFormatDto.GeoJsonFlat);
            var result = response.GetResult<GeoPagedResultDto<Observation>>();

            await System.IO.File.WriteAllTextAsync(@"c:\gis\public-observations-point.geojson", result.GeoJson);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.TotalCount.Should().BeGreaterThan(0);
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Get_GeoJson_with_PointWithBuffer_as_geometry()
        {
            // Do the following changes to the code to export observations as GeoJSON with the pointWithBuffer as geometry:
            // 1. Change SOS.Lib.Extensions.SearchExtensions.ToProjection(). Comment out the exclude row:
            //    .Field("location.pointWithBuffer")
            // 2. Change SOS.Lib.Helpers.GeoJsonHelper.GetFeature() to use GeoJsonGeometryType.PointWithBuffer
            
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var searchFilter = new SearchFilterInternalDto
            {
                ValidationStatus = SearchFilterBaseDto.StatusValidationDto.BothValidatedAndNotValidated,
                OccurrenceStatus = OccurrenceStatusFilterValuesDto.Present,
                Geographics = new GeographicsFilterDto
                {
                    Areas = new List<AreaFilterDto>
                    {
                        TestData.Areas.TranasMunicipality
                    },
                    ConsiderObservationAccuracy = true
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearchInternal(
                "",
                searchFilter,
                0,
                10000,
                "",
                SearchSortOrder.Asc,
                false,
                "sv-SE",
                true,
                OutputFormatDto.GeoJsonFlat);
            var result = response.GetResult<GeoPagedResultDto<Observation>>();

            await System.IO.File.WriteAllTextAsync(@"c:\gis\public-observations-pointWithBuffer.geojson", result.GeoJson);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.TotalCount.Should().BeGreaterThan(0);
        }
    }
}

