using FizzWare.NBuilder;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Observations.Api.IntegrationTests.Setup;
using SOS.Observations.Api.IntegrationTests.TestData.TestDataBuilder;
using SOS.Shared.Api.Dtos;
using SOS.Shared.Api.Dtos.Enum;
using SOS.Shared.Api.Dtos.Filter;

namespace SOS.Observations.Api.IntegrationTests.Tests.ApiEndpoints.ObservationsEndpoints.ObservationsByCursorEndpoint;

[Collection(TestCollection.Name)]
public class ObservationsByCursorTests : TestBase
{
    public ObservationsByCursorTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    [Fact]
    public async Task SearchByCursor_PaginatesThroughAllResults_WhenUsingCursor()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
            .All().HaveValuesFromPredefinedObservations()
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto { OccurrenceStatus = OccurrenceStatusFilterValuesDto.Present };
        var allOccurrenceIds = new HashSet<string>();

        // Act - First page
        var response1 = await apiClient.PostAsync(
            "/observations/searchbycursor?take=30",
            JsonContent.Create(searchFilter));
        var result1 = await response1.Content.ReadFromJsonAsync<SearchByCursorResultDto<Observation>>();

        // Act - Second page
        var response2 = await apiClient.PostAsync(
            $"/observations/searchbycursor?take=30&cursor={result1!.NextCursor}",
            JsonContent.Create(searchFilter));
        var result2 = await response2.Content.ReadFromJsonAsync<SearchByCursorResultDto<Observation>>();

        // Act - Third page
        var response3 = await apiClient.PostAsync(
            $"/observations/searchbycursor?take=30&cursor={result2!.NextCursor}",
            JsonContent.Create(searchFilter));
        var result3 = await response3.Content.ReadFromJsonAsync<SearchByCursorResultDto<Observation>>();

        // Act - Fourth page (last 10)
        var response4 = await apiClient.PostAsync(
            $"/observations/searchbycursor?take=30&cursor={result3!.NextCursor}",
            JsonContent.Create(searchFilter));
        var result4 = await response4.Content.ReadFromJsonAsync<SearchByCursorResultDto<Observation>>();

        // Act - Fifth page (no records)
        var response5 = await apiClient.PostAsync(
           $"/observations/searchbycursor?take=30&cursor={result4!.NextCursor}",
           JsonContent.Create(searchFilter));
        var result5 = await response5.Content.ReadFromJsonAsync<SearchByCursorResultDto<Observation>>();


        // Collect all occurrence IDs
        foreach (var obs in result1.Records!) allOccurrenceIds.Add(obs.Occurrence!.OccurrenceId!);
        foreach (var obs in result2.Records!) allOccurrenceIds.Add(obs.Occurrence!.OccurrenceId!);
        foreach (var obs in result3.Records!) allOccurrenceIds.Add(obs.Occurrence!.OccurrenceId!);
        foreach (var obs in result4!.Records!) allOccurrenceIds.Add(obs.Occurrence!.OccurrenceId!);

        // Assert
        response1.StatusCode.Should().Be(HttpStatusCode.OK);
        response2.StatusCode.Should().Be(HttpStatusCode.OK);
        response3.StatusCode.Should().Be(HttpStatusCode.OK);
        response4.StatusCode.Should().Be(HttpStatusCode.OK);

