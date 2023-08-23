using FizzWare.NBuilder;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Observations.Api.Dtos.Filter;
using SOS.Observations.Api.Dtos;
using SOS.ContainerIntegrationTests.Setup;
using SOS.ContainerIntegrationTests.TestData.TestDataBuilder;
using SOS.ContainerIntegrationTests.Helpers;

namespace SOS.ContainerIntegrationTests.Tests.ObservationsBySearchEndpoint;

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
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(20).IsInDateSpan("2000-01-01T00:00:00", "2000-01-31T23:59:59")
             .TheNext(60).IsInDateSpan("2000-02-01T00:00:00", "2000-02-29T23:59:59")
             .TheNext(20).IsInDateSpan("2000-03-01T00:00:00", "2000-03-31T23:59:59")
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
        var response = await apiClient.PostAsync($"/observations/search", JsonContent.Create(searchFilter));
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(60,
            because: "60 observations added to Elasticsearch have ProjectId = 1.");
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenFilteringByOverlappingStartDateAndEndDate()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(20).IsInDateSpan("2000-01-01T00:00:00", "2000-01-31T23:59:59")
             .TheNext(60).IsInDateSpan("2000-02-01T00:00:00", "2000-02-29T23:59:59")
             .TheNext(20).IsInDateSpan("2000-03-01T00:00:00", "2000-03-31T23:59:59")
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
        var response = await apiClient.PostAsync($"/observations/search", JsonContent.Create(searchFilter));
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(60,
            because: "60 observations added to Elasticsearch have ProjectId = 1.");
    }

    [Fact]
    [Trait("Category", "AutomaticIntegrationTest")]
    public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenFilteringByOnlyStartDate()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(20).IsInDateSpan("2000-01-01T00:00:00", "2000-01-31T23:59:59")
             .TheNext(60).IsInDateSpan("2000-02-01T00:00:00", "2000-02-29T23:59:59")
             .TheNext(20).IsInDateSpan("2000-03-01T00:00:00", "2000-03-31T23:59:59")
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
        var response = await apiClient.PostAsync($"/observations/search", JsonContent.Create(searchFilter));
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(60,
            because: "60 observations added to Elasticsearch have ProjectId = 1.");
    }

    [Fact]
    [Trait("Category", "AutomaticIntegrationTest")]
    public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenFilteringByOnlyEndDate()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(20).IsInDateSpan("2000-01-01T00:00:00", "2000-01-31T23:59:59")
             .TheNext(60).IsInDateSpan("2000-02-01T00:00:00", "2000-02-29T23:59:59")
             .TheNext(20).IsInDateSpan("2000-03-01T00:00:00", "2000-03-31T23:59:59")
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
        var response = await apiClient.PostAsync($"/observations/search", JsonContent.Create(searchFilter));
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();
        var debug = DebugTestOnlyEndDateFilter(verbatimObservations, processedObservations, result!.Records);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(60,
            because: "60 observations added to Elasticsearch have ProjectId = 1.");
    }

    private List<TestResultItem> DebugTestOnlyEndDateFilter(
        IList<ArtportalenObservationVerbatim> verbatimObservations,
        IEnumerable<Observation> allObservations,
        IEnumerable<Observation> resultObservations)
    {
        var testResultItems = DebugTestResultHelper.CreateTestResultSummary(verbatimObservations, allObservations, resultObservations);
        foreach (var item in testResultItems)
        {
            item.VerbatimValue = item.VerbatimObservation.EndDate!.Value;
            item.ProcessedValue = item.ProcessedObservation.Event.EndDate!.Value;
        }

        testResultItems = testResultItems
            .OrderBy(m => m.ProcessedObservation.Event.EndDate!.Value)
            .ToList();
        return testResultItems;
    }
}
