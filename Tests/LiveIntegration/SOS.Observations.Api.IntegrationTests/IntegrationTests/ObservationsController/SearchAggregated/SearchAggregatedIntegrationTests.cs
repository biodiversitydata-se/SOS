using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.AggregatedResult;
using SOS.Observations.Api.Dtos;
using SOS.Observations.Api.Dtos.Filter;
using SOS.Observations.Api.IntegrationTests.Extensions;
using SOS.Observations.Api.IntegrationTests.Fixtures;
using Xunit;

namespace SOS.Observations.Api.IntegrationTests.IntegrationTests.ObservationsController.SearchAggregated
{
    [Collection(Collections.ApiIntegrationTestsCollection)]
    public class SearchAggregatedIntegrationTests
    {
        private readonly ApiIntegrationTestFixture _fixture;

        public SearchAggregatedIntegrationTests(ApiIntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Searchaggregated_test()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var searchFilter = new SearchFilterAggregationInternalDto
            {
                Taxon = new TaxonFilterDto
                {
                    Ids = new List<int> { TestData.TaxonIds.Aves },
                    IncludeUnderlyingTaxa = true,
                    TaxonListIds = new List<int> { (int)TaxonListId.RedlistedSpecies },
                    TaxonListOperator = TaxonListOperatorDto.Filter
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.SearchAggregatedInternal(null, null, searchFilter, AggregationType.SpeciesSightingsList, 0, 2);
            var result = response.GetResult<PagedResultDto<AggregatedSpecies>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Records.Count().Should().Be(2, "because the take parameter is 2");
            result.Records.First().RedlistCategory.Should().NotBeNullOrEmpty("because we are searching only redlisted species.");
        }
    }
}
