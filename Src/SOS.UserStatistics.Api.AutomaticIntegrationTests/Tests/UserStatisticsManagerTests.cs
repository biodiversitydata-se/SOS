namespace SOS.UserStatistics.Api.AutomaticIntegrationTests.Tests;

[Collection(Collections.ApiAutomaticIntegrationTestsCollection)]
public class UserStatisticsManagerTests
{
    private readonly UserStatisticsAutomaticIntegrationTestFixture _fixture;

    public UserStatisticsManagerTests(UserStatisticsAutomaticIntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }
}
