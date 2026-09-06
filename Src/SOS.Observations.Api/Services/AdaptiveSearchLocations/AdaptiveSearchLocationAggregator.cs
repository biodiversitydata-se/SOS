using SOS.Lib.Extensions;
using SOS.Lib.Gis;
using SOS.Lib.Models.Processed.Observation;
using System.Globalization;
using System.Text.Json.Nodes;
using ProcessedLocationType = SOS.Lib.Enums.LocationType;

namespace SOS.Observations.Api.Services.AdaptiveSearchLocations;

internal interface IAdaptiveSearchLocationAggregator
{
    List<CachedLocationAggregate> CreateAggregates(
        IEnumerable<JsonObject> records,
        IReadOnlyDictionary<string, long>? observationCountByLocationId = null,
        bool isSummaryApproximated = false);
    List<JsonObject> CreateSampleObservations(IEnumerable<JsonObject>? records);
    int CountLocations(IEnumerable<JsonObject> records);
    IEnumerable<List<Observation>> GroupObservationsByLocation(IEnumerable<Observation> observations);
}

internal sealed class AdaptiveSearchLocationAggregator : IAdaptiveSearchLocationAggregator
{
    private const int SampleObservationsPerLocationType = 3;
    private const double SampleObservationOffsetDegrees = 0.002d;
    private static readonly int[] DiffusedObservationCoordinateUncertaintyInMetersValues = { 100, 500, 1000 };

    public List<CachedLocationAggregate> CreateAggregates(
        IEnumerable<JsonObject> records,
        IReadOnlyDictionary<string, long>? observationCountByLocationId = null,
        bool isSummaryApproximated = false)
    {
        var observations = records
            .Select(LocationObservationMapper.Map)
            .Where(HasUsableLocation)
            .ToList();

        return GroupObservationsByLocation(observations)
            .Select(observations => CreateAggregate(observations, observationCountByLocationId, isSummaryApproximated))
            .ToList();
    }

    public int CountLocations(IEnumerable<JsonObject> records)
    {
        var observations = records
            .Select(LocationObservationMapper.Map)
            .Where(HasUsableLocation)
            .ToList();

        return GroupObservationsByLocation(observations).Count();
    }

    public IEnumerable<List<Observation>> GroupObservationsByLocation(IEnumerable<Observation> observations)
    {
        return observations
            .Where(HasUsableLocation)
            .GroupBy(observation => observation.Location!.DistinctLocationId, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.ToList());
    }

    public List<JsonObject> CreateSampleObservations(IEnumerable<JsonObject>? records)
    {
        var recordList = records?
            .Select(record => record.DeepClone().AsObject())
            .ToList() ?? new List<JsonObject>();
        if (recordList.Count == 0)
        {
            return recordList;
        }

        var (sampleLongitude, sampleLatitude) = GetSampleAnchorCoordinate(recordList);
        var sampleIndex = 0;
        var sampleLocationIndex = 0;
        foreach (var sampleObservationType in CreateSampleObservationTypes())
        {
            if (sampleIndex >= recordList.Count)
            {
                return recordList;
            }

            var locationName = $"sample-{sampleObservationType.ToString().ToLowerInvariant()}-{sampleLocationIndex + 1}";
            ApplySampleObservationType(
                recordList[sampleIndex],
                sampleObservationType,
                sampleLocationIndex,
                locationName,
                25 + sampleIndex,
                sampleLongitude,
                sampleLatitude);
            sampleIndex++;
            sampleLocationIndex++;
        }

        ApplySampleLocationPriorityCases(
            recordList,
            ref sampleIndex,
            ref sampleLocationIndex,
            sampleLongitude,
            sampleLatitude);

        for (var diffusedIndex = 0;
             diffusedIndex < DiffusedObservationCoordinateUncertaintyInMetersValues.Length && sampleIndex < recordList.Count;
             diffusedIndex++, sampleIndex++)
        {
            ApplySampleDiffusedObservation(
                recordList[sampleIndex],
                diffusedIndex,
                sampleLocationIndex,
                sampleLongitude,
                sampleLatitude);
            sampleLocationIndex++;
        }

        return recordList;
    }

