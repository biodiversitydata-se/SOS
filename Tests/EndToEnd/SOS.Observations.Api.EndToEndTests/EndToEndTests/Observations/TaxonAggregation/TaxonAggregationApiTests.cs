﻿using FluentAssertions;
using SOS.Observations.Api.EndToEndTests.Fixtures;
using System;
using System.Linq;
using System.Threading.Tasks;
using SOS.Shared.Api.Dtos.Filter;
using Xunit;

namespace SOS.Observations.Api.EndToEndTests.EndToEndTests.Observations.TaxonAggregation
{
    public class TaxonAggregationApiTests : IClassFixture<ApiEndToEndTestFixture>
    {
        private readonly ApiEndToEndTestFixture _fixture;

        public TaxonAggregationApiTests(ApiEndToEndTestFixture apiTestFixture)
        {
            _fixture = apiTestFixture;
        }

        [Fact]
        [Trait("Category", "ApiEndToEndTest")]
        public async Task TaxonAggregation()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            SearchFilterDto searchFilter = new SearchFilterDto
            {
                Date = new DateFilterDto()
                {
                    StartDate = new DateTime(1990, 1, 31, 07, 59, 46),
                    EndDate = DateTime.Now
                },
                VerificationStatus = Shared.Api.Dtos.Enum.StatusVerificationDto.BothVerifiedAndNotVerified,
                OccurrenceStatus = Shared.Api.Dtos.Enum.OccurrenceStatusFilterValuesDto.Present
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await _fixture.SosApiClient.SearchSosTaxonAggregation(searchFilter, 100, 0);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.TotalCount.Should().BeGreaterThan(30000, "There are observations on more than 30 000 taxa");
            result.Records.First().ObservationCount.Should().BeGreaterThan(100000,
                "The taxon with most observations has more than 100 000 observations");
        }

        [Fact]
        [Trait("Category", "ApiEndToEndTest")]
        public async Task TaxonAggregation_with_boundingbox()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            SearchFilterDto searchFilter = new SearchFilterDto
            {
                Date = new DateFilterDto()
                {
                    StartDate = new DateTime(1990, 1, 31, 07, 59, 46),
                    EndDate = new DateTime(2020, 1, 31, 07, 59, 46)
                },
                VerificationStatus = Shared.Api.Dtos.Enum.StatusVerificationDto.BothVerifiedAndNotVerified,
                OccurrenceStatus = Shared.Api.Dtos.Enum.OccurrenceStatusFilterValuesDto.Present
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await _fixture.SosApiClient.SearchSosTaxonAggregation(
                searchFilter,
                500,
                0,
                17.9296875,
                59.355596110016315,
                18.28125,
                59.17592824927137);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.TotalCount.Should().BeGreaterThan(8000, "There are observations on more than 8 000 taxa inside the bounding box");
            result.Records.First().ObservationCount.Should().BeGreaterThan(2500,
                "The taxon with most observations inside the bounding box has more than 2 500 observations");
        }
    }
}