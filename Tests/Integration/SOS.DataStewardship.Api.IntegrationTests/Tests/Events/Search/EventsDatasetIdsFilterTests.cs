using SOS.DataStewardship.Api.Contracts.Models;

namespace SOS.DataStewardship.Api.IntegrationTests.Tests.Events.Search;

[Collection(Constants.IntegrationTestsCollectionName)]
public class EventsDatasetIdsFilterTests : TestBase
{
    public EventsDatasetIdsFilterTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    [Fact]
    public async Task EventsBySearch_ReturnsExpectedEvents_GivenExistingDatasetId()
    {
        // Arrange
        var testDataSet = TestData.Create(10);
        string datasetId 
            = testDataSet.Observations.First().DataStewardship.DatasetIdentifier 
            = testDataSet.Events.First().DataStewardship.DatasetIdentifier 
            = Guid.NewGuid().ToString();
        await ProcessFixture.AddDataToElasticsearchAsync(testDataSet.Events, testDataSet.Observations);
        
        var searchFilter = new EventsFilter() {
            DatasetIds = new List<string>() { datasetId }
        };
        
        // Act
        var pageResult = await ApiClient.PostAndReturnAsJsonAsync<PagedResult<Event>, EventsFilter>(
            $"events?skip=0&take=0", searchFilter, jsonSerializerOptions);

        // Assert
        pageResult.TotalCount.Should().Be(1);
    }
}