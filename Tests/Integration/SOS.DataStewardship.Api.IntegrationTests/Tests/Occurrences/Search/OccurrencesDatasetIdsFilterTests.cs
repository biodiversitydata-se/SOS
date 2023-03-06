using SOS.DataStewardship.Api.Contracts.Models;

namespace SOS.DataStewardship.Api.IntegrationTests.Tests.Occurrences.Search;

[Collection(Constants.IntegrationTestsCollectionName)]
public class OccurrencesDatasetIdsFilterTests : TestBase
{
    public OccurrencesDatasetIdsFilterTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    [Fact]
    public async Task OccurrencesBySearch_ReturnsExpectedOccurrences_GivenExistingDatasetId()
    {
        // Arrange
        var testDataSet = TestData.Create(10);
        string datasetId 
            = testDataSet.Observations.First().DataStewardshipDatasetId 
            = Guid.NewGuid().ToString();
        await ProcessFixture.AddObservationsToElasticsearchAsync(testDataSet.Observations);
        var searchFilter = new OccurrenceFilter() {
            DatasetIds = new List<string>() { datasetId }
        };
        
        // Act
        var pageResult = await ApiClient.PostAndReturnAsJsonAsync<PagedResult<OccurrenceModel>, OccurrenceFilter>(
            $"datastewardship/occurrences?skip=0&take=0", searchFilter, jsonSerializerOptions);

        // Assert
        pageResult.TotalCount.Should().Be(1);
    }
}