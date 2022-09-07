using SOS.Lib.Enums;
using SOS.UserStatistics.Api.Dtos;

namespace SOS.UserStatistics.Api.Tests;

public class UserStatisticsModuleTests
{
    private readonly UserStatisticsTestFixture _fixture;

    public UserStatisticsModuleTests(UserStatisticsTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Test_PagedSpeciesCountAggregation_with_IncludeOtherAreasSpeciesCount()
    {
        // Arrange
        var query = new SpeciesCountUserStatisticsQuery
        {
            AreaType = AreaType.Province,
            IncludeOtherAreasSpeciesCount = true
        };

        // Act
        //var response = await _fixture.UserStatisticsModule.PagedSpeciesCountAggregation(query, 0, 5);
        //var result = response.GetResultObject<PagedResultDto<UserStatisticsItem>>();

        // Assert

    }

}
