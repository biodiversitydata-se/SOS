using SOS.Api.Contracts.Common.Responses;
using SOS.Api.Contracts.Observations.Requests;
using FizzWare.NBuilder;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Observations.Api.IntegrationTests.Setup;
using SOS.Observations.Api.IntegrationTests.TestData.TestDataBuilder;
using SOS.Observations.Api.IntegrationTests.Helpers;
using FluentAssertions.Execution;

namespace SOS.Observations.Api.IntegrationTests.Tests.ApiEndpoints.ObservationsEndpoints.ObservationsBySearchEndpoint;

[Collection(TestCollection.Name)]
public class DateFilterTests : TestBase
{
    public DateFilterTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenFilteringByBetweenStartDateAndEndDate()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(5)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(1).IsInDateSpan("2000-01-01T00:00:00", "2000-01-31T23:59:59")
             .TheNext(3).IsInDateSpan("2000-02-01T00:00:00", "2000-02-29T23:59:59")
             .TheNext(1).IsInDateSpan("2000-03-01T00:00:00", "2000-03-31T23:59:59")
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto
        {
            Date = new DateFilterDto
            {
                StartDate = DateTime.Parse("2000-02-01T00:00:00"),
                EndDate = DateTime.Parse("2000-02-29T23:59:59"),
                DateFilterType = DateFilterTypeDto.BetweenStartDateAndEndDate
            }
        };

