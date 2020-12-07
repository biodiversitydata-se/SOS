using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using SOS.Observations.Api.Dtos.Filter;
using SOS.Observations.Api.IntegrationTests.Fixtures;
using SOS.TestHelpers;
using Xunit;

namespace SOS.Observations.Api.IntegrationTests.EndToEndTests.Observations.GeoGridAggregation
{
    public class GeoGridAggregationApiTests : IClassFixture<ObservationApiEndToEndTestFixture>
    {
        private readonly ObservationApiEndToEndTestFixture _fixture;

        public GeoGridAggregationApiTests(ObservationApiEndToEndTestFixture apiTestFixture)
        {
            _fixture = apiTestFixture;
        }

        [Fact]
        [Trait("Catgory", "ApiEndToEndTest")]
        public async Task GeoGridAggregation_Mammalia()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            SearchFilterDto searchFilter = new SearchFilterDto
            {
                Taxon = new TaxonFilterDto {TaxonIds = new List<int>() { 4000107 }, IncludeUnderlyingTaxa = true},
                Date = new DateFilterDto
                {
                    StartDate = new DateTime(1990, 1, 31, 07, 59, 46),
                    EndDate = new DateTime(2020, 1, 31, 07, 59, 46)
                },
                OnlyValidated = false,
                OccurrenceStatus = OccurrenceStatusFilterValuesDto.Present
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await _fixture.SosApiClient.SearchSosGeoAggregation(searchFilter);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.GridCellCount.Should().BeGreaterThan(1000);
            result.GridCells.First().ObservationsCount.Should().BeGreaterThan(1000);
        }
    }
}