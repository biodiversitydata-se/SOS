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

namespace SOS.Observations.Api.IntegrationTests.IntegrationTests.ObservationsController.SearchEndpoint
{
    [Collection(Collections.ApiIntegrationTestsCollection)]
    public class TaxonFilterIntegrationTests
    {
        private readonly ApiIntegrationTestFixture _fixture;

        public TaxonFilterIntegrationTests(ApiIntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Search_for_mammalia_species_that_is_invasive()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            SearchFilterDto searchFilter = new SearchFilterDto
            {
                Taxon = new TaxonFilterDto
                {
                    Ids = new List<int> { TestData.TaxonIds.Mammalia }, 
                    IncludeUnderlyingTaxa = true,
                    TaxonListIds = new List<int> {(int)TaxonListId.InvasiveSpecies},
                    TaxonListOperator = TaxonListOperatorDto.Filter
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearch(searchFilter, 0, 2);
            var result = response.GetResult<PagedResultDto<Observation>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Records.Count().Should().Be(2, "because the take parameter is 2");
        }
    }
}
