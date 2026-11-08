using System.Text.Json.Nodes;
using ProcessedLocationType = SOS.Lib.Enums.LocationType;

namespace SOS.Observations.Api.Services.AdaptiveSearchLocations;

/// <summary>
/// Rewrites search results into a fixed set of sample observations, used to debug map symbols
/// (createSampleObservations=true). Each sample type is placed at its own location near the first result.
/// </summary>
internal static class AdaptiveSearchSampleObservationGenerator
{
    private const int SampleObservationsPerLocationType = 3;
    private const double SampleObservationOffsetDegrees = 0.002d;
    private const int SampleYear = 2025;
    private const int SamplesPerRow = 5;
    private static readonly (double Longitude, double Latitude) DefaultAnchorCoordinate = (15d, 58d);
    private static readonly int[] DiffusedObservationCoordinateUncertaintyInMetersValues = { 100, 500, 1000 };

    private static readonly SampleObservationType[] SampleObservationTypes =
    {
        SampleObservationType.Present,
        SampleObservationType.PresentSensitive,
        SampleObservationType.Treated,
        SampleObservationType.NotRecovered,
        SampleObservationType.NotPresent
    };

    /// <summary>
    /// Locations with several observations, used to verify which presentation status wins.
    /// </summary>
    private static readonly SampleLocationPriorityCase[] SampleLocationPriorityCases =
    {
        new("sample-priority-present-wins", new[] { SampleObservationType.NotPresent, SampleObservationType.NotRecovered, SampleObservationType.Treated, SampleObservationType.Present }, 45),
        new("sample-priority-present-sensitive-all-sensitive", new[] { SampleObservationType.PresentSensitive, SampleObservationType.PresentSensitive }, 50),
        new("sample-priority-present-over-sensitive", new[] { SampleObservationType.PresentSensitive, SampleObservationType.Present }, 55),
        new("sample-priority-treated-wins", new[] { SampleObservationType.NotPresent, SampleObservationType.NotRecovered, SampleObservationType.Treated }, 60),
        new("sample-priority-not-recovered-wins", new[] { SampleObservationType.NotPresent, SampleObservationType.NotRecovered }, 65),
        new("sample-priority-not-present-only", new[] { SampleObservationType.NotPresent, SampleObservationType.NotPresent }, 70)
    };

    /// <summary>
    /// Returns modified copies of the records; the input records are not changed.
    /// Records beyond the available sample cases are returned unchanged.
    /// </summary>
    public static List<JsonObject> Create(IEnumerable<JsonObject>? records)
    {
        var recordList = records?
            .Select(record => record.DeepClone().AsObject())
            .ToList() ?? new List<JsonObject>();
        if (recordList.Count == 0)
        {
            return recordList;
        }

        var (anchorLongitude, anchorLatitude) = GetAnchorCoordinate(recordList);
        var sampleIndex = 0;
        var sampleLocationIndex = 0;

        foreach (var sampleObservationType in SampleObservationTypes.SelectMany(type => Enumerable.Repeat(type, SampleObservationsPerLocationType)))
        {
            if (sampleIndex >= recordList.Count)
            {
                return recordList;
            }

            var locationName = $"sample-{sampleObservationType.ToString().ToLowerInvariant()}-{sampleLocationIndex + 1}";
            ApplySampleObservationType(recordList[sampleIndex], sampleObservationType, sampleLocationIndex, locationName,
                25 + sampleIndex, anchorLongitude, anchorLatitude);
            sampleIndex++;
            sampleLocationIndex++;
        }

        foreach (var priorityCase in SampleLocationPriorityCases)
        {
            if (sampleIndex + priorityCase.ObservationTypes.Count > recordList.Count)
            {
                break;
            }

            var locationIndex = sampleLocationIndex++;
            foreach (var observationType in priorityCase.ObservationTypes)
            {
                ApplySampleObservationType(recordList[sampleIndex], observationType, locationIndex, priorityCase.LocationName,
                    priorityCase.CoordinateUncertaintyInMeters, anchorLongitude, anchorLatitude);
                sampleIndex++;
            }
        }

        for (var diffusedIndex = 0;
             diffusedIndex < DiffusedObservationCoordinateUncertaintyInMetersValues.Length && sampleIndex < recordList.Count;
             diffusedIndex++, sampleIndex++)
        {
            ApplySampleDiffusedObservation(recordList[sampleIndex], diffusedIndex, sampleLocationIndex, anchorLongitude, anchorLatitude);
            sampleLocationIndex++;
        }

        return recordList;
    }

