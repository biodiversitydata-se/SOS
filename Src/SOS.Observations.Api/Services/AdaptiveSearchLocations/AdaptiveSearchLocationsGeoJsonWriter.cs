using SOS.Api.Contracts.Common.Geo;
using Microsoft.AspNetCore.Http;
using SOS.Api.Contracts.Observations.Responses;
using NetTopologySuite.Features;
using SOS.Lib.Enums;
using SOS.Lib.Gis;
using SOS.Lib.Models.Gis;
using SOS.Lib.Models.Search.Result;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace SOS.Observations.Api.Services.AdaptiveSearchLocations;

internal static class AdaptiveSearchLocationsGeoJsonWriter
{
    private const int ClusterHashByteLength = 8;

    private static readonly JavaScriptEncoder JsonEncoder =
        JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Latin1Supplement); //JavaScriptEncoder.UnsafeRelaxedJsonEscaping;

    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        AllowTrailingCommas = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        Encoder = JsonEncoder,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters =
        {
            new JsonStringEnumConverter(),
            new NetTopologySuite.IO.Converters.GeoJsonConverterFactory()
        }
    };

    private static readonly JsonWriterOptions JsonWriterOptions = new JsonWriterOptions
    {
        SkipValidation = true,
        Encoder = JsonEncoder
    };

    public static async Task WriteFeatureCollectionAsync(
        IEnumerable<LocationFeature> locationFeatures,
        LatLonBoundingBoxDto? bbox,
        HttpResponse response,
        int? cutoffYear = null,
        List<Feature>? geographicAreas = null,
        CancellationToken cancellationToken = default,
        IReadOnlyList<FilterAreaReferenceDto>? filterAreaReferences = null)
    {
        await using var jsonWriter = new Utf8JsonWriter(response.BodyWriter, JsonWriterOptions);
        WriteFeatureCollectionStart(jsonWriter, bbox, filterAreaReferences);
        WriteGeographicAreaFeatures(jsonWriter, geographicAreas, cancellationToken);

        // Emit drawing order: surfaces (largest first), lines, then points and clusters.
        // LINQ evaluates each area key once; equal keys retain their original order.
        var orderedFeatures = locationFeatures
            .Where(feature => feature.Geometry != null)
            .OrderByDescending(feature => feature.Geometry!.Dimension)
            .ThenByDescending(feature => feature.Geometry!.Area);
        foreach (var feature in orderedFeatures)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var properties = LocationFeatureResponseMapper.ToProperties(feature, cutoffYear: cutoffYear);
            var featureId = GetFeatureId(feature, properties.Locations.Ids);
            if (string.IsNullOrWhiteSpace(featureId))
            {
                continue;
            }

            jsonWriter.WriteStartObject();
            jsonWriter.WriteString("type", "Feature");
            jsonWriter.WriteString("id", featureId);
            jsonWriter.WritePropertyName("geometry");
            JsonSerializer.Serialize(jsonWriter, feature.Geometry, JsonSerializerOptions);
            jsonWriter.WritePropertyName("properties");
            JsonSerializer.Serialize(jsonWriter, properties, JsonSerializerOptions);
            jsonWriter.WriteEndObject();
        }

        await WriteFeatureCollectionEndAsync(jsonWriter, response, cancellationToken);
    }

    public static async Task WriteGeogridFeatureCollectionAsync(
        GeoGridTileResult gridResult,
        LatLonBoundingBoxDto? bbox,
        HttpResponse response,
        List<Feature>? geographicAreas = null,
        CancellationToken cancellationToken = default,
        IReadOnlyList<FilterAreaReferenceDto>? filterAreaReferences = null)
    {
        await using var jsonWriter = new Utf8JsonWriter(response.BodyWriter, JsonWriterOptions);
        WriteFeatureCollectionStart(jsonWriter, bbox, filterAreaReferences);

        foreach (var gridCellTile in gridResult.GridCellTiles ?? Enumerable.Empty<GridCellTile>())
        {
            cancellationToken.ThrowIfCancellationRequested();
            WriteGeographicAreaFeature(jsonWriter, gridCellTile.GetFeature(CoordinateSys.WGS84));
        }

        WriteGeographicAreaFeatures(jsonWriter, geographicAreas, cancellationToken);
        await WriteFeatureCollectionEndAsync(jsonWriter, response, cancellationToken);
    }

    private static void WriteFeatureCollectionStart(
        Utf8JsonWriter jsonWriter,
        LatLonBoundingBoxDto? bbox,
        IReadOnlyList<FilterAreaReferenceDto>? filterAreaReferences)
    {
        jsonWriter.WriteStartObject();
        jsonWriter.WriteString("type", "FeatureCollection");
        WriteGeoJsonBbox(jsonWriter, bbox);
        jsonWriter.WritePropertyName("filterAreaReferences");
        JsonSerializer.Serialize(jsonWriter, filterAreaReferences ?? Array.Empty<FilterAreaReferenceDto>(), JsonSerializerOptions);
        jsonWriter.WritePropertyName("features");
        jsonWriter.WriteStartArray();
    }

    private static async Task WriteFeatureCollectionEndAsync(
        Utf8JsonWriter jsonWriter,
        HttpResponse response,
        CancellationToken cancellationToken)
    {
        jsonWriter.WriteEndArray();
        jsonWriter.WriteEndObject();
        await jsonWriter.FlushAsync(cancellationToken);
        await response.BodyWriter.FlushAsync(cancellationToken);
    }

    private static void WriteGeographicAreaFeatures(
        Utf8JsonWriter jsonWriter,
        List<Feature>? geographicAreas,
        CancellationToken cancellationToken)
    {
        if (geographicAreas == null)
        {
            return;
        }

        foreach (var geographicArea in geographicAreas)
        {
            cancellationToken.ThrowIfCancellationRequested();
            WriteGeographicAreaFeature(jsonWriter, geographicArea);
        }
    }

    private static void WriteGeographicAreaFeature(Utf8JsonWriter jsonWriter, Feature feature)
    {
        if (feature?.Geometry == null)
        {
            return;
        }

        jsonWriter.WriteStartObject();
        jsonWriter.WriteString("type", "Feature");
        jsonWriter.WritePropertyName("geometry");
        JsonSerializer.Serialize(jsonWriter, feature.Geometry, JsonSerializerOptions);
        jsonWriter.WritePropertyName("properties");
        jsonWriter.WriteStartObject();

        if (feature.Attributes != null)
        {
            foreach (var name in feature.Attributes.GetNames())
            {
                jsonWriter.WritePropertyName(name);
                WriteAttributeValue(jsonWriter, feature.Attributes[name]);
            }
        }

        jsonWriter.WriteEndObject();
        jsonWriter.WriteEndObject();
    }

    private static void WriteAttributeValue(Utf8JsonWriter jsonWriter, object? value)
    {
        switch (value)
        {
            case null:
                jsonWriter.WriteNullValue();
                break;
            case string stringValue:
                jsonWriter.WriteStringValue(stringValue);
                break;
            case int intValue:
                jsonWriter.WriteNumberValue(intValue);
                break;
            case long longValue:
                jsonWriter.WriteNumberValue(longValue);
                break;
            case double doubleValue:
                jsonWriter.WriteNumberValue(doubleValue);
                break;
            case bool boolValue:
                jsonWriter.WriteBooleanValue(boolValue);
                break;
            default:
                JsonSerializer.Serialize(jsonWriter, value, JsonSerializerOptions);
                break;
        }
    }

    private static string? GetFeatureId(LocationFeature feature, IEnumerable<string> locationIds)
    {
        if (!feature.IsCluster) return feature.Location?.DistinctLocationId;
        var ids = locationIds
            .Select(id => id.ToUpperInvariant()).Distinct(StringComparer.Ordinal).OrderBy(id => id, StringComparer.Ordinal).ToArray();
        var bytes = JsonSerializer.SerializeToUtf8Bytes(ids);
        var hash = System.Security.Cryptography.SHA256.HashData(bytes);
        return "cluster-" + Convert.ToHexString(hash.AsSpan(0, ClusterHashByteLength)).ToLowerInvariant();
    }

    private static void WriteGeoJsonBbox(Utf8JsonWriter jsonWriter, LatLonBoundingBoxDto? bbox)
    {
        if (bbox?.TopLeft == null || bbox.BottomRight == null)
        {
            return;
        }

        jsonWriter.WritePropertyName("bbox");
        jsonWriter.WriteStartArray();
        jsonWriter.WriteNumberValue(bbox.TopLeft.Longitude);
        jsonWriter.WriteNumberValue(bbox.BottomRight.Latitude);
        jsonWriter.WriteNumberValue(bbox.BottomRight.Longitude);
        jsonWriter.WriteNumberValue(bbox.TopLeft.Latitude);
        jsonWriter.WriteEndArray();
    }
}
