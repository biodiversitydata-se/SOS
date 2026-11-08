using SOS.Lib.Gis;
using SOS.Lib.Models.Processed.Observation;
using System.Text.Json.Nodes;

namespace SOS.Observations.Api.Services.AdaptiveSearchLocations;

internal interface IAdaptiveSearchLocationAggregator
{
    /// <summary>
    /// Groups observation records by geographic position and summarizes each group.
    /// </summary>
    /// <param name="records">Records projected with the location feature output fields.</param>
    /// <param name="observationCountByLocationId">
    /// Total observation counts per location id when the records are representatives (one per location).
    /// </param>
    /// <param name="isSummaryApproximated">True when the records may not include every matching observation.</param>
    /// <param name="isPresentationStatusApproximated">Defaults to <paramref name="isSummaryApproximated"/>.</param>
    List<LocationAggregate> CreateAggregates(
        IEnumerable<JsonObject> records,
        IReadOnlyDictionary<string, long>? observationCountByLocationId = null,
        bool isSummaryApproximated = false,
        bool? isPresentationStatusApproximated = null);
    int CountLocations(IEnumerable<JsonObject> records);
    IEnumerable<List<Observation>> GroupObservationsByLocation(IEnumerable<Observation> observations);
}

internal sealed class AdaptiveSearchLocationAggregator : IAdaptiveSearchLocationAggregator
{
    public List<LocationAggregate> CreateAggregates(
        IEnumerable<JsonObject> records,
        IReadOnlyDictionary<string, long>? observationCountByLocationId = null,
        bool isSummaryApproximated = false,
        bool? isPresentationStatusApproximated = null)
    {
        return GroupObservationsByLocation(MapObservations(records))
            .Select(observations => CreateAggregate(observations, observationCountByLocationId, isSummaryApproximated,
                isPresentationStatusApproximated ?? isSummaryApproximated))
            .ToList();
    }

    public int CountLocations(IEnumerable<JsonObject> records)
    {
        return GroupObservationsByLocation(MapObservations(records)).Count();
    }

    public IEnumerable<List<Observation>> GroupObservationsByLocation(IEnumerable<Observation> observations)
    {
        return observations
            .Where(HasUsableLocation)
            .GroupBy(observation => observation.Location!.DistinctLocationId, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.ToList());
    }

    private static IEnumerable<Observation> MapObservations(IEnumerable<JsonObject> records)
    {
        return records.Select(LocationObservationMapper.Map);
    }

    private static LocationAggregate CreateAggregate(
        List<Observation> observations,
        IReadOnlyDictionary<string, long>? observationCountByLocationId,
        bool isSummaryApproximated,
        bool isPresentationStatusApproximated)
    {
        var locations = observations
            .Select(observation => observation.Location!)
            .OrderBy(location => location.LocationId, StringComparer.Ordinal)
            .DistinctBy(location => location.LocationId, StringComparer.OrdinalIgnoreCase)
            .ToList();
        var observationSummary = CreateLocationObservationSummary(observations);

        // Representative records contain one observation per location; the real totals come from the aggregation.
        var representativeObservationCount = GetRepresentativeObservationCount(locations, observationCountByLocationId);
        var hasObservationsNotFetched = representativeObservationCount > observations.Count;
        if (representativeObservationCount.HasValue)
        {
            ApplyRepresentativeObservationCount(observationSummary, representativeObservationCount.Value);
        }

        return new LocationAggregate
        {
            DistinctLocationId = locations[0].DistinctLocationId,
            Location = locations[0],
            Locations = locations,
            GeneralizedLocationIds = observations
                .Where(observation => observation.IsGeneralized)
                .Select(observation => observation.Location!.LocationId)
                .ToHashSet(StringComparer.OrdinalIgnoreCase),
            TaxonIds = observations
                .Where(observation => observation.Taxon != null)
                .Select(observation => observation.Taxon.Id)
                .Distinct()
                .ToList(),
            ObservationSummary = observationSummary,
            IsSummaryApproximated = isSummaryApproximated && hasObservationsNotFetched,
            IsPresentationStatusApproximated = isPresentationStatusApproximated &&
                (!representativeObservationCount.HasValue || hasObservationsNotFetched)
        };
    }

    private static long? GetRepresentativeObservationCount(
        IEnumerable<Location> distinctLocations,
        IReadOnlyDictionary<string, long>? observationCountByLocationId)
    {
        if (observationCountByLocationId == null || observationCountByLocationId.Count == 0)
        {
            return null;
        }

        long observationCount = 0;
        var hasObservationCount = false;
        foreach (var location in distinctLocations)
        {
            if (observationCountByLocationId.TryGetValue(location.LocationId, out var count))
            {
                observationCount += count;
                hasObservationCount = true;
            }
        }

        return hasObservationCount ? observationCount : null;
    }

    private static void ApplyRepresentativeObservationCount(LocationObservationSummary summary, long observationCount)
    {
        var count = ToIntCount(observationCount);
        if (count <= 0)
        {
            return;
        }

        summary.ObservationCount = count;
        summary.DebugObservations = count <= 5 ? summary.DebugObservations : null;
    }

