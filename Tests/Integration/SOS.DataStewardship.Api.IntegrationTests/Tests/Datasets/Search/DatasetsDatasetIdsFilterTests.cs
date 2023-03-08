namespace SOS.DataStewardship.Api.IntegrationTests.Tests.Datasets.Search;

[Collection(Constants.IntegrationTestsCollectionName)]
public class DatasetsDatasetIdsFilterTests : TestBase
{
    public DatasetsDatasetIdsFilterTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    [Fact]
    public async Task DatasetsBySearch_ReturnsExpectedDatasets_GivenExistingDatasetId()
    {
        // Arrange
        var testDataSet = TestData.Create(10);
        string datasetId 
            = testDataSet.Observations.First().DataStewardshipDatasetId 
            = testDataSet.Events.First().Dataset.Identifier 
            = testDataSet.Datasets.First().Identifier
            = Guid.NewGuid().ToString();
        await ProcessFixture.AddDataToElasticsearchAsync(testDataSet);
        
        var searchFilter = new DatasetFilter() {
            DatasetIds = new List<string>() { datasetId }
        };
        
        // Act
        var pageResult = await ApiClient.PostAndReturnAsJsonAsync<PagedResult<Dataset>, DatasetFilter>(
            $"datastewardship/datasets?skip=0&take=1", searchFilter, jsonSerializerOptions);

        // Assert
        pageResult.TotalCount.Should().Be(1);
        pageResult.Records.First().Identifier.Should().Be(testDataSet.Datasets.First().Identifier);
    }
}