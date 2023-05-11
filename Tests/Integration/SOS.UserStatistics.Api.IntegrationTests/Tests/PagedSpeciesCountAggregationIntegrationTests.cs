namespace SOS.UserStatistics.Api.IntegrationTests.Tests;

[Collection(Collections.ApiIntegrationTestsCollection)]
public class PagedSpeciesCountAggregationIntegrationTests
{
    private readonly UserStatisticsIntegrationTestFixture _fixture;

    public PagedSpeciesCountAggregationIntegrationTests(UserStatisticsIntegrationTestFixture fixture)
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
            await _fixture.UserStatisticsManager.PagedSpeciesCountSearchAsync(query, 0, 5);
        }

        // Assert
        initialResult.Records.Count().Should().Be(5, "because the take parameter is 5");
    }

}
