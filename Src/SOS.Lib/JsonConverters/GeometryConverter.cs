using NetTopologySuite.Geometries;
using System.Buffers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SOS.Lib.JsonConverters;

/// <summary>
///     Json converter for Geometry
/// </summary>
public class GeometryConverter : JsonConverter<Geometry>
{
    private const string InvalidGeometryMessage = "Invalid geometry.";
    private static GeometryFactory geomFactory = new GeometryFactory();

    public override bool HandleNull => true;

    private string ReadCoordinateData(ref Utf8JsonReader reader)
    {
        var result = new StringBuilder();
        var openBrackets = 0;
        var addComma = false;

        // while (openBrackets != 0 || firstLoop)
        do
        {
            if (reader.TokenType == JsonTokenType.StartArray)
            {
                if (addComma)
                {
                    result.Append(',');
                    addComma = false;
                }

                openBrackets++;
            }

            if (reader.TokenType == JsonTokenType.Number && addComma)
            {
                result.Append(',');
            }

            var seq = reader.HasValueSequence
                ? reader.ValueSequence.ToArray()
                : reader.ValueSpan.ToArray();
            result.Append(Encoding.Default.GetString(seq));

            addComma = reader.TokenType == JsonTokenType.Number && !addComma;

            if (reader.TokenType == JsonTokenType.EndArray)
            {
                openBrackets--;
                addComma = true;
            }

            if (openBrackets != 0)
            {
                reader.Read();
            }
        } while (openBrackets != 0);

        return result.ToString();
    }

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

    private Polygon CreatePolygon(GeometryFactory geomFactory, IEnumerable<IEnumerable<double[]>> coordinates)
    {
        var coordinateRings = coordinates?
            .Select(ring => ring?.ToArray())
            .ToArray();
        if (coordinateRings == null ||
            coordinateRings.Length == 0 ||
            coordinateRings.Any(ring => ring == null ||
                ring.Length < 4 ||
                ring.Any(coordinate => coordinate == null || coordinate.Length < 2)))
        {
            throw new JsonException(InvalidGeometryMessage);
        }

        var exteriorRing = geomFactory.CreateLinearRing(coordinateRings.First()
            .Select(p => new Coordinate(p[0], p[1])).ToArray());

        var holes = coordinateRings.Skip(1).Select(h => geomFactory.CreateLinearRing(h
            .Select(p => new Coordinate(p[0], p[1])).ToArray())).ToArray();
        var polygon = geomFactory.CreatePolygon(exteriorRing, holes);
        return polygon;
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
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException(InvalidGeometryMessage);
        }

        var coordinateString = string.Empty;
        var type = string.Empty;

        reader.Read();

        while (reader.TokenType != JsonTokenType.EndObject)
        {
            var property = reader.GetString();
            reader.Read();

            switch (property?.ToLower())
            {
                case "coordinates":
                    coordinateString = ReadCoordinateData(ref reader);
                    break;
                case "type":
                    type = reader.GetString();
                    break;
            }

            reader.Read();
        }
        
        try
        {
            switch (type?.ToLower())
            {
                case "point":
                    var pointCoordinates = JsonSerializer.Deserialize<double[]>(coordinateString);
                    var point = geomFactory.CreatePoint(new Coordinate(pointCoordinates[0], pointCoordinates[1]));
                    return point;
                case "polygon":
                    var polygonCoordinates =
                        JsonSerializer.Deserialize<IEnumerable<IEnumerable<double[]>>>(coordinateString);

                    return CreatePolygon(geomFactory, polygonCoordinates);
                case "multipolygon":
                    var muliPolygonCoordinates =
                        JsonSerializer.Deserialize<IEnumerable<IEnumerable<IEnumerable<double[]>>>>(coordinateString);

                    return geomFactory.CreateMultiPolygon(muliPolygonCoordinates
                        .Select(p => CreatePolygon(geomFactory, p)).ToArray());
                case "linestring":
                    var lineCoords = JsonSerializer.Deserialize<double[][]>(coordinateString);
                    var coords = lineCoords.Select(p => new Coordinate(p[0], p[1])).ToArray();
                    return geomFactory.CreateLineString(coords);
                case "multilinestring":
                    var multiLineCoords = JsonSerializer.Deserialize<IEnumerable<IEnumerable<double[]>>>(coordinateString);
                    var lineStrings = multiLineCoords.Select(lc => geomFactory.CreateLineString(lc.Select(p => new Coordinate(p[0], p[1])).ToArray())).ToArray();
                    return geomFactory.CreateMultiLineString(lineStrings);
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