    private static void ApplySampleObservationType(JsonObject record, SampleObservationType sampleObservationType, int sampleLocationIndex,
        string sampleLocationName, int coordinateUncertaintyInMeters, double anchorLongitude, double anchorLatitude)
    {
        record["isGeneralized"] = false;
        record["sensitive"] = sampleObservationType == SampleObservationType.PresentSensitive;

        var occurrence = GetOrCreateJsonObject(record, "occurrence");
        occurrence["isPositiveObservation"] = sampleObservationType is SampleObservationType.Present or SampleObservationType.PresentSensitive;
        occurrence["isNotRediscoveredObservation"] = sampleObservationType == SampleObservationType.NotRecovered;
        occurrence["isNeverFoundObservation"] = sampleObservationType == SampleObservationType.NotPresent;

        SetSampleYear(record);

        if (sampleObservationType == SampleObservationType.Treated)
        {
            var artportalenInternal = GetOrCreateJsonObject(record, "artportalenInternal");
            artportalenInternal["invasiveSpeciesTreatment"] = new JsonObject
            {
                ["id"] = 0,
                ["value"] = "Sample treatment"
            };
        }
        else
        {
            ClearTreatment(record);
        }

        ConfigureSamplePointLocation(record, sampleLocationIndex, sampleLocationName, coordinateUncertaintyInMeters, anchorLongitude, anchorLatitude);
    }

    private static void ApplySampleDiffusedObservation(JsonObject record, int diffusedIndex, int sampleLocationIndex,
        double anchorLongitude, double anchorLatitude)
    {
        record["isGeneralized"] = true;
        record["sensitive"] = false;

        var occurrence = GetOrCreateJsonObject(record, "occurrence");
        occurrence["isPositiveObservation"] = true;
        occurrence["isNotRediscoveredObservation"] = false;
        occurrence["isNeverFoundObservation"] = false;

        ClearTreatment(record);
        SetSampleYear(record);

        ConfigureSamplePointLocation(
            record,
            sampleLocationIndex,
            $"sample-diffused-{diffusedIndex + 1}",
            DiffusedObservationCoordinateUncertaintyInMetersValues[diffusedIndex],
            anchorLongitude,
            anchorLatitude);
    }

    private static void SetSampleYear(JsonObject record)
    {
        var eventNode = GetOrCreateJsonObject(record, "event");
        eventNode["startYear"] = SampleYear;
        eventNode["endYear"] = SampleYear;
    }

    private static void ClearTreatment(JsonObject record)
    {
        if (record["artportalenInternal"] is JsonObject artportalenInternal)
        {
            artportalenInternal["invasiveSpeciesTreatment"] = null;
        }
    }

    /// <summary>
    /// Places sample locations in a grid of <see cref="SamplesPerRow"/> columns around the anchor coordinate.
    /// </summary>
    private static void ConfigureSamplePointLocation(JsonObject record, int sampleIndex, string sampleName,
        int coordinateUncertaintyInMeters, double anchorLongitude, double anchorLatitude)
    {
        var location = GetOrCreateJsonObject(record, "location");
        var offsetColumn = sampleIndex % SamplesPerRow - 2;
        var offsetRow = sampleIndex / SamplesPerRow - 2;

        location["locationId"] = sampleName;
        location["locality"] = sampleName;
        location["type"] = (int)ProcessedLocationType.Point;
        location["decimalLongitude"] = Math.Round(anchorLongitude + offsetColumn * SampleObservationOffsetDegrees, 5);
        location["decimalLatitude"] = Math.Round(anchorLatitude + offsetRow * SampleObservationOffsetDegrees, 5);
        location["coordinateUncertaintyInMeters"] = coordinateUncertaintyInMeters;
        location["sweref99TmX"] = null;
        location["sweref99TmY"] = null;
    }

    private static (double Longitude, double Latitude) GetAnchorCoordinate(IEnumerable<JsonObject> records)
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

        return DefaultAnchorCoordinate;
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
        if (!parent.TryGetPropertyValue(propertyName, out var value) || value is not JsonValue jsonValue)
        {
            return null;
        }

        return jsonValue.TryGetValue<double>(out var result) ? result : null;
    }

    private enum SampleObservationType
    {
        Present,
        PresentSensitive,
        Treated,
        NotRecovered,
        NotPresent
    }

    private sealed record SampleLocationPriorityCase(
        string LocationName,
        IReadOnlyList<SampleObservationType> ObservationTypes,
        int CoordinateUncertaintyInMeters);
}
