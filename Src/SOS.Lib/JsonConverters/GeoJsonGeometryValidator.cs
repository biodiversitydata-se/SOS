using System.Text.Json;

namespace SOS.Lib.JsonConverters;

/// <summary>Checks search geometry JSON before the server constructs a geometry.</summary>
internal static class GeoJsonGeometryValidator
{
    public static bool IsValidStructure(JsonElement geometry)
    {
        if (geometry.ValueKind != JsonValueKind.Object ||
            !TryGetPropertyIgnoreCase(geometry, "type", out var typeElement) || typeElement.ValueKind != JsonValueKind.String ||
            !TryGetPropertyIgnoreCase(geometry, "coordinates", out var coordinates))
        {
            return false;
        }

        return typeElement.GetString()?.ToLowerInvariant() switch
        {
            "polygon" => IsValidPolygonCoordinates(coordinates),
            "multipolygon" => coordinates.ValueKind == JsonValueKind.Array &&
                coordinates.EnumerateArray().All(IsValidPolygonCoordinates),
            _ => true
        };
    }

    private static bool TryGetPropertyIgnoreCase(JsonElement element, string name, out JsonElement value)
    {
        if (element.TryGetProperty(name, out value)) return true;

        // Match the server's geometry reader, which also accepts PascalCase request properties.
        foreach (var property in element.EnumerateObject())
        {
            if (property.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                value = property.Value;
                return true;
            }
        }
        return false;
    }

    private static bool IsValidPolygonCoordinates(JsonElement polygonCoordinates)
    {
        if (polygonCoordinates.ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        return polygonCoordinates.GetArrayLength() > 0 && polygonCoordinates.EnumerateArray().All(IsValidLinearRing);
    }

    private static bool IsValidLinearRing(JsonElement ring)
    {
        if (ring.ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        if (ring.GetArrayLength() < 4) return false;
        JsonElement first = default;
        JsonElement last = default;
        foreach (var position in ring.EnumerateArray())
        {
            if (position.ValueKind != JsonValueKind.Array || position.GetArrayLength() < 2) return false;
            if (first.ValueKind == JsonValueKind.Undefined) first = position;
            last = position;
        }
        return PositionsAreEqual(first, last);
    }

    private static bool PositionsAreEqual(JsonElement first, JsonElement last)
    {
        if (first.GetArrayLength() != last.GetArrayLength()) return false;
        var lastCoordinates = last.EnumerateArray();
        foreach (var coordinate in first.EnumerateArray())
        {
            lastCoordinates.MoveNext();
            if (coordinate.ValueKind != JsonValueKind.Number || lastCoordinates.Current.ValueKind != JsonValueKind.Number ||
                !coordinate.GetDouble().Equals(lastCoordinates.Current.GetDouble())) return false;
        }
        return true;
    }
}
