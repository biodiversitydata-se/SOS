using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using SOS.Observations.Api.Dtos;
using SOS.Observations.Api.Dtos.Filter;
using SOS.Observations.Api.IntegrationTests.Extensions;
using SOS.Observations.Api.IntegrationTests.Fixtures;
using Xunit;

namespace SOS.Observations.Api.IntegrationTests.IntegrationTests.Observations.TaxonAggregation
{
    [Collection(Collections.ApiIntegrationTestsCollection)]
    public class TaxonAggregationIntegrationTests
    {
        private readonly ApiIntegrationTestFixture _fixture;

        public TaxonAggregationIntegrationTests(ApiIntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task TaxonAggregation()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var searchFilter = new SearchFilterAggregationDto()
            {
                Date = new DateFilterDto()
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
            var response = await _fixture.ObservationsController.TaxonAggregationAsync(searchFilter, 0, 100);
            var result = response.GetResult<PagedResultDto<TaxonAggregationItemDto>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.TotalCount.Should().BeGreaterThan(30000, "There are observations on more than 30 000 taxa");
            result.Records.First().ObservationCount.Should().BeGreaterThan(100000,
                "The taxon with most observations has more than 100 000 observations");
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task TaxonAggregation_with_boundingbox()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var searchFilter = new SearchFilterAggregationDto()
            {
                Date = new DateFilterDto()
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
            var response = await _fixture.ObservationsController.TaxonAggregationAsync(
                searchFilter,
                0,
                500,
                17.9296875,
                59.355596110016315,
                18.28125,
                59.17592824927137);
            var result = response.GetResult<PagedResultDto<TaxonAggregationItemDto>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.TotalCount.Should().BeGreaterThan(8000, "There are observations on more than 8 000 taxa inside the bounding box");
            result.Records.First().ObservationCount.Should().BeGreaterThan(2500,
                "The taxon with most observations inside the bounding box has more than 2 500 observations");
        }
    }
}
