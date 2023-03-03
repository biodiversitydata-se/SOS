namespace SOS.DataStewardship.Api.IntegrationTests.Tests.Occurrences;

[Collection(Constants.IntegrationTestsCollectionName)]
public class OccurrencesPaginationTests : TestBase
{
    public OccurrencesPaginationTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    [Fact]
    public async Task OccurrencesBySearch_ReturnsAllOccurrences_WhenPaginatingAllRecords()
    {
        // Arrange
        var observations = TestData.Create(10).Observations;
        await ProcessFixture.AddObservationsToElasticsearchAsync(observations);
        var searchFilter = new OccurrenceFilter();
        int take = 2;
        var occurrenceModels = new List<OccurrenceModel>();

        // Act
        for (int skip = 0; skip < observations.Count(); skip += take)
        {
            var pageResult = await ApiClient.PostAndReturnAsJsonAsync<PagedResult<OccurrenceModel>, OccurrenceFilter>(
                $"datastewardship/occurrences?skip={skip}&take={take}", searchFilter, jsonSerializerOptions);
            occurrenceModels.AddRange(pageResult.Records);
        }

        // Assert
        var occurrenceIds = observations.Select(m => m.Occurrence.OccurrenceId);
        var uniqueOccurrenceIds = occurrenceModels.Select(m => m.OccurrenceID).Distinct();
        uniqueOccurrenceIds.Should().BeEquivalentTo(occurrenceIds);
    }


    [Fact]
    public async Task OccurrencesBySearch_ReturnsCorrectPagingMetadata_GivenValidInput()
    {
        // Arrange        
        var observations = TestData.Create(10).Observations;
        await ProcessFixture.AddObservationsToElasticsearchAsync(observations);
        var searchFilter = new OccurrenceFilter();
        int skip = 5;
        int take = 2;

        // Act
        var pageResult = await ApiClient.PostAndReturnAsJsonAsync<PagedResult<OccurrenceModel>, OccurrenceFilter>(
            $"datastewardship/occurrences?skip={skip}&take={take}", searchFilter, jsonSerializerOptions);

        // Assert
        pageResult.TotalCount.Should().Be(observations.Count());
        pageResult.Take.Should().Be(take);
        pageResult.Count.Should().Be(take);
        pageResult.Skip.Should().Be(skip);
    }


    [Fact]
    public async Task OccurrencesBySearch_ReturnsNoRecords_GivenOutOfRangeSkipParameter()
    {
        // Arrange
        var observations = TestData.Create(10).Observations;
        await ProcessFixture.AddObservationsToElasticsearchAsync(observations);
        var searchFilter = new OccurrenceFilter();
        int skip = observations.Count();
        int take = 2;

        // Act
        var pageResult = await ApiClient.PostAndReturnAsJsonAsync<PagedResult<EventModel>, OccurrenceFilter>(
            $"datastewardship/occurrences?skip={skip}&take={take}", searchFilter, jsonSerializerOptions);

        // Assert
        pageResult.TotalCount.Should().Be(observations.Count());
        pageResult.Take.Should().Be(take);
        pageResult.Count.Should().Be(0);
        pageResult.Skip.Should().Be(skip);
    }


    [Fact]
    public async Task OccurrencesBySearch_ReturnsBadRequest_GivenInvalidSkipAndTake()
    {
        // Arrange
        var observations = TestData.Create(10).Observations;
        await ProcessFixture.AddObservationsToElasticsearchAsync(observations);

        var searchFilter = new OccurrenceFilter();
        int skipNegative = -1;
        int skipTooLarge = 1000000;
        int skip = 2;
        int take = 2;
        int takeNegative = -1;
        int takeTooLarge = 1000000;

        // Act
        var responseSkipNegative = await ApiClient.PostAsJsonAsync(
            $"datastewardship/occurrences?skip={skipNegative}&take={take}", searchFilter, jsonSerializerOptions);

        var responseSkipTooLarge = await ApiClient.PostAsJsonAsync(
            $"datastewardship/occurrences?skip={skipTooLarge}&take={take}", searchFilter, jsonSerializerOptions);

        var responseTakeNegative = await ApiClient.PostAsJsonAsync(
            $"datastewardship/occurrences?skip={skip}&take={takeNegative}", searchFilter, jsonSerializerOptions);

        var responseTakeTooLarge = await ApiClient.PostAsJsonAsync(
            $"datastewardship/occurrences?skip={skip}&take={takeTooLarge}", searchFilter, jsonSerializerOptions);

        // Assert
        responseSkipNegative.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        responseSkipTooLarge.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        responseTakeNegative.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        responseTakeTooLarge.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }
}