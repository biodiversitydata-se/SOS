namespace SOS.DataStewardship.Api.IntegrationTests.IntegrationTests.Events;

[Collection(Constants.IntegrationTestsCollectionName)]
public class EventsNotFoundTests : TestBase
{
    public EventsNotFoundTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }
    
    [Fact]
    public async Task EventById_ReturnsHttp404NotFound_GivenEventIdThatDoesntExist()
    {
        // Arrange
        string eventId = "NonExistingEventId";
        var events = TestData.GetTestData().events;
        await ProcessFixture.AddEventsToElasticsearchAsync(events);

        // Act
        var result = await ApiClient.GetAsync($"datastewardship/events/{eventId}");      

        // Assert
        result.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);        
    }

    [Fact]
    public async Task EventsBySearch_ReturnsEmptyCollection_GivenSearchCriteraWithNoHits()
    {
        // Arrange
        var testData = TestData.GetTestData();        
        await ProcessFixture.AddDataToElasticsearchAsync((testData.events, testData.observations));
        var searchFilter = new EventsFilter
        {
            DatasetIds = new List<string> { "NonExistingDatasetId" }
        };

        // Act        
        var pageResult = await ApiClient.PostAndReturnAsJsonAsync<PagedResult<EventModel>, EventsFilter>(
            $"datastewardship/events", searchFilter, jsonSerializerOptions);

        // Assert
        pageResult.TotalCount.Should().Be(0);
        pageResult.Records.Should().BeEmpty();
    }
}