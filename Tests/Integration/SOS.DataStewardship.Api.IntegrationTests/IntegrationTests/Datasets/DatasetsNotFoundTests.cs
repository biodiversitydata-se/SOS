namespace SOS.DataStewardship.Api.IntegrationTests.IntegrationTests.Datasets;

[Collection(Constants.IntegrationTestsCollectionName)]
public class DatasetsNotFoundTests : TestBase
{
    public DatasetsNotFoundTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    [Fact]
    public async Task DatasetById_ReturnsHttp404NotFound_GivenDatasetIdThatDoesntExist()
    {
        // Arrange
        string datasetId = "NonExistingDatasetId";        
        var datasets = TestData.GetTestData().datasets;
        await ProcessFixture.AddDatasetsToElasticsearchAsync(datasets);

        // Act
        var result = await ApiClient.GetAsync($"datastewardship/datasets/{datasetId}");

        // Assert
        result.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DatasetsBySearch_ReturnsEmptyCollection_GivenSearchCriteraWithNoHits()
    {
        // Arrange
        var testData = TestData.GetTestData();
        await ProcessFixture.AddDataToElasticsearchAsync(testData);
        var searchFilter = new DatasetFilter
        {
            DatasetIds = new List<string> { "NonExistingDatasetId" }
        };

        // Act        
        var pageResult = await ApiClient.PostAndReturnAsJsonAsync<PagedResult<Dataset>, DatasetFilter>(
            $"datastewardship/datasets", searchFilter, jsonSerializerOptions);

        // Assert
        pageResult.TotalCount.Should().Be(0);
        pageResult.Records.Should().BeEmpty();
    }
}