using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using SOS.Lib.Models.Processed.Observation;
using SOS.Observations.Api.Dtos;
using SOS.Observations.Api.Dtos.Filter;
using SOS.Observations.Api.IntegrationTests.Extensions;
using SOS.Observations.Api.IntegrationTests.Fixtures;
using Xunit;

namespace SOS.Observations.Api.IntegrationTests.IntegrationTests.ObservationsController.SearchEndpoint
{
    [Collection(Collections.ApiIntegrationTestsCollection)]
    public class BirdNestActivityFilterIntegrationTests
    {
        private readonly ApiIntegrationTestFixture _fixture;

        public BirdNestActivityFilterIntegrationTests(ApiIntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Search_for_birdNestActivity_less_than_five()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            SearchFilterDto searchFilter = new SearchFilterDto
            {
                DataProvider = new DataProviderFilterDto { Ids = new List<int>{1}},
                Taxon = new TaxonFilterDto { Ids = new List<int> { TestData.TaxonIds.Aves }, IncludeUnderlyingTaxa = true },
                BirdNestActivityLimit = 5,
                Output = new OutputFilterDto
                {
                    Fields = new [] { "occurrence.birdNestActivityId" }
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearch(null, null, searchFilter, 0, 100);
            var result = response.GetResult<PagedResultDto<Observation>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            foreach (var record in result.Records)
            {
                record.Occurrence.BirdNestActivityId.Should().BeLessOrEqualTo(5);
            }
        }
    }
}
