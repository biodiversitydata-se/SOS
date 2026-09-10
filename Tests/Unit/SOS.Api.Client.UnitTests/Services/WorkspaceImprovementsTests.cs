using System.Text.Json;
using SOS.Api.Client.Abstractions;
using SOS.Api.Client.Exceptions;
using SOS.Api.Client.Services;
using SOS.Api.Client.Workspaces;
using SOS.Api.Contracts.Workspaces;
using Xunit;

namespace SOS.Api.Client.UnitTests.Services;

public class WorkspaceImprovementsTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;
    private static SosWorkspaceFilterService Service(ISosWorkspaceTaxonSource? source = null) =>
        new(new Clock(), source is null ? [] : [source]);

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task EmptySelectedSource_DoesNotFallBackToOtherSelection(bool emptyTaxa)
    {
        var filter = await Service(new Source { List = emptyTaxa ? [10] : [] }).CreateSearchFilterAsync(new SosWorkspaceDocument
        {
            Search = new() { TaxonSelection = new()
            {
                Taxa = [new() { TaxonId = 10, IncludeChildren = emptyTaxa }],
                TaxonCategoryIds = [99], TaxonListIds = [1]
            }}
        }, cancellationToken: Ct);
        Assert.Equal([-1], filter.Taxon!.Ids);
    }

    [Fact]
    public async Task LookupFailure_IsPropagated()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() => Service(new Source { Fail = true })
            .CreateSearchFilterAsync(new SosWorkspaceDocument { Search = new() { TaxonSelection = new() { TaxonListIds = [1] } } }, cancellationToken: Ct));
    }

    [Theory]
    [InlineData(WorkspaceDateFilterMode.CurrentYear, 2026)]
    [InlineData(WorkspaceDateFilterMode.PreviousYear, 2025)]
    [InlineData(WorkspaceDateFilterMode.LastFiveCalendarYears, 2022)]
    [InlineData(WorkspaceDateFilterMode.LastTwentyFiveCalendarYears, 2002)]
    [InlineData(WorkspaceDateFilterMode.LastFiveYears, 2021)]
    [InlineData(WorkspaceDateFilterMode.LastTwentyFiveYears, 2001)]
    public async Task BroadOverrides_PreserveRelativeBounds(WorkspaceDateFilterMode mode, int startYear)
    {
        foreach (var observation in new[] { true, false })
        {
            var document = new SosWorkspaceDocument { Search = new() { DateFilter = new() { Mode = mode, FilterByObservationDate = observation } } };
            var original = await Service().CreateSearchFilterAsync(document, cancellationToken: Ct);
            var filter = await Service().CreateSearchFilterAsync(document, new() { DateFrom = new(1900, 1, 1), DateTo = new(2099, 12, 31) }, Ct);
            Assert.Equal(original.Date?.StartDate, filter.Date?.StartDate);
            Assert.Equal(original.Date?.EndDate, filter.Date?.EndDate);
            Assert.Equal(original.ExtendedFilter!.ReportedDateFrom, filter.ExtendedFilter!.ReportedDateFrom);
            Assert.Equal(original.ExtendedFilter.ReportedDateTo, filter.ExtendedFilter.ReportedDateTo);
            Assert.Equal(startYear, (filter.Date?.StartDate ?? filter.ExtendedFilter.ReportedDateFrom)!.Value.Year);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task OneSidedOverride_KeepsOtherBoundary(bool observation)
    {
        var filter = await Service().CreateSearchFilterAsync(new SosWorkspaceDocument { Search = new() { DateFilter = new()
        { Mode = WorkspaceDateFilterMode.PreviousYear, FilterByObservationDate = observation } } }, new() { DateFrom = new(2025, 6, 1) }, Ct);
        Assert.Equal(new DateTime(2025, 6, 1), (filter.Date?.StartDate ?? filter.ExtendedFilter!.ReportedDateFrom)!.Value.Date);
        Assert.Equal(new DateTime(2025, 12, 31), (filter.Date?.EndDate ?? filter.ExtendedFilter!.ReportedDateTo)!.Value.Date);
    }

    [Theory]
    [InlineData(2027, 2028)]
    [InlineData(2025, 2024)]
    public async Task NonOverlappingOrReversedOverrides_Fail(int first, int last)
    {
        await Assert.ThrowsAsync<SosWorkspaceFilterValidationException>(() => Service().CreateSearchFilterAsync(
            new SosWorkspaceDocument { Search = new() { DateFilter = new() { Mode = WorkspaceDateFilterMode.CurrentYear } } },
            new() { DateFrom = new(first, 1, 1), DateTo = new(last, 12, 31) }, Ct));
    }

    [Fact]
    public async Task RecurringPeriodAcrossNewYear_CanBeNarrowedToOneSegment()
    {
        var document = Recurring();
        var filter = await Service().CreateSearchFilterAsync(document, new() { DateFrom = new(2024, 1, 1), DateTo = new(2024, 4, 30) }, Ct);
        Assert.Equal(new DateTime(2024, 2, 28), filter.Date!.EndDate!.Value.Date);
        Assert.False(filter.ExtendedFilter!.UsePeriodForAllYears);
        Assert.True(document.Search.DateFilter.IgnoreYear);
        await Assert.ThrowsAsync<SosWorkspaceFilterValidationException>(() => Service().CreateSearchFilterAsync(document,
            new() { DateFrom = new(2024, 1, 1), DateTo = new(2024, 12, 31) }, Ct));
    }

    [Fact]
    public async Task RecurringYearRestriction_IsPreserved()
    {
        var document = Recurring();
        document.Search.DateFilter.StartYear = 2020;
        document.Search.DateFilter.EndYear = 2023;
        await Assert.ThrowsAsync<SosWorkspaceFilterValidationException>(() => Service().CreateSearchFilterAsync(document,
            new() { DateFrom = new(2024, 1, 1), DateTo = new(2024, 2, 1) }, Ct));
    }

    [Fact]
    public async Task LeapDayPeriod_WorksWithNonLeapClockAndLeapTargetYear()
    {
        var document = Recurring();
        document.Search.DateFilter.StartDate = document.Search.DateFilter.EndDate = new(2000, 2, 29);
        var filter = await Service().CreateSearchFilterAsync(document, new() { DateFrom = new(2024, 1, 1), DateTo = new(2024, 12, 31) }, Ct);
        Assert.Equal(new DateTime(2024, 2, 29), filter.Date!.StartDate!.Value.Date);
        await Assert.ThrowsAsync<SosWorkspaceFilterValidationException>(() => Service().CreateSearchFilterAsync(document,
            new() { DateFrom = new(2025, 1, 1), DateTo = new(2025, 12, 31) }, Ct));
    }

    [Theory]
    [InlineData("{}", "SchemaVersion")]
    [InlineData("{\"schemaVersion\":\"1\"}", "SchemaVersion")]
    [InlineData("{\"schemaVersion\":2,\"search\":{\"futureField\":true}}", "SchemaVersion")]
    [InlineData("{\"schemaVersion\":1}", "Document")]
    [InlineData("{\"schemaVersion\":1,\"search\":null}", "Search")]
    [InlineData("{\"schemaVersion\":1,\"search\":{\"futureField\":true}}", "Document")]
    [InlineData("{", "Document")]
    public void Deserialize_ReportsClearErrors(string json, string field)
    {
        Assert.Equal(field, Assert.Throws<SosWorkspaceFilterValidationException>(() => SosWorkspaceDocuments.Deserialize(json)).ParameterName);
    }

    [Fact]
    public void DocumentRoundTrip_PreservesMetadataAndOtherApps()
    {
        var document = SosWorkspaceDocuments.Deserialize("{\"schemaVersion\":1,\"search\":{},\"futureMetadata\":42,\"applicationState\":{\"other\":{\"zoom\":3}}}");
        var state = new State { Values = [1] };
        document.SetApplicationState("test", state);
        state.Values.Add(2);
        var loaded = SosWorkspaceDocuments.Deserialize(SosWorkspaceDocuments.Serialize(document));
        var read = loaded.GetApplicationState<State>("test")!;
        Assert.Equal([1], read.Values);
        read.Values.Add(3);
        Assert.Equal([1], loaded.GetApplicationState<State>("test")!.Values);
        Assert.Equal(3, loaded.ApplicationState!["other"].GetProperty("zoom").GetInt32());
        Assert.Equal(42, loaded.ExtensionData!["futureMetadata"].GetInt32());
        Assert.Null(loaded.GetApplicationState<State>("missing"));
    }

    [Fact]
    public void Validate_DoesNotResolveTaxaAndRejectsInvalidDatesBeforeSaving()
    {
        var document = new SosWorkspaceDocument { Search = new() { TaxonSelection = new() { TaxonListIds = [123] } } };
        SosWorkspaceDocuments.Validate(document);
        document.Search.DateFilter = new() { Mode = WorkspaceDateFilterMode.Custom, StartDate = new(2025, 1, 1), EndDate = new(2024, 1, 1) };
        Assert.Throws<SosWorkspaceFilterValidationException>(() => SosWorkspaceDocuments.Serialize(document));
        document.Search.DateFilter.IgnoreYear = true;
        SosWorkspaceDocuments.Validate(document);
    }

    private static SosWorkspaceDocument Recurring() => new() { Search = new() { DateFilter = new()
    { Mode = WorkspaceDateFilterMode.Custom, IgnoreYear = true, StartDate = new(2000, 11, 1), EndDate = new(2000, 2, 28) } } };
    public class State { public List<int> Values { get; set; } = []; }
    private sealed class Clock : TimeProvider
    { public override DateTimeOffset GetUtcNow() => new(2026, 7, 1, 12, 0, 0, TimeSpan.Zero); }
    private sealed class Source : ISosWorkspaceTaxonSource
    {
        public IReadOnlyCollection<int> List { get; init; } = [];
        public bool Fail { get; init; }
        public Task<SosResolvedTaxon?> GetTaxonAsync(int id, CancellationToken cancellationToken = default) => Task.FromResult<SosResolvedTaxon?>(new(id, 1));
        public Task<IReadOnlyCollection<SosResolvedTaxon>> GetDescendantsAsync(int id, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<SosResolvedTaxon>>([]);
        public Task<IReadOnlyCollection<int>> GetTaxonListIdsAsync(int id, bool includeChildTaxaForRedlists, CancellationToken cancellationToken = default) =>
            Fail ? throw new InvalidOperationException("Lookup failed") : Task.FromResult(List);
    }
}
