using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.Observation;
using SOS.Shared.Api.Dtos;
using SOS.Shared.Api.Dtos.Enum;
using SOS.Shared.Api.Dtos.Filter;
using SOS.Observations.Api.LiveIntegrationTests.Extensions;
using SOS.Observations.Api.LiveIntegrationTests.Fixtures;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace SOS.Observations.Api.LiveIntegrationTests.IntegrationTests.ObservationsController.GeoJson
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
                VerificationStatus = StatusVerificationDto.BothVerifiedAndNotVerified,
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
                null,
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

            //await System.IO.File.WriteAllTextAsync(@"c:\gis\public-observations-point.geojson", result.GeoJson);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.TotalCount.Should().BeGreaterThan(0);
        }
    }
}

