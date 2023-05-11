using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using SOS.Observations.Api.Dtos.Filter;
using SOS.Observations.Api.EndToEndTests.Fixtures;
using Xunit;

namespace SOS.Observations.Api.EndToEndTests.EndToEndTests.Observations.GeoGridAggregation
{
    public class GeoGridAggregationApiTests : IClassFixture<ApiEndToEndTestFixture>
    {
        private readonly ApiEndToEndTestFixture _fixture;

        public GeoGridAggregationApiTests(ApiEndToEndTestFixture apiTestFixture)
        {
            _fixture = apiTestFixture;
        }

        [Fact]
        [Trait("Category", "ApiEndToEndTest")]
        public async Task GeoGridAggregation_Mammalia()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            SearchFilterDto searchFilter = new SearchFilterDto
            {
                Taxon = new TaxonFilterDto { Ids = new List<int>() { 4000107 }, IncludeUnderlyingTaxa = true},
                Date = new DateFilterDto
                {
                    StartDate = new DateTime(1990, 1, 31, 07, 59, 46),
                    EndDate = new DateTime(2020, 1, 31, 07, 59, 46)
                },
                VerificationStatus = SearchFilterBaseDto.StatusVerificationDto.BothVerifiedAndNotVerified,
                OccurrenceStatus = OccurrenceStatusFilterValuesDto.Present
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await _fixture.SosApiClient.SearchSosGeoAggregation(searchFilter, 10);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.GridCellCount.Should().BeGreaterThan(1000);
            result.GridCells.First().ObservationsCount.Should().BeGreaterThan(1000);
        }
    }
}