        result1.Records.Should().HaveCount(30);
        result2.Records.Should().HaveCount(30);
        result3.Records.Should().HaveCount(30);
        result4.Records.Should().HaveCount(10);
        result5.Records.Should().HaveCount(0);
        result5.NextCursor.Should().BeNullOrEmpty(because: "this is the last page");
        allOccurrenceIds.Should().HaveCount(100, because: "all observations should be unique across pages");
    }

    [Fact]
    public async Task SearchByCursor_ReturnsNullCursor_WhenNoMoreResultsExist()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(30)
            .All().HaveValuesFromPredefinedObservations()
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto { OccurrenceStatus = OccurrenceStatusFilterValuesDto.Present };

        // Act
        var response1 = await apiClient.PostAsync(
            "/observations/searchbycursor?take=50",
            JsonContent.Create(searchFilter));
        var result1 = await response1.Content.ReadFromJsonAsync<SearchByCursorResultDto<Observation>>();

        var response2 = await apiClient.PostAsync(
            $"/observations/searchbycursor?take=50&cursor={result1!.NextCursor}",
            JsonContent.Create(searchFilter));
        var result2 = await response2.Content.ReadFromJsonAsync<SearchByCursorResultDto<Observation>>();


        // Assert
        response1.StatusCode.Should().Be(HttpStatusCode.OK);
        result1.Should().NotBeNull();
        result1!.TotalCount.Should().Be(30);
        result1.Records.Should().HaveCount(30);

        result2.Records.Should().HaveCount(0);
        result2.NextCursor.Should().BeNullOrEmpty(because: "all results have been fetched");
    }

    [Fact]
    public async Task SearchByCursor_TotalCountChanges_WhenObservationsAddedBetweenRequests()
    {
        // Arrange - Create initial observations
        var initialObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(50)
            .All().HaveValuesFromPredefinedObservations()
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(initialObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto { OccurrenceStatus = OccurrenceStatusFilterValuesDto.Present };

        // Act - First page request
        var response1 = await apiClient.PostAsync(
            "/observations/searchbycursor?take=30",
            JsonContent.Create(searchFilter));
        var result1 = await response1.Content.ReadFromJsonAsync<SearchByCursorResultDto<Observation>>();

        // Add more observations between requests (simulating "live" data changes)
        var additionalObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(25)
            .All().HaveValuesFromPredefinedObservations()
            .Build();
        var processedAdditionalObservations = ProcessFixture.ProcessObservations(additionalObservations);
        await ProcessFixture.AddObservationsToElasticsearchAsync(processedAdditionalObservations, clearExistingObservations: false);

        // Act - Second page request (using cursor from first request)
        var response2 = await apiClient.PostAsync(
            $"/observations/searchbycursor?take=30&cursor={result1!.NextCursor}",
            JsonContent.Create(searchFilter));
        var result2 = await response2.Content.ReadFromJsonAsync<SearchByCursorResultDto<Observation>>();

        // Assert
        response1.StatusCode.Should().Be(HttpStatusCode.OK);
        response2.StatusCode.Should().Be(HttpStatusCode.OK);

        result1.TotalCount.Should().Be(50, because: "initial request should see 50 observations");
        result2!.TotalCount.Should().Be(75, because: "second request should see 75 observations after adding 25 more");

        // Note: This demonstrates the "live view" nature of search_after pagination
        // as documented in the endpoint remarks - TotalCount can change between requests
    }

    [Fact]
    public async Task SearchByCursor_ReturnsObservationsInCorrectOrder_WhenSortingAscending()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(50)
            .All().HaveValuesFromPredefinedObservations()
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto { OccurrenceStatus = OccurrenceStatusFilterValuesDto.Present };

        // Act
        var response = await apiClient.PostAsync(
            "/observations/searchbycursor?take=50&sortBy=modified&sortOrder=asc",
            JsonContent.Create(searchFilter));
        var result = await response.Content.ReadFromJsonAsync<SearchByCursorResultDto<Observation>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Records.Should().HaveCount(50);
        result.Records!.Select(o => o.Modified).Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task SearchByCursor_ReturnsObservationsInCorrectOrder_WhenSortingDescending()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(50)
            .All().HaveValuesFromPredefinedObservations()
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto { OccurrenceStatus = OccurrenceStatusFilterValuesDto.Present };

        // Act
        var response = await apiClient.PostAsync(
            "/observations/searchbycursor?take=50&sortBy=modified&sortOrder=desc",
            JsonContent.Create(searchFilter));
        var result = await response.Content.ReadFromJsonAsync<SearchByCursorResultDto<Observation>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Records.Should().HaveCount(50);
        result.Records!.Select(o => o.Modified).Should().BeInDescendingOrder();
    }

    [Fact]
    public async Task SearchByCursor_MaintainsSortOrderAcrossPages_WhenPaginating()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(60)
            .All().HaveValuesFromPredefinedObservations()
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto { OccurrenceStatus = OccurrenceStatusFilterValuesDto.Present };
        var allModifiedDates = new List<DateTime?>();

        // Act - First page
        var response1 = await apiClient.PostAsync(
            "/observations/searchbycursor?take=30&sortBy=modified&sortOrder=asc",
            JsonContent.Create(searchFilter));
        var result1 = await response1.Content.ReadFromJsonAsync<SearchByCursorResultDto<Observation>>();

        // Act - Second page
        var response2 = await apiClient.PostAsync(
            $"/observations/searchbycursor?take=30&sortBy=modified&sortOrder=asc&cursor={result1!.NextCursor}",
            JsonContent.Create(searchFilter));
        var result2 = await response2.Content.ReadFromJsonAsync<SearchByCursorResultDto<Observation>>();

        // Collect all modified dates in order
        allModifiedDates.AddRange(result1.Records!.Select(o => o.Modified));
        allModifiedDates.AddRange(result2!.Records!.Select(o => o.Modified));

        // Assert
        response1.StatusCode.Should().Be(HttpStatusCode.OK);
        response2.StatusCode.Should().Be(HttpStatusCode.OK);
        allModifiedDates.Should().BeInAscendingOrder(because: "sort order should be maintained across pages");
    }

    [Fact]
    public async Task SearchByCursor_ReturnsCorrectTotalCount_AcrossAllPages()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(75)
            .All().HaveValuesFromPredefinedObservations()
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto { OccurrenceStatus = OccurrenceStatusFilterValuesDto.Present };

        // Act - First page
        var response1 = await apiClient.PostAsync(
            "/observations/searchbycursor?take=25",
            JsonContent.Create(searchFilter));
        var result1 = await response1.Content.ReadFromJsonAsync<SearchByCursorResultDto<Observation>>();

        // Act - Second page
        var response2 = await apiClient.PostAsync(
            $"/observations/searchbycursor?take=25&cursor={result1!.NextCursor}",
            JsonContent.Create(searchFilter));
        var result2 = await response2.Content.ReadFromJsonAsync<SearchByCursorResultDto<Observation>>();

        // Assert
        result1.TotalCount.Should().Be(75);
        result2!.TotalCount.Should().Be(75, because: "total count should remain consistent across pages when no observations is added between requests");
    }

    [Fact]
    public async Task SearchByCursor_ReturnsBadRequest_WhenTakeExceedsLimit()
    {
        // Arrange
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto { OccurrenceStatus = OccurrenceStatusFilterValuesDto.Present };

        // Act
        var response = await apiClient.PostAsync(
            "/observations/searchbycursor?take=3000",
            JsonContent.Create(searchFilter));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SearchByCursor_ReturnsBadRequest_WhenSortByIsInvalid()
    {
        // Arrange
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto { OccurrenceStatus = OccurrenceStatusFilterValuesDto.Present };

        // Act
        var response = await apiClient.PostAsync(
            "/observations/searchbycursor?take=50&sortBy=invalid.field.name",
            JsonContent.Create(searchFilter));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}