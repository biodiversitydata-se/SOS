using Microsoft.AspNetCore.Mvc;
using SOS.DataStewardship.Api.Contracts.Models;
using Dataset = SOS.DataStewardship.Api.Contracts.Models.Dataset;

namespace SOS.DataStewardship.Api.IntegrationTests.Tests.Datasets;

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
        var datasets = TestData.Create(10).Datasets;
        await ProcessFixture.AddDatasetsToElasticsearchAsync(datasets);

        // Act
        var response = await ApiClient.GetAsync($"datasets/{datasetId}");
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>(jsonSerializerOptions);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        problemDetails.Status.Should().Be((int)System.Net.HttpStatusCode.NotFound);
        problemDetails.Detail.Should().Be($"Dataset with identifier={datasetId} was not found.");
    }

    [Fact]
    public async Task DatasetsBySearch_ReturnsEmptyCollection_GivenSearchCriteraWithNoHits()
    {
        // Arrange        
        var testDataSet = TestData.Create(10);
        await ProcessFixture.AddDataToElasticsearchAsync(testDataSet);
        var searchFilter = new DatasetFilter {
            DatasetIds = new List<string> { "NonExistingDatasetId" }
        };

        // Act        
        var pageResult = await ApiClient.PostAndReturnAsJsonAsync<PagedResult<Dataset>, DatasetFilter>(
            $"datasets", searchFilter, jsonSerializerOptions);

        // Assert
        pageResult.TotalCount.Should().Be(0);
        pageResult.Records.Should().BeEmpty();
    }
}