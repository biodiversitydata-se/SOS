using SOS.DataStewardship.Api.Contracts.Models;

namespace SOS.DataStewardship.Api.IntegrationTests.Tests.Events.Search;

[Collection(Constants.IntegrationTestsCollectionName)]
public class EventsTaxonFilterTests : TestBase
{
    public EventsTaxonFilterTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    [Fact]
    public async Task EventsBySearch_ReturnsExpectedEvents_GivenExistingTaxonId()
    {
        // Arrange
        var testDataSet = TestData.Create(10);
        int taxonId = testDataSet.Observations.First().Taxon.Id = -5000; // Unique TaxonId
        await ProcessFixture.AddDataToElasticsearchAsync(testDataSet.Events, testDataSet.Observations);        
        var searchFilter = new EventsFilter {
            Taxon = new TaxonFilter {
                Ids = new List<int> { taxonId }
            }
        };

        // Act
        var pageResult = await ApiClient.PostAndReturnAsJsonAsync<PagedResult<EventModel>, EventsFilter>(
            $"datastewardship/events?skip=0&take=1", searchFilter, jsonSerializerOptions);

        // Assert
        pageResult.TotalCount.Should().Be(1);
        pageResult.Records.First().EventID.Should().Be(testDataSet.Observations.First().Event.EventId);
    }
}