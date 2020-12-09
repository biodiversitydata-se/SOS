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
using JsonConvert = Newtonsoft.Json.JsonConvert;

namespace SOS.Observations.Api.IntegrationTests.IntegrationTests.Observations
{
    [Collection(Collections.ApiIntegrationTestsCollection)]
    public class ObservationsSearchApiIntegrationTests
    {
        private readonly ApiIntegrationTestFixture _fixture;

        public ObservationsSearchApiIntegrationTests(ApiIntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Search_for_Otter()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            SearchFilterDto searchFilter = new SearchFilterDto
            {
                Taxon = new TaxonFilterDto() { TaxonIds = new List<int>() { 100077 }, IncludeUnderlyingTaxa = true },
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
            var response = await _fixture.ObservationsController.SearchAsync(searchFilter, 0, 2);
            var result = response.GetResult<PagedResultDto<Observation>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Records.Count().Should().Be(2, "because the take parameter is 2");
            result.TotalCount.Should().BeGreaterThan(5000, "because there should be more than 5000 observations of otter");
            result.Records.First().Taxon.Id.Should().Be(100077, "because otter has TaxonId=100077");
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Search_for_Otter_At_Location()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            SearchFilterDto searchFilter = new SearchFilterDto
            {
                Taxon = new TaxonFilterDto() { TaxonIds = new List<int>() { 100077 }, IncludeUnderlyingTaxa = true },
                Areas = new[] { new AreaFilterDto { Type = AreaType.Municipality, FeatureId = "687" } },
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
            var response = await _fixture.ObservationsController.SearchAsync(searchFilter, 0, 2);
            var result = response.GetResult<PagedResultDto<Observation>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Records.First().Taxon.VernacularName.Should().Be("utter", "because otter has the swedish vernacular name 'utter'");
            result.Records.First().Location.Municipality.Name.Should().Be("Tranås", "because the Area search is limited to Tranås municipality");
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Search_for_Wolf_should_return_0_observations()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            SearchFilterDto searchFilter = new SearchFilterDto
            {
                Taxon = new TaxonFilterDto() { TaxonIds = new List<int>() { 100024 }, IncludeUnderlyingTaxa = true },
                OnlyValidated = false,
                OccurrenceStatus = OccurrenceStatusFilterValuesDto.Present
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.SearchAsync(searchFilter, 0, 10);
            var result = response.GetResult<PagedResultDto<Observation>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Records.Should().BeEmpty("because Wolf is protected");
        }
    }
}
