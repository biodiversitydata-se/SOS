using NetTopologySuite.Geometries;
using Microsoft.AspNetCore.Http;
using NetTopologySuite.Features;
using SOS.Lib.Enums;
using SOS.Lib.Gis;
using SOS.Lib.Models.Gis;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace SOS.Observations.Api.Services.AdaptiveSearchLocations;

internal static class LocationClusterItemGeoJsonWriter
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        AllowTrailingCommas = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Latin1Supplement),
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters =
        {
            new JsonStringEnumConverter(),
            new NetTopologySuite.IO.Converters.GeoJsonConverterFactory()
        }
    };

    public static async Task WriteFeatureCollectionAsync(
        IEnumerable<LocationClusterItem> locationClusterItems,
        LatLonBoundingBox? bbox,
        HttpResponse response,
        int? cutoffYear = null,
        List<Feature>? geographicAreas = null)
    {
        await using var jsonWriter = new Utf8JsonWriter(response.BodyWriter, new JsonWriterOptions
        {
            SkipValidation = true
        });

        jsonWriter.WriteStartObject();
        jsonWriter.WriteString("type", "FeatureCollection");
        jsonWriter.WriteString("crs", "EPSG:4326");
        WriteGeoJsonBbox(jsonWriter, bbox);
        jsonWriter.WritePropertyName("features");
        jsonWriter.WriteStartArray();

        if (geographicAreas != null)
        {
            foreach (var geographicArea in geographicAreas)
            {
                WriteGeographicAreaFeature(jsonWriter, geographicArea);
            }
        }

        var featureIndex = 0;
        foreach (var locationClusterItem in locationClusterItems)
        {
            if (locationClusterItem.Geometry == null)
            {
                continue;
            }

            var featureId = GetFeatureId(locationClusterItem, ++featureIndex);
            var dto = LocationClusterItemDto.FromLocationClusterItem(locationClusterItem, featureId, cutoffYear);

            jsonWriter.WriteStartObject();
            jsonWriter.WriteString("type", "Feature");
            jsonWriter.WriteString("id", featureId);
            jsonWriter.WritePropertyName("geometry");
            JsonSerializer.Serialize(jsonWriter, locationClusterItem.Geometry, JsonSerializerOptions);
            jsonWriter.WritePropertyName("properties");
            JsonSerializer.Serialize(jsonWriter, dto, JsonSerializerOptions);
            jsonWriter.WriteEndObject();
        }

        jsonWriter.WriteEndArray();
        jsonWriter.WriteEndObject();
        await jsonWriter.FlushAsync();
        await response.BodyWriter.FlushAsync();
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

    private static string GetFeatureId(LocationClusterItem locationClusterItem, int featureIndex)
    {
        if (locationClusterItem.IsCluster)
        {
            return $"cluster-{featureIndex}";
        }

        return AdaptiveSearchLocationAggregator.GetLocationId(locationClusterItem.Location) ??
               AdaptiveSearchLocationAggregator.GetLocationId(locationClusterItem.Locations.FirstOrDefault()) ??
               $"synthetic-location-{featureIndex}";
    }

    private static void WriteGeoJsonBbox(Utf8JsonWriter jsonWriter, LatLonBoundingBox? bbox)
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
