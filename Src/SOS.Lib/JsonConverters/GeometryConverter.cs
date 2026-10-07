using NetTopologySuite.Geometries;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SOS.Lib.JsonConverters;

/// <summary>
///     Json converter for Geometry
/// </summary>
public class GeometryConverter : JsonConverter<Geometry>
{
    private const string InvalidGeometryMessage = "Invalid geometry.";
    private static readonly GeometryFactory GeometryFactory = new();

    public override bool HandleNull => true;

    private void WritePoint(Utf8JsonWriter writer, Coordinate coordinate)
    {
        writer.WriteStartArray();

        writer.WriteNumberValue(coordinate.X);
        writer.WriteNumberValue(coordinate.Y);

        writer.WriteEndArray();
    }

    private void WritePolygon(Utf8JsonWriter writer, Polygon polygon)
    {
        writer.WriteStartArray();

        writer.WriteStartArray();
        foreach (var point in polygon.ExteriorRing.Coordinates)
        {
            WritePoint(writer, point);
        }

        writer.WriteEndArray();

        foreach (var lineRing in polygon.Holes)
        {
            writer.WriteStartArray();
            foreach (var point in lineRing.Coordinates)
            {
                WritePoint(writer, point);
            }

            writer.WriteEndArray();
        }

        writer.WriteEndArray();
    }

    private static Coordinate ReadCoordinate(JsonElement position)
    {
        if (position.ValueKind != JsonValueKind.Array || position.GetArrayLength() < 2)
        {
            throw new JsonException(InvalidGeometryMessage);
        }

        var ordinates = position.EnumerateArray();
        ordinates.MoveNext();
        var x = ordinates.Current.GetDouble();
        ordinates.MoveNext();
        var y = ordinates.Current.GetDouble();
        // Search geometries remain two-dimensional, but all supplied ordinates must be numbers.
        while (ordinates.MoveNext()) ordinates.Current.GetDouble();
        return new Coordinate(x, y);
    }

    private static Coordinate[] ReadCoordinates(JsonElement positions)
    {
        var coordinates = new Coordinate[positions.GetArrayLength()];
        var index = 0;
        foreach (var position in positions.EnumerateArray()) coordinates[index++] = ReadCoordinate(position);
        return coordinates;
    }

    private static Polygon ReadPolygon(JsonElement coordinates)
    {
        var ringCount = coordinates.GetArrayLength();
        if (ringCount == 0) throw new JsonException(InvalidGeometryMessage);

        LinearRing? shell = null;
        var holes = new LinearRing[ringCount - 1];
        var index = 0;
        foreach (var ring in coordinates.EnumerateArray())
        {
            if (ring.GetArrayLength() < 4) throw new JsonException(InvalidGeometryMessage);
            var linearRing = GeometryFactory.CreateLinearRing(ReadCoordinates(ring));
            if (index == 0) shell = linearRing;
            else holes[index - 1] = linearRing;
            index++;
        }
        return GeometryFactory.CreatePolygon(shell, holes);
    }

    public override bool CanConvert(Type type)
    {
        return typeof(Geometry).IsAssignableFrom(type);
    }

    public override void Write(Utf8JsonWriter writer, Geometry value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStartObject();

        var type = value.GeometryType;

        writer.WritePropertyName("coordinates");
        switch (type?.ToLower())
        {
            case "point":
                var point = (Point)value;
                WritePoint(writer, point.Coordinate);
                break;
            case "polygon":
                var polygon = (Polygon)value;
                WritePolygon(writer, polygon);
                break;
            case "multipolygon":
                var muliPolygon = (MultiPolygon)value;
                writer.WriteStartArray();
                foreach (var poly in muliPolygon.Geometries)
                {
                    WritePolygon(writer, (Polygon)poly);
                }
                writer.WriteEndArray();
                break;
            case "linestring":
                var line = (LineString)value;
                writer.WriteStartArray();
                foreach (var c in line.Coordinates)
                {
                    WritePoint(writer, c);
                }
                writer.WriteEndArray();
                break;
            case "multilinestring":
                var multiLine = (MultiLineString)value;
                writer.WriteStartArray();
                foreach (var l in multiLine.Geometries)
                {
                    var ls = (LineString)l;
                    writer.WriteStartArray();
                    foreach (var c in ls.Coordinates)
                    {
                        WritePoint(writer, c);
                    }
                    writer.WriteEndArray();
                }
                writer.WriteEndArray();
                break;
        }

        writer.WriteString("type", type);

        writer.WriteEndObject();
    }

    public override Geometry Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        return ReadGeometry(document.RootElement);
    }

    // Reuse the geographic filter's existing JSON document instead of parsing each geometry again.
    internal static Geometry ReadGeometry(JsonElement element)
    {
        try
        {
            if (element.ValueKind != JsonValueKind.Object) throw new JsonException(InvalidGeometryMessage);
            JsonElement coordinates = default;
            string? type = null;
            foreach (var property in element.EnumerateObject())
            {
                if (property.Name.Equals("coordinates", StringComparison.OrdinalIgnoreCase)) coordinates = property.Value;
                else if (property.Name.Equals("type", StringComparison.OrdinalIgnoreCase)) type = property.Value.GetString();
            }

            switch (type?.ToLowerInvariant())
            {
                case "point":
                    return GeometryFactory.CreatePoint(ReadCoordinate(coordinates));
                case "polygon":
                    return ReadPolygon(coordinates);
                case "multipolygon":
                    var polygons = new Polygon[coordinates.GetArrayLength()];
                    var polygonIndex = 0;
                    foreach (var polygon in coordinates.EnumerateArray()) polygons[polygonIndex++] = ReadPolygon(polygon);
                    return GeometryFactory.CreateMultiPolygon(polygons);
                case "linestring":
                    return GeometryFactory.CreateLineString(ReadCoordinates(coordinates));
                case "multilinestring":
                    var lines = new LineString[coordinates.GetArrayLength()];
                    var lineIndex = 0;
                    foreach (var line in coordinates.EnumerateArray())
                        lines[lineIndex++] = GeometryFactory.CreateLineString(ReadCoordinates(line));
                    return GeometryFactory.CreateMultiLineString(lines);
                default:
                    throw new JsonException(InvalidGeometryMessage);
            }
        }
        catch (JsonException)
        {
            throw;
        }
        catch (Exception exception) when (exception is ArgumentException or
            IndexOutOfRangeException or
            InvalidOperationException or
            NullReferenceException or
            FormatException or
            NotSupportedException)
        {
            throw new JsonException(InvalidGeometryMessage, exception);
        }
    }
}
