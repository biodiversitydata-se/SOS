using SOS.DataStewardship.Api.IntegrationTests.Extensions;
using SOS.DataStewardship.Api.IntegrationTests.Helpers;
using SOS.DataStewardship.Api.IntegrationTests.TestData;
using SOS.Lib.Models.Processed.DataStewardship.Event;
using SOS.Lib.Models.Processed.Observation;
using Xunit.Abstractions;

namespace SOS.DataStewardship.Api.IntegrationTests.IntegrationTests.Occurrences;

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
        var occurrences = ObservationsTestData.GetObservationTestData();
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
        await ProcessFixture.AddObservationsToElasticsearchAsync(ObservationsTestData.GetObservationTestData());
        var searchFilter = new OccurrenceFilter
        {
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