        // Act
        var response = await apiClient.PostAsync($"/observations/search", JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(3,
            because: "3 observations added to Elasticsearch have startdate and end date between 2000-02-01 and 2000-02-29.");
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenFilteringByOverlappingStartDateAndEndDate()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(5)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(1).IsInDateSpan("2000-01-01T00:00:00", "2000-01-31T23:59:59")
             .TheNext(3).IsInDateSpan("2000-02-01T00:00:00", "2000-02-29T23:59:59")
             .TheNext(1).IsInDateSpan("2000-03-01T00:00:00", "2000-03-31T23:59:59")
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto
        {
            Date = new DateFilterDto
            {
                StartDate = DateTime.Parse("2000-02-01T00:00:00"),
                EndDate = DateTime.Parse("2000-02-29T23:59:59"),
                DateFilterType = DateFilterTypeDto.OverlappingStartDateAndEndDate
            }
        };

        // Act
        var response = await apiClient.PostAsync($"/observations/search", JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(3,
            because: "3 observations added to Elasticsearch have startdate or end date overlapping 2000-02-01 and 2000-02-29.");
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenFilteringByOnlyStartDate()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(5)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(1).IsInDateSpan("2000-01-01T00:00:00", "2000-01-31T23:59:59")
             .TheNext(3).IsInDateSpan("2000-02-01T00:00:00", "2000-02-29T23:59:59")
             .TheNext(1).IsInDateSpan("2000-03-01T00:00:00", "2000-03-31T23:59:59")
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto
        {
            Date = new DateFilterDto
            {
                StartDate = DateTime.Parse("2000-02-01T00:00:00"),
                EndDate = DateTime.Parse("2000-02-29T23:59:59"),
                DateFilterType = DateFilterTypeDto.OnlyStartDate
            }
        };

        // Act
        var response = await apiClient.PostAsync($"/observations/search", JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(3,
            because: "3 observations added to Elasticsearch have startdate between 2000-02-01 and 2000-02-29.");
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenFilteringByOnlyEndDate()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(5)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(1).IsInDateSpan("2000-01-01T00:00:00", "2000-01-31T23:59:59")
             .TheNext(3).IsInDateSpan("2000-02-01T00:00:00", "2000-02-29T23:59:59")
             .TheNext(1).IsInDateSpan("2000-03-01T00:00:00", "2000-03-31T23:59:59")
            .Build();
        var processedObservations = await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto
        {
            Date = new DateFilterDto
            {
                StartDate = DateTime.Parse("2000-02-01T00:00:00"),
                EndDate = DateTime.Parse("2000-02-29T23:59:59"),
                DateFilterType = DateFilterTypeDto.OnlyEndDate
            }
        };
        
        // Act
        var response = await apiClient.PostAsync($"/observations/search", JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>(TestContext.Current.CancellationToken);
        var debug = DebugTestOnlyEndDateFilter(verbatimObservations, processedObservations, result!.Records);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(3,
            because: "3 observations added to Elasticsearch have end date between 2000-02-01 and 2000-02-29.");
    }

    [Fact]
    public async Task SearchEndpoint_PreservesExplicitTimeZones()
    {
        var observations = await AddObservationsWithDatesAsync(
            new DateTime(2000, 1, 31, 9, 30, 0, DateTimeKind.Utc),
            new DateTime(2000, 1, 31, 10, 0, 0, DateTimeKind.Utc),
            new DateTime(2000, 1, 31, 11, 0, 0, DateTimeKind.Utc));
        using var apiClient = TestFixture.CreateApiClient();
        var cases = new[]
        {
            (Endpoint: "/observations/search", DateField: "date", Start: "2000-01-31T09:45:00Z", End: "2000-01-31T10:45:00Z"),
            (Endpoint: "/observations/search", DateField: "date", Start: "2000-01-31T12:45:00+03:00", End: "2000-01-31T13:45:00+03:00"),
            (Endpoint: "/observations/search", DateField: "date", Start: "2000-01-31T04:45:00-05:00", End: "2000-01-31T05:45:00-05:00"),
            (Endpoint: "/observations/internal/search", DateField: "date", Start: "2000-01-31T12:45:00+03:00", End: "2000-01-31T13:45:00+03:00"),
            (Endpoint: "/observations/internal/search", DateField: "modifiedDate", Start: "2000-01-31T12:45:00+03:00", End: "2000-01-31T13:45:00+03:00"),
            (Endpoint: "/observations/internal/search", DateField: "extendedFilter", Start: "2000-01-31T12:45:00+03:00", End: "2000-01-31T13:45:00+03:00")
        };

        foreach (var (endpoint, dateField, start, end) in cases)
        {
            var dateJson = dateField switch
            {
                "modifiedDate" => $$"""{"from":"{{start}}","to":"{{end}}"}""",
                "extendedFilter" => $$"""{"reportedDateFrom":"{{start}}","reportedDateTo":"{{end}}"}""",
                _ => $$"""{"startDate":"{{start}}","endDate":"{{end}}","dateFilterType":"BetweenStartDateAndEndDate"}"""
            };
            var json = $$"""{"{{dateField}}":{{dateJson}}}""";
            using var request = new StringContent(json, Encoding.UTF8, "application/json");
            using var response = await apiClient.PostAsync(endpoint, request, TestContext.Current.CancellationToken);

            response.StatusCode.Should().Be(HttpStatusCode.OK, $"endpoint={endpoint}, dateField={dateField}, start={start}, end={end}");
            using var scope = new AssertionScope($"endpoint={endpoint}, dateField={dateField}, start={start}, end={end}");
            var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>(TestContext.Current.CancellationToken);
            result!.TotalCount.Should().Be(1);
            result.Records!.Select(observation => observation.Occurrence.OccurrenceId)
                .Should().Equal(observations[1].Occurrence.OccurrenceId);
        }
    }

    [Fact]
    public async Task SearchEndpoint_DateOnlyEndDate_IncludesTheDayButNotTheNextDay()
    {
        var observations = await AddObservationsWithDatesAsync(
            new DateTime(2000, 1, 31, 0, 0, 0, DateTimeKind.Local),
            new DateTime(2000, 1, 31, 12, 0, 0, DateTimeKind.Local),
            new DateTime(2000, 1, 31, 23, 59, 59, DateTimeKind.Local),
            new DateTime(2000, 2, 1, 0, 0, 0, DateTimeKind.Local));
        // Send date-only JSON directly: serializing a DTO would supply an explicit midnight.
        const string json = """{"date":{"startDate":"2000-01-31","endDate":"2000-01-31","dateFilterType":"BetweenStartDateAndEndDate"}}""";
        using var apiClient = TestFixture.CreateApiClient();

        foreach (var endpoint in new[] { "/observations/search", "/observations/internal/search" })
        {
            using var request = new StringContent(json, Encoding.UTF8, "application/json");
            using var response = await apiClient.PostAsync(endpoint, request, TestContext.Current.CancellationToken);

            response.StatusCode.Should().Be(HttpStatusCode.OK, $"endpoint={endpoint}");
            using var scope = new AssertionScope($"endpoint={endpoint}");
            var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>(TestContext.Current.CancellationToken);
            result!.TotalCount.Should().Be(3);
            result.Records!.Select(observation => observation.Occurrence.OccurrenceId)
                .Should().BeEquivalentTo(observations.Take(3).Select(observation => observation.Occurrence.OccurrenceId));
        }
    }

    [Fact]
    public async Task SearchEndpoint_PeriodAcrossYears_UsesLocalCalendarDates()
    {
        var observations = await AddObservationsWithDatesAsync(
            new DateTime(2020, 1, 30, 12, 0, 0, DateTimeKind.Local),
            new DateTime(2020, 1, 31, 12, 0, 0, DateTimeKind.Local),
            new DateTime(2020, 2, 1, 12, 0, 0, DateTimeKind.Local));
        var filter = new SearchFilterInternalDto
        {
            Date = new DateFilterDto
            {
                StartDate = new DateTime(2026, 1, 31, 0, 0, 0, DateTimeKind.Local),
                EndDate = new DateTime(2026, 1, 31, 23, 59, 59, DateTimeKind.Local),
                DateFilterType = DateFilterTypeDto.BetweenStartDateAndEndDate
            },
            ExtendedFilter = new ExtendedFilterDto { UsePeriodForAllYears = true }
        };
        using var apiClient = TestFixture.CreateApiClient();

        var response = await apiClient.PostAsync("/observations/internal/search", JsonContent.Create(filter), TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>(TestContext.Current.CancellationToken);
        result!.TotalCount.Should().Be(1);
        result.Records!.Select(observation => observation.Occurrence.OccurrenceId)
            .Should().Equal(observations[1].Occurrence.OccurrenceId);
    }

    private async Task<List<Observation>> AddObservationsWithDatesAsync(params DateTime[] dates)
    {
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(dates.Length)
            .All().HaveValuesFromPredefinedObservations()
            .Build();
        var observations = ProcessFixture.ProcessObservations(verbatimObservations);
        for (var i = 0; i < observations.Count; i++)
        {
            observations[i].Event = new Event(dates[i]);
            observations[i].Modified = dates[i].ToUniversalTime();
            observations[i].Occurrence.ReportedDate = dates[i].ToUniversalTime();
        }
        await ProcessFixture.AddObservationsToElasticsearchAsync(observations);
        return observations;
    }

    private List<TestResultItem> DebugTestOnlyEndDateFilter(
        IList<ArtportalenObservationVerbatim> verbatimObservations,
        IEnumerable<Observation> allObservations,
        IEnumerable<Observation> resultObservations)
    {
        var testResultItems = DebugTestResultHelper.CreateTestResultSummary(verbatimObservations, allObservations, resultObservations);
        foreach (var item in testResultItems)
        {
            item.VerbatimValue = item.VerbatimObservation?.EndDate!.Value;
            item.ProcessedValue = item.ProcessedObservation?.Event.EndDate!.Value;
        }

        testResultItems = testResultItems
            .OrderBy(m => m.ProcessedObservation?.Event.EndDate!.Value)
            .ToList();
        return testResultItems;
    }
}
