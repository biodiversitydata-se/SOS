namespace SOS.DataStewardship.Api.IntegrationTests.Tests.Datasets;

[Collection(Constants.IntegrationTestsCollectionName)]
public class DatasetsPaginationTests : TestBase
{
    public DatasetsPaginationTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    [Fact]
    public async Task DatasetsBySearch_ReturnsAllDatasets_WhenPaginatingAllRecords()
    {
        // Arrange
        var testDataSet = TestData.Create(10);
        await ProcessFixture.AddDataToElasticsearchAsync(testDataSet);
        var searchFilter = new DatasetFilter();
        var datasetModels = new List<Dataset>();
        int take = 2;

        // Act
        for (int skip = 0; skip < testDataSet.Datasets.Count(); skip += take)
        {
            var pageResult = await ApiClient.PostAndReturnAsJsonAsync<PagedResult<Dataset>, DatasetFilter>(
                $"datastewardship/datasets?skip={skip}&take={take}", searchFilter, jsonSerializerOptions);
            datasetModels.AddRange(pageResult.Records);
        }

        // Assert
        var datasetIds = testDataSet.Datasets.Select(m => m.Identifier).ToList();
        var uniqueDatasetIds = datasetModels.Select(m => m.Identifier).Distinct();
        uniqueDatasetIds.Should().BeEquivalentTo(datasetIds);
    }

    [Fact]
    public async Task DatasetsBySearch_ReturnsCorrectPagingMetadata_GivenValidInput()
    {
        // Arrange                                
        var testDataSet = TestData.Create(10);
        await ProcessFixture.AddDataToElasticsearchAsync(testDataSet);
        var searchFilter = new DatasetFilter();
        int skip = 5;
        int take = 2;

        // Act        
        var pageResult = await ApiClient.PostAndReturnAsJsonAsync<PagedResult<Dataset>, DatasetFilter>(
            $"datastewardship/datasets?skip={skip}&take={take}", searchFilter, jsonSerializerOptions);

        // Assert
        pageResult.TotalCount.Should().Be(testDataSet.Datasets.Count());
        pageResult.Take.Should().Be(take);
        pageResult.Count.Should().Be(take);
        pageResult.Skip.Should().Be(skip);
    }


    [Fact]
    public async Task DatasetsBySearch_ReturnsNoRecords_GivenOutOfRangeSkipParameter()
    {
        // Arrange
        var testDataSet = TestData.Create(10);
        await ProcessFixture.AddDataToElasticsearchAsync(testDataSet);
        var searchFilter = new DatasetFilter();
        int skip = testDataSet.Datasets.Count();
        int take = 2;

        // Act
        var pageResult = await ApiClient.PostAndReturnAsJsonAsync<PagedResult<Dataset>, DatasetFilter>(
            $"datastewardship/datasets?skip={skip}&take={take}", searchFilter, jsonSerializerOptions);

        // Assert
        pageResult.TotalCount.Should().Be(testDataSet.Datasets.Count());
        pageResult.Take.Should().Be(take);
        pageResult.Count.Should().Be(0);
        pageResult.Skip.Should().Be(skip);
    }

    [Fact]
    public async Task DatasetsBySearch_ReturnsBadRequest_GivenInvalidSkipAndTake()
    {
        // Arrange        
        var testDataSet = TestData.Create(10);
        await ProcessFixture.AddDataToElasticsearchAsync(testDataSet);

        var searchFilter = new DatasetFilter();
        int skipNegative = -1;
        int skipTooLarge = 1000000;
        int skip = 2;
        int take = 2;
        int takeNegative = -1;
        int takeTooLarge = 1000000;

        // Act
        var responseSkipNegative = await ApiClient.PostAsJsonAsync(
            $"datastewardship/datasets?skip={skipNegative}&take={take}", searchFilter, jsonSerializerOptions);

        var responseSkipTooLarge = await ApiClient.PostAsJsonAsync(
            $"datastewardship/datasets?skip={skipTooLarge}&take={take}", searchFilter, jsonSerializerOptions);

        var responseTakeNegative = await ApiClient.PostAsJsonAsync(
            $"datastewardship/datasets?skip={skip}&take={takeNegative}", searchFilter, jsonSerializerOptions);

        var responseTakeTooLarge = await ApiClient.PostAsJsonAsync(
            $"datastewardship/datasets?skip={skip}&take={takeTooLarge}", searchFilter, jsonSerializerOptions);

        // Assert
        responseSkipNegative.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        responseSkipTooLarge.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        responseTakeNegative.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        responseTakeTooLarge.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }
}