namespace SOS.DataStewardship.Api.IntegrationTests.Tests.Occurrences.Search;

[Collection(Constants.IntegrationTestsCollectionName)]
public class OccurrencesTaxonFilterTests : TestBase
{
    public OccurrencesTaxonFilterTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    [Fact]
    public async Task OccurrencesBySearch_ReturnsExpectedOccurrences_GivenExistingDatasetId()
    {
        // Arrange
        var testDataSet = TestData.Create(10);
        int taxonId = testDataSet.Observations.First().Taxon.Id = -5000; // Unique TaxonId
        await ProcessFixture.AddObservationsToElasticsearchAsync(testDataSet.Observations);
        var searchFilter = new OccurrenceFilter() {
            Taxon = new TaxonFilter() {
                Ids = new List<int> { taxonId }
            }
        };
        
        // Act
        var pageResult = await ApiClient.PostAndReturnAsJsonAsync<PagedResult<OccurrenceModel>, OccurrenceFilter>(
            $"datastewardship/occurrences?skip=0&take=0", searchFilter, jsonSerializerOptions);

        // Assert
        pageResult.TotalCount.Should().Be(1);
    }
}