    private static CachedLocationAggregate CreateAggregate(
        List<Observation> observations,
        IReadOnlyDictionary<string, long>? observationCountByLocationId = null,
        bool isSummaryApproximated = false)
    {
        var location = observations.First().Location!;
        var locationIds = observations
            .Select(observation => observation.Location!.LocationId)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        var taxonIds = observations
            .Select(observation => observation.Taxon?.Id)
            .Where(taxonId => taxonId.HasValue)
            .Select(taxonId => taxonId!.Value)
            .Distinct()
            .ToList();

        var observationSummary = CreateLocationObservationSummary(observations);
        var representativeObservationCount = GetRepresentativeObservationCount(observations, observationCountByLocationId);
        var isAggregateSummaryApproximated = isSummaryApproximated &&
                                             representativeObservationCount.HasValue &&
                                             representativeObservationCount.Value > observations.Count;
        if (representativeObservationCount.HasValue)
        {
            ApplyRepresentativeObservationCount(observationSummary, representativeObservationCount.Value);
        }

        return new CachedLocationAggregate
        {
            DistinctLocationId = location.DistinctLocationId,
            Location = location,
            LocationIds = locationIds,
            TaxonIds = taxonIds,
            ObservationSummary = observationSummary,
            IsSummaryApproximated = isAggregateSummaryApproximated
        };
    }

