using NetTopologySuite.Geometries;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Gis;
using SOS.Shared.Api.Dtos;
using SOS.Shared.Api.Dtos.Filter;
using System.Text.Json.Nodes;
using ProcessedLocationType = SOS.Lib.Enums.LocationType;

namespace SOS.Observations.Api.Services.AdaptiveSearch;

public interface IAdaptiveSearchByLocationMapper
{
    AdaptiveSearchResultByLocationDto<JsonObject, JsonObject> CreateResult(
        LatLonBoundingBoxDto boundingBox,
        int totalObservationsCount,
        IEnumerable<JsonObject> records,
        Dictionary<string, Geometry>? polygonLocationGeometries,
        List<(global::SOS.Lib.Models.Shared.Area area, Geometry geometry)> geographicAreas);
}

internal sealed class AdaptiveSearchByLocationMapper : IAdaptiveSearchByLocationMapper
{
    public AdaptiveSearchResultByLocationDto<JsonObject, JsonObject> CreateResult(
        LatLonBoundingBoxDto boundingBox,
        int totalObservationsCount,
        IEnumerable<JsonObject> records,
        Dictionary<string, Geometry>? polygonLocationGeometries,
        List<(global::SOS.Lib.Models.Shared.Area area, Geometry geometry)> geographicAreas)
    {
        const string syntheticLocationPrefix = "synthetic-location-";
        var recordList = records.ToList();
        var syntheticCounter = 0;

        var locations = recordList
            .Select(record =>
            {
                var location = GetLocationNode(record);
                var locationId = location?["locationId"]?.GetValue<string>();
                if (string.IsNullOrWhiteSpace(locationId))
                {
                    locationId = $"{syntheticLocationPrefix}{++syntheticCounter}";
                }

                return new
                {
                    Record = record,
                    Location = location,
                    LocationId = locationId
                };
            })
            .GroupBy(item => item.LocationId, StringComparer.OrdinalIgnoreCase)
            .Select(group =>
            {
                var observations = group
                    .Select(item => CreateObservationWithoutLocation(item.Record))
                    .ToList();

                var geometry = GetLocationGeometry(group.First().Location, group.Key, polygonLocationGeometries);
                return new AdaptiveSearchLocationDto<JsonObject, JsonObject>
                {
                    Location = CreateLocationNode(group.First().Location, group.Key),
                    Geometry = geometry,
                    LocationCount = 1,                    
                    ObservationCount = observations.Count,
                    TaxaCount = group
                        .Select(item => GetTaxonId(item.Record))
                        .Where(taxonId => taxonId.HasValue)
                        .Select(taxonId => taxonId!.Value)
                        .Distinct()
                        .Count(),
                    Observations = observations
                };
            })
            .ToList();

        return AdaptiveSearchResultByLocationDto<JsonObject, JsonObject>.CreateLocationsResult(
            boundingBox,
            totalObservationsCount,
            recordList.Count,
            locations,
            geographicAreas);
    }

    private static JsonObject? GetLocationNode(JsonObject record)
    {
        if (record.TryGetPropertyValue("location", out var locationNode) && locationNode is JsonObject locationObject)
        {
            return locationObject;
        }

        return null;
    }

    private static int? GetTaxonId(JsonObject record)
    {
        if (record.TryGetPropertyValue("taxon", out var taxonNode) && taxonNode is JsonObject taxonObject)
        {
            return taxonObject["id"]?.GetValue<int>();
        }

        return null;
    }

    private static JsonObject CreateObservationWithoutLocation(JsonObject record)
    {
        var observation = record.DeepClone().AsObject();
        observation.Remove("location");
        return observation;
    }

    private static JsonObject CreateLocationNode(JsonObject? locationNode, string locationId)
    {
        var location = locationNode?.DeepClone().AsObject() ?? new JsonObject();
        location["locationId"] = locationId;
        return location;
    }

    private static Geometry? GetLocationGeometry(
        JsonObject? locationNode,
        string locationId,
        Dictionary<string, Geometry>? polygonLocationGeometries)
    {
        var locationType = GetLocationType(locationNode);
        if (locationType == ProcessedLocationType.Polygon &&
            polygonLocationGeometries?.TryGetValue(locationId, out var polygonGeometry) == true)
        {
            return polygonGeometry;
        }

        var latitude = GetDouble(locationNode?["decimalLatitude"]);
        var longitude = GetDouble(locationNode?["decimalLongitude"]);
        if (!latitude.HasValue || !longitude.HasValue)
        {
            return null;
        }

        return new Point(longitude.Value, latitude.Value)
        {
            SRID = CoordinateSys.WGS84.Srid()
        };
    }

    private static ProcessedLocationType? GetLocationType(JsonObject? locationNode)
    {
        if (locationNode?["type"] is not JsonValue typeNode)
        {
            return null;
        }

        if (typeNode.TryGetValue<int>(out var typeValue))
        {
            return (ProcessedLocationType)typeValue;
        }

        return null;
    }

    private static double? GetDouble(JsonNode? node)
    {
        if (node is JsonValue value && value.TryGetValue<double>(out var doubleValue))
        {
            return doubleValue;
        }

        return null;
    }
}
