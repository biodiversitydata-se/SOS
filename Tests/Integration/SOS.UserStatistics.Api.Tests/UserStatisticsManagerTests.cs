using SOS.Observations.Api.IntegrationTests.Fixtures;
using SOS.UserStatistics.Api.Tests.Fixtures;

namespace SOS.UserStatistics.Api.Tests;

[Collection(Collections.ApiTestsCollection)]
public class UserStatisticsManagerTests
{
    private readonly UserStatisticsTestFixture _fixture;

    public UserStatisticsManagerTests(UserStatisticsTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Test_PagedSpeciesCountSearchAsync_with_IncludeOtherAreasSpeciesCount()
    {
        // Arrange

        var query = new SpeciesCountUserStatisticsQuery
        {
            AreaType = AreaType.Province,
            IncludeOtherAreasSpeciesCount = true
        };

        // Act
        var res = await _fixture.UserStatisticsManager.PagedSpeciesCountSearchAsync(query, 0, 5);

        // Assert
        res.Records.Count().Should().Be(5, "because the take paramter is 5");
        res.TotalCount.Should().BeGreaterThan(1000, "because there should be more than 1000 users with observations");
    }

    [Fact]
    public async Task Test_CacheSize_for_PagedSpeciesCountAggregation_with_IncludeOtherAreasSpeciesCount()
    {
        // Arrange
        var query = new SpeciesCountUserStatisticsQuery
        {
            AreaType = AreaType.Province,
            IncludeOtherAreasSpeciesCount = true
        };

        // Act
        var initialResult = await _fixture.UserStatisticsManager.PagedSpeciesCountSearchAsync(query, 0, 5);

        int nrIterations = 100;
        for (int i = 0; i < nrIterations; i++)
        {
            query = query.Clone();
            query.TaxonId = i;
            await _fixture.UserStatisticsManager.PagedSpeciesCountSearchAsync(query, 0, 30);
        }
       
        // Assert
        initialResult.Records.Count().Should().Be(30, "because the take parameter is 5");
    }

}
