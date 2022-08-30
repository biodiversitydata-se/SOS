using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using SOS.Lib.Enums;
using SOS.Lib.Models.Statistics;
using SOS.Observations.Api.Dtos;
using SOS.Observations.Api.IntegrationTests.Extensions;
using SOS.Observations.Api.IntegrationTests.Fixtures;
using Xunit;

namespace SOS.Observations.Api.IntegrationTests.IntegrationTests.UserStatisticsController
{
    [Collection(Collections.ApiIntegrationTestsCollection)]
    public class PagedSpeciesCountAggregationIntegrationTests
    {
        private readonly ApiIntegrationTestFixture _fixture;

        public PagedSpeciesCountAggregationIntegrationTests(ApiIntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Test_PagedSpeciesCountAggregation_with_IncludeOtherAreasSpeciesCount()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var query = new SpeciesCountUserStatisticsQuery
            {
                AreaType = AreaType.Province,
                IncludeOtherAreasSpeciesCount = true
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.UserStatisticsController.PagedSpeciesCountAggregation(
                query,
                0,
                5);
            var result = response.GetResultObject<PagedResultDto<UserStatisticsItem>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Records.Count().Should().Be(5, "because the take parameter is 5");
            result.TotalCount.Should().BeGreaterThan(1000, "because there should be more than 1000 users with observations");
        }

        /// <remarks>
        /// In order to run this test properly, make sure to comment out the taxon filter in SearchExtensionsUserObservations.ToQuery().
        /// Take a memory snapshot before the loop and then after the loop is done and compare the results.
        /// </remarks>>
        [Fact(Skip = "Run on demand")]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Test_CacheSize_for_PagedSpeciesCountAggregation_with_IncludeOtherAreasSpeciesCount()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var query = new SpeciesCountUserStatisticsQuery
            {
                AreaType = AreaType.Province,
                IncludeOtherAreasSpeciesCount = true
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var initialResponse = await _fixture.UserStatisticsController.PagedSpeciesCountAggregation(
                query,
                0,
                5);

            int nrIterations = 100;
            for (int i = 0; i < nrIterations; i++)
            {
                query = query.Clone();
                query.TaxonId = i;
                var response = await _fixture.UserStatisticsController.PagedSpeciesCountAggregation(
                    query,
                    0,
                    30);
            }
            var result = initialResponse.GetResultObject<PagedResultDto<UserStatisticsItem>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Records.Count().Should().Be(30, "because the take parameter is 5");
        }
    }
}