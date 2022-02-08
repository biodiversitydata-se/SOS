using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Nest;
using NetTopologySuite.Features;
using SOS.Observations.Api.Dtos;
using SOS.Observations.Api.Dtos.Filter;
using SOS.Observations.Api.IntegrationTests.Extensions;
using SOS.Observations.Api.IntegrationTests.Fixtures;
using Xunit;

namespace SOS.Observations.Api.IntegrationTests.IntegrationTests.ObservationsController.GeoGridAggregationEndpoint
{
    [Collection(Collections.ApiIntegrationTestsCollection)]
    public class MetricGridAggregationIntegrationTests
    {
        private readonly ApiIntegrationTestFixture _fixture;

        public MetricGridAggregationIntegrationTests(ApiIntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task MetricGeoGridAggregation_with_Mammalia_and_bbox_filter()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var searchFilter = new SearchFilterAggregationInternalDto
            {
                Taxon = new TaxonFilterDto { Ids = new List<int>() { 4000107 }, IncludeUnderlyingTaxa = true },
                Date = new DateFilterDto
                {
                    StartDate = new DateTime(1990, 1, 31, 07, 59, 46),
                    EndDate = new DateTime(2020, 1, 31, 07, 59, 46)
                },
                Geographics = new GeographicsFilterDto
                {
                    BoundingBox = new LatLonBoundingBoxDto
                    {
                        BottomRight = new LatLonCoordinateDto
                        {
                            Latitude = 59.17592824927137,
                            Longitude = 18.28125
                        },
                        TopLeft = new LatLonCoordinateDto
                        {
                            Latitude = 59.355596110016315,
                            Longitude = 17.9296875
                        }
                    }
                },
                VerificationStatus = SearchFilterBaseDto.StatusVerificationDto.BothVerifiedAndNotVerified,
                OccurrenceStatus = OccurrenceStatusFilterValuesDto.Present
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.MetricGridAggregationInternalAsync(
                null, 
                null, 
                searchFilter, 
                10000, 
                false, 
                false, 
                OutputFormatDto.GeoJson);
            var result = response.GetResult<FeatureCollection>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().NotBeNull();            
        }
    }
}