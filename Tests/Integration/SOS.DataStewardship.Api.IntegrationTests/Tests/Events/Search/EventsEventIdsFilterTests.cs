using SOS.DataStewardship.Api.Contracts.Models;

namespace SOS.DataStewardship.Api.IntegrationTests.Tests.Events.Search;

[Collection(Constants.IntegrationTestsCollectionName)]
public class EventsEventIdsFilterTests : TestBase
{
    public EventsEventIdsFilterTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    [Fact]
    public async Task EventsBySearch_ReturnsExpectedEvents_GivenExistingEventId()
    {
        // Arrange
        var testDataSet = TestData.Create(10);
        string eventId
            = testDataSet.Observations.First().Event.EventId
            = testDataSet.Events.First().EventId 
            = Guid.NewGuid().ToString();
        await ProcessFixture.AddDataToElasticsearchAsync(testDataSet.Events, testDataSet.Observations);
        var searchFilter = new EventsFilter() {
            EventIds = new List<string>() { eventId }
        };

        // Act
        var pageResult = await ApiClient.PostAndReturnAsJsonAsync<PagedResult<Event>, EventsFilter>(
            $"datastewardship/events?skip=0&take=0", searchFilter, jsonSerializerOptions);

        // Assert
        pageResult.TotalCount.Should().Be(1);
    }
}