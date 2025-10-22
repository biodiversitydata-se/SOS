using FluentAssertions;
using SOS.Lib.Models.Processed.Observation;
using SOS.Shared.Api.Dtos;
using SOS.Shared.Api.Dtos.Filter;
using SOS.Observations.Api.LiveIntegrationTests.Extensions;
using SOS.Observations.Api.LiveIntegrationTests.Fixtures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SOS.Observations.Api.LiveIntegrationTests.IntegrationTests.ObservationsController.SearchEndpoint
{
    [Collection(Fixtures.Collections.ApiIntegrationTestsCollection)]
    public class ProjectFilterIntegrationTests
    {
        private readonly ApiIntegrationTestFixture _fixture;

        public ProjectFilterIntegrationTests(ApiIntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Search_for_observations_in_a_specific_project()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            SearchFilterDto searchFilter = new SearchFilterDto
            {
                ProjectIds = new List<int> { 3503 },
                Date = new DateFilterDto
                {
                    StartDate = new DateTime(2020, 3, 29),
                    EndDate = new DateTime(2020, 3, 29)
                },
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearch(null, null, searchFilter, 0, 2);
            var result = response.GetResult<PagedResultDto<Observation>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Records.Count().Should().Be(2, "because the take parameter is 2");
            result.Records.First().Projects.First().Id.Should().Be(3503, "because the search filter speciefied ProjectIds=3503");

        }
    }
}
