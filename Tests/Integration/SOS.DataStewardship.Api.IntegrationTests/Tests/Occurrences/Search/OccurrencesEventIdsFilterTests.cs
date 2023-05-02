using SOS.DataStewardship.Api.Contracts.Models;

namespace SOS.DataStewardship.Api.IntegrationTests.Tests.Occurrences.Search;

[Collection(Constants.IntegrationTestsCollectionName)]
public class OccurrencesEventIdsFilterTests : TestBase
{
    public OccurrencesEventIdsFilterTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    [Fact]
    public async Task OccurrencesBySearch_ReturnsExpectedOccurrences_GivenExistingEventId()
    {
        // Arrange
        var testDataSet = TestData.Create(10);
        string eventId 
            = testDataSet.Observations.First().Event.EventId 
            = Guid.NewGuid().ToString();
        await ProcessFixture.AddObservationsToElasticsearchAsync(testDataSet.Observations);
        var searchFilter = new OccurrenceFilter() {
            EventIds = new List<string>() { eventId }            
        };
        
        // Act
        var pageResult = await ApiClient.PostAndReturnAsJsonAsync<PagedResult<Occurrence>, OccurrenceFilter>(
            $"datastewardship/occurrences?skip=0&take=0", searchFilter, jsonSerializerOptions);

        // Assert
        pageResult.TotalCount.Should().Be(1);
    }
}