    private static int ToIntCount(long count)
    {
        return count > int.MaxValue
            ? int.MaxValue
            : Math.Max(0, (int)count);
    }

    private static LocationObservationSummary CreateLocationObservationSummary(IReadOnlyCollection<Observation> observations)
    {
        int? minPresentEndYear = null;
        int? maxPresentEndYear = null;
        int? minNotRecoveredEndYear = null;
        int? maxNotRecoveredEndYear = null;
        var presentationStatusSummaries = new Dictionary<PresentationStatusSummaryKey, int>();
        var summary = new LocationObservationSummary
        {
            ObservationCount = observations.Count,
            IsAllObservationsSensitive = observations.Count > 0 && observations.All(observation => observation.Sensitive),
            AllObservationsArePresent = observations.Count > 0 && observations.All(observation => observation.Occurrence?.IsPositiveObservation == true),
            DebugObservations = observations.Count <= 5
                ? observations.Select(CreateDebugObservation).ToList()
                : null
        };

        foreach (var observation in observations)
        {
            AddPresentationStatusSummary(presentationStatusSummaries, observation);

            if (observation.Occurrence?.IsPositiveObservation == true)
            {
                summary.FoundCount++;
                AddYear(observation.Event?.EndYear ?? observation.Event?.StartYear, ref minPresentEndYear, ref maxPresentEndYear);
            }

            if (observation.IsGeneralized)
            {
                summary.DiffusedCount++;
            }

            if ((observation.ArtportalenInternal?.InvasiveSpeciesTreatment?.Id ?? -1) >= 0)
            {
                summary.TreatedCount++;
            }

            if (observation.Taxon?.Attributes?.IsRedlisted == true)
            {
                summary.RedlistedCount++;
            }

            if (observation.Taxon?.Attributes?.IsInvasiveInSweden == true)
            {
                summary.InvasiveInSwedenCount++;
            }

            if (observation.Occurrence?.IsNotRediscoveredObservation == true)
            {
                summary.NotRecoveredCount++;
                AddYear(observation.Event?.EndYear ?? observation.Event?.StartYear, ref minNotRecoveredEndYear, ref maxNotRecoveredEndYear);
            }

            if (observation.Occurrence?.IsNeverFoundObservation == true)
            {
                summary.NeverFoundCount++;
            }
        }

        summary.MinPresentEndYear = minPresentEndYear;
        summary.MaxPresentEndYear = maxPresentEndYear;
        summary.MinNotRecoveredEndYear = minNotRecoveredEndYear;
        summary.MaxNotRecoveredEndYear = maxNotRecoveredEndYear;
        summary.PresentationStatusSummaries = presentationStatusSummaries
            .Select(item => new LocationObservationSummary.PresentationStatusSummary
            {
                IsPresent = item.Key.IsPresent,
                IsSensitive = item.Key.IsSensitive,
                IsTreated = item.Key.IsTreated,
                IsNotRecovered = item.Key.IsNotRecovered,
                EndYear = item.Key.EndYear,
                Count = item.Value
            })
            .ToList();
        return summary;
    }

    private static DebugObservation CreateDebugObservation(Observation observation)
    {
        return new DebugObservation
        {
            EndYear = observation.Event?.EndYear ?? observation.Event?.StartYear,
            Diffused = observation.IsGeneralized,
            Sensitive = observation.Sensitive,
            Treatment = observation.ArtportalenInternal?.InvasiveSpeciesTreatment?.Id,
            Positive = observation.Occurrence?.IsPositiveObservation,
            NotRedisc = observation.Occurrence?.IsNotRediscoveredObservation,
            NeverFound = observation.Occurrence?.IsNeverFoundObservation
        };
    }

    private static void AddYear(int? year, ref int? minYear, ref int? maxYear)
    {
        if (!year.HasValue)
        {
            return;
        }

        minYear = minYear.HasValue ? Math.Min(minYear.Value, year.Value) : year.Value;
        maxYear = maxYear.HasValue ? Math.Max(maxYear.Value, year.Value) : year.Value;
    }

    private static void AddPresentationStatusSummary(
        Dictionary<PresentationStatusSummaryKey, int> presentationStatusSummaries,
        Observation observation)
    {
        var key = new PresentationStatusSummaryKey(
            observation.Occurrence?.IsPositiveObservation == true,
            observation.Sensitive,
            (observation.ArtportalenInternal?.InvasiveSpeciesTreatment?.Id ?? -1) >= 0,
            observation.Occurrence?.IsNotRediscoveredObservation == true,
            observation.Event?.EndYear ?? observation.Event?.StartYear);

        presentationStatusSummaries[key] = presentationStatusSummaries.TryGetValue(key, out var count)
            ? count + 1
            : 1;
    }

    private static bool HasUsableLocation(Observation observation)
    {
        return !string.IsNullOrWhiteSpace(observation?.Location?.LocationId);
    }
}

internal readonly record struct PresentationStatusSummaryKey(
    bool IsPresent,
    bool IsSensitive,
    bool IsTreated,
    bool IsNotRecovered,
    int? EndYear);