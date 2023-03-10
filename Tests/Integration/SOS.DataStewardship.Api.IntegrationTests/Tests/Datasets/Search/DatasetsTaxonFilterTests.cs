using SOS.DataStewardship.Api.Contracts.Models;
using Dataset = SOS.DataStewardship.Api.Contracts.Models.Dataset;

namespace SOS.DataStewardship.Api.IntegrationTests.Tests.Datasets.Search;

[Collection(Constants.IntegrationTestsCollectionName)]
public class DatasetsTaxonFilterTests : TestBase
{
    public DatasetsTaxonFilterTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    [Fact]
    public async Task DatasetsBySearch_ReturnsExpectedDatasets_GivenExistingTaxonId()
    {
        // Arrange
        var testDataSet = TestData.Create(10);
        int taxonId 
            = testDataSet.Observations.First().Taxon.Id 
            = -5000; // Unique TaxonId
        await ProcessFixture.AddDataToElasticsearchAsync(testDataSet);
        var searchFilter = new DatasetFilter {
            Taxon = new TaxonFilter {
                Ids = new List<int> { taxonId }
            }
        };

        // Act
        var pageResult = await ApiClient.PostAndReturnAsJsonAsync<PagedResult<Dataset>, DatasetFilter>(
            $"datastewardship/datasets?skip=0&take=1", searchFilter, jsonSerializerOptions);

        // Assert
        pageResult.TotalCount.Should().Be(1);
        pageResult.Records.First().Identifier.Should().Be(testDataSet.Observations.First().DataStewardshipDatasetId);
    }
}