    private static long? GetRepresentativeObservationCount(
        IEnumerable<Observation> observations,
        IReadOnlyDictionary<string, long>? observationCountByLocationId)
    {
        if (observationCountByLocationId == null || observationCountByLocationId.Count == 0)
        {
            return null;
        }

        long observationCount = 0;
        var hasObservationCount = false;
        foreach (var locationId in observations
                     .Select(observation => observation.Location!.LocationId)
                     .Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (observationCountByLocationId.TryGetValue(locationId, out var count))
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

    private static void ApplySampleLocationPriorityCases(
        List<JsonObject> records,
        ref int sampleIndex,
        ref int sampleLocationIndex,
        double sampleLongitude,
        double sampleLatitude)
    {
        foreach (var priorityCase in CreateSampleLocationPriorityCases())
        {
            if (sampleIndex + priorityCase.ObservationTypes.Count > records.Count)
            {
                return;
            }

            var locationIndex = sampleLocationIndex++;
            for (var observationIndex = 0; observationIndex < priorityCase.ObservationTypes.Count; observationIndex++)
            {
                var observationType = priorityCase.ObservationTypes[observationIndex];
                ApplySampleObservationType(
                    records[sampleIndex],
                    observationType,
                    locationIndex,
                    priorityCase.LocationName,
                    priorityCase.CoordinateUncertaintyInMeters,
                    sampleLongitude,
                    sampleLatitude);
                sampleIndex++;
            }
        }
    }

    private static IEnumerable<SampleLocationPriorityCase> CreateSampleLocationPriorityCases()
    {
        yield return new SampleLocationPriorityCase("sample-priority-present-wins", new[] { SampleObservationType.NotPresent, SampleObservationType.NotRecovered, SampleObservationType.Treated, SampleObservationType.Present }, 45);
        yield return new SampleLocationPriorityCase("sample-priority-present-sensitive-all-sensitive", new[] { SampleObservationType.PresentSensitive, SampleObservationType.PresentSensitive }, 50);
        yield return new SampleLocationPriorityCase("sample-priority-present-over-sensitive", new[] { SampleObservationType.PresentSensitive, SampleObservationType.Present }, 55);
        yield return new SampleLocationPriorityCase("sample-priority-treated-wins", new[] { SampleObservationType.NotPresent, SampleObservationType.NotRecovered, SampleObservationType.Treated }, 60);
        yield return new SampleLocationPriorityCase("sample-priority-not-recovered-wins", new[] { SampleObservationType.NotPresent, SampleObservationType.NotRecovered }, 65);
        yield return new SampleLocationPriorityCase("sample-priority-not-present-only", new[] { SampleObservationType.NotPresent, SampleObservationType.NotPresent }, 70);
    }

    private static IEnumerable<SampleObservationType> CreateSampleObservationTypes()
    {
        var sampleObservationTypes = new[]
        {
            SampleObservationType.Present,
            SampleObservationType.PresentSensitive,
            SampleObservationType.Treated,
            SampleObservationType.NotRecovered,
            SampleObservationType.NotPresent
        };

        foreach (var sampleObservationType in sampleObservationTypes)
        {
            for (var i = 0; i < SampleObservationsPerLocationType; i++)
            {
                yield return sampleObservationType;
            }
        }
    }

    private static void ApplySampleObservationType(JsonObject record, SampleObservationType sampleObservationType, int sampleLocationIndex, string sampleLocationName, int coordinateUncertaintyInMeters, double sampleLongitude, double sampleLatitude)
    {
        record["isGeneralized"] = false;
        record["sensitive"] = sampleObservationType == SampleObservationType.PresentSensitive;

        var occurrence = GetOrCreateJsonObject(record, "occurrence");
        occurrence["isPositiveObservation"] = sampleObservationType is SampleObservationType.Present or SampleObservationType.PresentSensitive;
        occurrence["isNotRediscoveredObservation"] = sampleObservationType == SampleObservationType.NotRecovered;
        occurrence["isNeverFoundObservation"] = sampleObservationType == SampleObservationType.NotPresent;

        var eventNode = GetOrCreateJsonObject(record, "event");
        eventNode["startYear"] = 2025;
        eventNode["endYear"] = 2025;

        if (sampleObservationType == SampleObservationType.Treated)
        {
            var artportalenInternal = GetOrCreateJsonObject(record, "artportalenInternal");
            artportalenInternal["invasiveSpeciesTreatment"] = new JsonObject
            {
                ["id"] = 0,
                ["value"] = "Sample treatment"
            };
        }
        else if (record["artportalenInternal"] is JsonObject artportalenInternal)
        {
            artportalenInternal["invasiveSpeciesTreatment"] = null;
        }

        ConfigureSamplePointLocation(record, sampleLocationIndex, sampleLocationName, coordinateUncertaintyInMeters, sampleLongitude, sampleLatitude);
    }

    private static void ApplySampleDiffusedObservation(JsonObject record, int diffusedIndex, int sampleLocationIndex, double sampleLongitude, double sampleLatitude)
    {
        record["isGeneralized"] = true;
        record["sensitive"] = false;

        var occurrence = GetOrCreateJsonObject(record, "occurrence");
        occurrence["isPositiveObservation"] = true;
        occurrence["isNotRediscoveredObservation"] = false;
        occurrence["isNeverFoundObservation"] = false;

        if (record["artportalenInternal"] is JsonObject artportalenInternal)
        {
            artportalenInternal["invasiveSpeciesTreatment"] = null;
        }

        var eventNode = GetOrCreateJsonObject(record, "event");
        eventNode["startYear"] = 2025;
        eventNode["endYear"] = 2025;

        ConfigureSamplePointLocation(
            record,
            sampleLocationIndex,
            $"sample-diffused-{diffusedIndex + 1}",
            DiffusedObservationCoordinateUncertaintyInMetersValues[diffusedIndex],
            sampleLongitude,
            sampleLatitude);
    }

    private static void ConfigureSamplePointLocation(JsonObject record, int sampleIndex, string sampleName, int coordinateUncertaintyInMeters, double sampleLongitude, double sampleLatitude)
    {
        var location = GetOrCreateJsonObject(record, "location");
        var offsetColumn = sampleIndex % 5 - 2;
        var offsetRow = sampleIndex / 5 - 2;

        location["locationId"] = sampleName;
        location["locality"] = sampleName;
        location["type"] = (int)ProcessedLocationType.Point;
        location["decimalLongitude"] = Math.Round(sampleLongitude + offsetColumn * SampleObservationOffsetDegrees, 5);
        location["decimalLatitude"] = Math.Round(sampleLatitude + offsetRow * SampleObservationOffsetDegrees, 5);
        location["coordinateUncertaintyInMeters"] = coordinateUncertaintyInMeters;
        location["sweref99TmX"] = null;
        location["sweref99TmY"] = null;
    }

    private static (double Longitude, double Latitude) GetSampleAnchorCoordinate(IEnumerable<JsonObject> records)
    {
        foreach (var record in records)
        {
            if (record["location"] is not JsonObject location)
            {
                continue;
            }

            var longitude = GetJsonDouble(location, "decimalLongitude");
            var latitude = GetJsonDouble(location, "decimalLatitude");
            if (longitude.HasValue && latitude.HasValue)
            {
                return (longitude.Value, latitude.Value);
            }
        }

        return (15d, 58d);
    }

    private static JsonObject GetOrCreateJsonObject(JsonObject parent, string propertyName)
    {
        if (parent[propertyName] is JsonObject child)
        {
            return child;
        }

        child = new JsonObject();
        parent[propertyName] = child;
        return child;
    }

    private static double? GetJsonDouble(JsonObject parent, string propertyName)
    {
        if (!parent.TryGetPropertyValue(propertyName, out var value) || value == null)
        {
            return null;
        }

        try
        {
            return value.GetValue<double>();
        }
        catch (InvalidOperationException)
        {
            return null;
        }
        catch (FormatException)
        {
            return null;
        }
        catch (InvalidCastException)
        {
            return null;
        }
    }

    internal static string? GetLocationId(SOS.Lib.Models.Processed.Observation.Location? location)
    {
        if (location == null)
        {
            return null;
        }

        return string.IsNullOrWhiteSpace(location.LocationId)
            ? null
            : location.LocationId;
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
