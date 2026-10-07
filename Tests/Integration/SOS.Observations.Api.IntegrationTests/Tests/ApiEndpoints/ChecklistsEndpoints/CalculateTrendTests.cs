using SOS.Api.Contracts.Checklists.Requests;
using FizzWare.NBuilder;
using SOS.Api.Contracts.Observations.Requests;
using SOS.Api.Contracts.Checklists.Responses;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Observations.Api.IntegrationTests.Setup;
using SOS.Observations.Api.IntegrationTests.TestData.TestDataBuilder;
using FluentAssertions.Execution;

namespace SOS.Observations.Api.IntegrationTests.Tests.ApiEndpoints.ChecklistsEndpoints;

[Collection(TestCollection.Name)]
public class CalculateTrendTests : TestBase
{
    public CalculateTrendTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    [Fact]
    public async Task CalculateTrend_UsesTheDateFilter_WithOrWithoutMinimumEffort()
    {
        var verbatimChecklists = Builder<ArtportalenChecklistVerbatim>.CreateListOfSize(3)
            .All().HaveValuesFromPredefinedChecklists()
                .With(checklist => checklist.TaxonIds = [103026])
                .With(checklist => checklist.TaxonIdsFound = [103026])
            .Build();
        var starts = new[]
        {
            new DateTime(2000, 1, 31, 9, 0, 0, DateTimeKind.Utc),
            new DateTime(2000, 1, 31, 10, 0, 0, DateTimeKind.Utc),
            new DateTime(2000, 1, 31, 11, 0, 0, DateTimeKind.Utc)
        };
        for (var i = 0; i < verbatimChecklists.Count; i++)
        {
            verbatimChecklists[i].StartDate = starts[i];
            verbatimChecklists[i].EndDate = starts[i].AddMinutes(30);
        }
        await ProcessFixture.ProcessAndAddChecklistsToElasticSearch(verbatimChecklists);
        using var apiClient = TestFixture.CreateApiClient();

        foreach (var minEffortTime in new string?[] { null, "00:15:00" })
        {
            var filter = new CalculateTrendFilterDto
            {
                Date = new DateFilterDto
                {
                    StartDate = new DateTimeOffset(2000, 1, 31, 12, 45, 0, TimeSpan.FromHours(3)),
                    EndDate = new DateTimeOffset(2000, 1, 31, 13, 45, 0, TimeSpan.FromHours(3)),
                    DateFilterType = DateFilterTypeDto.BetweenStartDateAndEndDate
                },
                Checklist = minEffortTime is null ? null : new TrendChecklistFilterDto { MinEffortTime = minEffortTime },
                TaxonId = 103026
            };
            using var response = await apiClient.PostAsync("/checklists/calculatetrend", JsonContent.Create(filter), TestContext.Current.CancellationToken);

            response.StatusCode.Should().Be(HttpStatusCode.OK, $"minEffortTime={minEffortTime ?? "null"}");
            using var scope = new AssertionScope($"minEffortTime={minEffortTime ?? "null"}");
            var result = await response.Content.ReadFromJsonAsync<TaxonTrendResult>(TestContext.Current.CancellationToken);
            result!.NrChecklists.Should().Be(1);
            result.NrPresentObservations.Should().Be(1);
            result.NrAbsentObservations.Should().Be(0);
            result.Quotient.Should().Be(1);
        }
    }

    [Fact]
    public async Task TestCalculateTrend()
    {
        // Arrange
        var verbatimChecklists = Builder<ArtportalenChecklistVerbatim>.CreateListOfSize(5)
            .All()
                .HaveValuesFromPredefinedChecklists()
                .With(m => m.StartDate = DateTime.Now - TimeSpan.FromMinutes(30)) // 30 minutes search effort time.
                .With(m => m.EndDate = DateTime.Now)
            .TheFirst(3) // 3 talgoxe found
                .With(m => m.TaxonIds = new List<int>() { 103026, 102998 })
                .With(m => m.TaxonIdsFound = new List<int>() { 103026 })
            .TheNext(1) // 1 talgoxe not found
                .With(m => m.TaxonIds = new List<int>() { 103026, 102998 })
                .With(m => m.TaxonIdsFound = new List<int>() { 102998 })
            .TheNext(1) // didn't look for talgoxe
                .With(m => m.TaxonIds = new List<int>() { 102998 })
                .With(m => m.TaxonIdsFound = new List<int>() { })
            .Build();
        await ProcessFixture.ProcessAndAddChecklistsToElasticSearch(verbatimChecklists);
        var apiClient = TestFixture.CreateApiClient();
        var calculateTrendFilter = new CalculateTrendFilterDto
        {
            Checklist = new TrendChecklistFilterDto
            {
                MinEffortTime = "00:15:00"
            },
            TaxonId = 103026
        };

        // Act
        var response = await apiClient.PostAsync($"/checklists/calculatetrend", JsonContent.Create(calculateTrendFilter), TestContext.Current.CancellationToken);
        var result = await response.Content.ReadFromJsonAsync<TaxonTrendResult>(TestContext.Current.CancellationToken);

        // Assert
        result!.Quotient.Should().BeApproximately(3.0 / 4, 0.0001, "because there are 3 present observations and 1 absent");
        result.NrPresentObservations.Should().Be(3, "because there are 3 checklists matching the search criteria where talgoxe was found");
        result.NrAbsentObservations.Should().Be(1, "because one checklist matches the search criteria where talgoxe was not found");
        result.NrChecklists.Should().Be(4, "because there are 4 checklists matching the search criteria");
        result.TaxonId.Should().Be(103026);
    }
}
