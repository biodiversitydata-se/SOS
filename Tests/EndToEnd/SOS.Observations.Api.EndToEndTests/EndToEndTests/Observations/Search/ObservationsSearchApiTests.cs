﻿using FluentAssertions;
using SOS.Observations.Api.EndToEndTests.Fixtures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SOS.Shared.Api.Dtos.Filter;
using Xunit;

namespace SOS.Observations.Api.EndToEndTests.EndToEndTests.Observations.Search
{
    public class ObservationsSearchApiTests : IClassFixture<ApiEndToEndTestFixture>
    {
        private readonly ApiEndToEndTestFixture _fixture;

        public ObservationsSearchApiTests(ApiEndToEndTestFixture apiTestFixture)
        {
            _fixture = apiTestFixture;
        }

        [Fact]
        [Trait("Category", "ApiEndToEndTest")]
        public async Task Search_for_Otter()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            SearchFilterDto searchFilter = new SearchFilterDto
            {
                Taxon = new TaxonFilterDto { Ids = new List<int> { TestData.TaxonIds.Otter }, IncludeUnderlyingTaxa = true },
                Date = new DateFilterDto
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
            var result = await _fixture.SosApiClient.SearchSos(searchFilter, 2, 0);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Records.Count().Should().Be(2, "because the take parameter is 2");
            result.TotalCount.Should().BeGreaterThan(3000, "because there should be more than 3000 observations of otter");
            result.Records.First().Taxon.Id.Should().Be(100077, "because otter has TaxonId=100077");
        }

        [Fact]
        [Trait("Category", "ApiEndToEndTest")]
        public async Task Search_for_Otter_At_Location()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            SearchFilterDto searchFilter = new SearchFilterDto
            {
                Taxon = new TaxonFilterDto { Ids = new List<int> { TestData.TaxonIds.Otter }, IncludeUnderlyingTaxa = true },
                Geographics = new GeographicsFilterDto
                {
                    Areas = new[]
                    {
                        TestData.Areas.TranasMunicipality, // Tranås Municipality
                        TestData.Areas.JonkopingCounty // Jönköping County
                    }
                },
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
            var result = await _fixture.SosApiClient.SearchSos(searchFilter, 2, 0);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Records.First().Taxon.VernacularName.Should().Be("utter", "because otter has the swedish vernacular name 'utter'");
            result.Records.First().Location.Municipality.Name.Should().Be("Tranås", "because the Area search is limited to Tranås municipality");
        }

        [Fact]
        [Trait("Category", "ApiEndToEndTest")]
        public async Task Search_for_Wolf_should_return_0_observations()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            SearchFilterDto searchFilter = new SearchFilterDto
            {
                Taxon = new TaxonFilterDto { Ids = new List<int> { TestData.TaxonIds.Wolf }, IncludeUnderlyingTaxa = true },
                VerificationStatus = Shared.Api.Dtos.Enum.StatusVerificationDto.BothVerifiedAndNotVerified,
                OccurrenceStatus = Shared.Api.Dtos.Enum.OccurrenceStatusFilterValuesDto.Present
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await _fixture.SosApiClient.SearchSos(searchFilter, 10, 0);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Records.Should().BeEmpty("because Wolf is protected");
        }
    }
}