using FluentAssertions;
using SOS.Api.Contracts.Observations.Requests;
using SOS.Lib.Extensions.Dto;
using SOS.Observations.Api.Services.AdaptiveSearchLocations;
using Xunit;

namespace SOS.Observations.Api.UnitTests.Services;

public class AdaptiveSearchLocationsStrategySelectorTests
{
    public static IEnumerable<object[]> Filters()
    {
        // The seven combinations in OccurrenceStatusTests.TestOccurrenceStatusScenarios.
        yield return Case(OccurrenceStatusFilterValuesDto.Present, SightingNotRecoveredFilterDto.NoFilter, SightingNotPresentFilterDto.DontIncludeNotPresent, true);
        yield return Case(OccurrenceStatusFilterValuesDto.Absent, SightingNotRecoveredFilterDto.NoFilter, SightingNotPresentFilterDto.IncludeNotPresent, false);
        yield return Case(OccurrenceStatusFilterValuesDto.Absent, SightingNotRecoveredFilterDto.OnlyNotRecovered, SightingNotPresentFilterDto.IncludeNotPresent, false);
        yield return Case(OccurrenceStatusFilterValuesDto.BothPresentAndAbsent, SightingNotRecoveredFilterDto.NoFilter, SightingNotPresentFilterDto.IncludeNotPresent, false);
        yield return Case(OccurrenceStatusFilterValuesDto.Absent, SightingNotRecoveredFilterDto.DontIncludeNotRecovered, SightingNotPresentFilterDto.IncludeNotPresent, false);
        yield return Case(OccurrenceStatusFilterValuesDto.BothPresentAndAbsent, SightingNotRecoveredFilterDto.DontIncludeNotRecovered, SightingNotPresentFilterDto.IncludeNotPresent, false);
        yield return Case(OccurrenceStatusFilterValuesDto.BothPresentAndAbsent, SightingNotRecoveredFilterDto.IncludeNotRecovered, SightingNotPresentFilterDto.DontIncludeNotPresent, false);
        yield return [new SearchFilterInternalDto(), true];
        yield return [new SearchFilterInternalDto { NotRecoveredFilter = SightingNotRecoveredFilterDto.IncludeNotRecovered }, false];
        yield return [new SearchFilterInternalDto { ExtendedFilter = new() { NotPresentFilter = SightingNotPresentFilterDto.IncludeNotPresent } }, false];
        yield return [new SearchFilterInternalDto { OccurrenceStatus = OccurrenceStatusFilterValuesDto.Present, ProtectionFilter = ProtectionFilterDto.Sensitive }, true];
        yield return [new SearchFilterInternalDto { OccurrenceStatus = OccurrenceStatusFilterValuesDto.Present, ProtectionFilter = ProtectionFilterDto.BothPublicAndSensitive }, false];
        yield return [Absent(new() { InvasiveSpeciesTreatmentIds = [0, 1] }), true];
        yield return [Absent(new() { InvasiveSpeciesTreatmentIds = [] }), false];
        yield return [Absent(new() { InvasiveSpeciesTreatmentIds = [-1, 0] }), false];
        yield return [Absent(new() { HasInvasiveSpeciesTreatment = true }), false];
        yield return [Absent(new() { HasInvasiveSpeciesTreatment = false }), false];
        yield return [Absent(new() { HasInvasiveSpeciesTreatment = false }, SightingNotRecoveredFilterDto.OnlyNotRecovered), true];
        yield return [Absent(new() { HasInvasiveSpeciesTreatment = false }, SightingNotRecoveredFilterDto.DontIncludeNotRecovered), true];
    }

    [Theory]
    [MemberData(nameof(Filters))]
    public void Auto_UsesNormalizedFilterAndOnlySelectsSafeRepresentatives(SearchFilterInternalDto filter, bool safe)
    {
        var normalized = filter.ToSearchFilterInternal(0, "sv-SE", "", SearchSortOrder.Asc);
        var request = new AdaptiveSearchLocationsRequest { Filter = filter };

        AdaptiveSearchLocationsStrategySelector.Resolve(request, normalized).Should().Be(safe
            ? AdaptiveSearchLocationsStrategy.LocationsThenGrid
            : AdaptiveSearchLocationsStrategy.ObservationsThenGrid);
        AdaptiveSearchLocationsStrategySelector.CanDetermineRepresentativePresentationStatus(normalized, null).Should().Be(safe);
        AdaptiveSearchLocationsStrategySelector.Resolve(new AdaptiveSearchLocationsRequest { Filter = filter, CutoffYear = 2024 }, normalized)
            .Should().Be(AdaptiveSearchLocationsStrategy.ObservationsThenGrid);
        AdaptiveSearchLocationsStrategySelector.Resolve(new AdaptiveSearchLocationsRequest { Filter = filter, CreateSampleObservations = true }, normalized)
            .Should().Be(AdaptiveSearchLocationsStrategy.ObservationsThenGrid);
    }

    [Theory]
    [InlineData(AdaptiveSearchLocationsStrategy.ObservationsThenGrid)]
    [InlineData(AdaptiveSearchLocationsStrategy.ObservationsThenLocationsThenGrid)]
    [InlineData(AdaptiveSearchLocationsStrategy.LocationsThenGrid)]
    public void ExplicitStrategies_ArePreserved(AdaptiveSearchLocationsStrategy strategy)
    {
        var filter = new SearchFilterInternalDto();
        var request = new AdaptiveSearchLocationsRequest { Filter = filter, AdaptiveSearchStrategy = strategy, CutoffYear = 2024 };
        AdaptiveSearchLocationsStrategySelector.Resolve(request, filter.ToSearchFilterInternal(0, "sv-SE", "", SearchSortOrder.Asc))
            .Should().Be(strategy);
    }

    private static object[] Case(OccurrenceStatusFilterValuesDto occurrence, SightingNotRecoveredFilterDto recovered, SightingNotPresentFilterDto present, bool safe) =>
        [new SearchFilterInternalDto { OccurrenceStatus = occurrence, NotRecoveredFilter = recovered, ExtendedFilter = new() { NotPresentFilter = present } }, safe];

    private static SearchFilterInternalDto Absent(ExtendedFilterDto extended, SightingNotRecoveredFilterDto recovered = SightingNotRecoveredFilterDto.NoFilter) =>
        new() { OccurrenceStatus = OccurrenceStatusFilterValuesDto.Absent, NotRecoveredFilter = recovered, ExtendedFilter = extended };
}
