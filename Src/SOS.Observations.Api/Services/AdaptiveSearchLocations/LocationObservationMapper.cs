using SOS.Lib.Models.Processed.Observation;
using System.Text.Json.Nodes;
using LocationType = SOS.Lib.Enums.LocationType;

namespace SOS.Observations.Api.Services.AdaptiveSearchLocations;

/// <summary>
/// Reads the projection from GetLocationFeatureOutputFields without serializing the records again.
/// </summary>
internal static class LocationObservationMapper
{
    public static Observation Map(JsonObject record)
    {
        var location = Property(record, "location");
        var municipality = Property(location, "municipality");
        var occurrence = Property(record, "occurrence");
        var observationEvent = Property(record, "event");
        var taxon = Property(record, "taxon");
        var attributes = Property(taxon, "attributes");
        var artportalenInternal = Property(record, "artportalenInternal");
        var treatment = Property(artportalenInternal, "invasiveSpeciesTreatment");

        return new Observation
        {
            IsGeneralized = Value<bool>(record, "isGeneralized") ?? false,
            Sensitive = Value<bool>(record, "sensitive") ?? false,
            Location = location == null ? null : new Location
            {
                LocationId = Property(location, "locationId")?.GetValue<string>(),
                Type = (LocationType)(Value<int>(location, "type") ?? 0),
                DecimalLatitude = Value<double>(location, "decimalLatitude"),
                DecimalLongitude = Value<double>(location, "decimalLongitude"),
                CoordinateUncertaintyInMeters = Value<int>(location, "coordinateUncertaintyInMeters"),
                Sweref99TmX = Value<double>(location, "sweref99TmX"),
                Sweref99TmY = Value<double>(location, "sweref99TmY"),
                Locality = Property(location, "locality")?.GetValue<string>(),
                Municipality = municipality == null ? null : new Area
                {
                    FeatureId = Property(municipality, "featureId")?.GetValue<string>(),
                    Name = Property(municipality, "name")?.GetValue<string>()
                }
            },
            Occurrence = occurrence == null ? null : new Occurrence
            {
                OccurrenceId = Property(occurrence, "occurrenceId")?.GetValue<string>(),
                IsPositiveObservation = Value<bool>(occurrence, "isPositiveObservation") ?? false,
                IsNotRediscoveredObservation = Value<bool>(occurrence, "isNotRediscoveredObservation") ?? false,
                IsNeverFoundObservation = Value<bool>(occurrence, "isNeverFoundObservation") ?? false
            },
            Event = observationEvent == null ? null : new Event
            {
                StartYear = Value<int>(observationEvent, "startYear"),
                EndYear = Value<int>(observationEvent, "endYear")
            },
            Taxon = taxon == null ? null : new Taxon
            {
                Id = Value<int>(taxon, "id") ?? 0,
                Attributes = attributes == null && taxon.AsObject().Any(p =>
                    string.Equals(p.Key, "attributes", StringComparison.OrdinalIgnoreCase)) ? null : new TaxonAttributes
                {
                    IsRedlisted = Value<bool>(attributes, "isRedlisted") ?? false,
                    IsInvasiveInSweden = Value<bool>(attributes, "isInvasiveInSweden") ?? false
                }
            },
            ArtportalenInternal = artportalenInternal == null ? null : new ArtportalenInternal
            {
                InvasiveSpeciesTreatment = treatment == null ? null : new VocabularyValue
                {
                    Id = Value<int>(treatment, "id") ?? 0
                }
            }
        };
    }

    private static T? Value<T>(JsonNode? node, string name) where T : struct
        => Property(node, name)?.GetValue<T>();

    private static JsonNode? Property(JsonNode? node, string name)
    {
        if (node is not JsonObject obj) return null;
        if (obj.TryGetPropertyValue(name, out var value)) return value;

        // Match the case-insensitive behavior of the previous deserializer.
        foreach (var property in obj)
        {
            if (string.Equals(property.Key, name, StringComparison.OrdinalIgnoreCase))
                return property.Value;
        }
        return null;
    }
}
