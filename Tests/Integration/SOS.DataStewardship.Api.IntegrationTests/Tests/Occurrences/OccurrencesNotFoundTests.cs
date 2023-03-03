namespace SOS.DataStewardship.Api.IntegrationTests.Tests.Occurrences;

[Collection(Constants.IntegrationTestsCollectionName)]
public class OccurrencesNotFoundTests : TestBase
{
    public OccurrencesNotFoundTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    [Fact]
    public async Task OccurrenceById_ReturnsHttp404NotFound_GivenOccurrenceIdThatDoesntExist()
    {
        // Arrange        
        string occurrenceId = "NonExistingOccurrenceId";        
        var occurrences = TestData.Create(10).Observations;
        await ProcessFixture.AddObservationsToElasticsearchAsync(occurrences);

        // Act
        var result = await ApiClient.GetAsync($"datastewardship/occurrences/{occurrenceId}");

        // Assert
        result.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task OccurrencesBySearch_ReturnsEmptyCollection_GivenSearchCriteraWithNoHits()
    {
        // Arrange
        var occurrences = TestData.Create(10).Observations;
        await ProcessFixture.AddObservationsToElasticsearchAsync(occurrences);
        var searchFilter = new OccurrenceFilter {
            DatasetIds = new List<string> { "NonExistingDatasetId" }
        };

        // Act        
        var pageResult = await ApiClient.PostAndReturnAsJsonAsync<PagedResult<OccurrenceModel>, OccurrenceFilter>(
            $"datastewardship/occurrences", searchFilter, jsonSerializerOptions);

        // Assert
        pageResult.TotalCount.Should().Be(0);
        pageResult.Records.Should().BeEmpty();
    }
}