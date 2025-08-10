using SOS.Status.Web.Client.Dtos.SosObsApi;
using System.Buffers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SOS.Status.Web.Client.JsonConverters;

public class GeoJsonConverter : JsonConverter<GeoJsonGeometry>
{
    private bool _useCapitalLetter; // Länsstyrelsen fix. Expects capital letter.
    public GeoJsonConverter(bool useCapitalLetter)
    {
        this._useCapitalLetter = useCapitalLetter;
    }
    public GeoJsonConverter()
    {
    }

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

    private void WritePoint(Utf8JsonWriter writer, GeoJson2DCoordinates coordinate)
    {
        writer.WriteStartArray();

        writer.WriteNumberValue(coordinate.X);
        writer.WriteNumberValue(coordinate.Y);

        writer.WriteEndArray();
    }

    private void WritePolygon(Utf8JsonWriter writer, GeoJsonPolygonCoordinates coordinates)
    {
        writer.WriteStartArray();

        writer.WriteStartArray();
        foreach (var point in coordinates.Exterior.Positions)
        {
            WritePoint(writer, point);
        }

        writer.WriteEndArray();

        foreach (var lineRing in coordinates.Holes)
        {
            writer.WriteStartArray();
            foreach (var point in lineRing.Positions)
            {
                WritePoint(writer, point);
            }

            writer.WriteEndArray();
        }

        writer.WriteEndArray();
    }

    private GeoJsonPolygon CreatePolygon(IEnumerable<IEnumerable<double[]>> coordinates)
    {
        if (!coordinates?.Any() ?? true)
        {
            return null;
        }

        var exteriorRing = coordinates.First()
            .Select(p => new GeoJson2DCoordinates(p[0], p[1])).ToArray();
        var holes = coordinates.Skip(1).Select(h => new GeoJsonLinearRingCoordinates(h
            .Select(p => new GeoJson2DCoordinates(p[0], p[1])).ToArray())).ToArray();
        var polygon = new GeoJsonPolygon(new GeoJsonPolygonCoordinates(
                        new GeoJsonLinearRingCoordinates(
                            exteriorRing
                        ),
                        holes
                    )
                );
        return polygon;
    }

    public override bool CanConvert(Type type)
    {
        return typeof(GeoJsonGeometry).IsAssignableFrom(type);
    }

    public override GeoJsonGeometry Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        var coordinateString = string.Empty;
        var type = string.Empty;

        reader.Read();

        while (reader.TokenType != JsonTokenType.EndObject)
        {
            if (reader.TokenType.Equals(JsonTokenType.PropertyName))
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
        }
        switch (type?.ToLower())
        {
            case "point":
                var pointCoordinates = JsonSerializer.Deserialize<double[]>(coordinateString);
                var point = new GeoJsonPoint(new GeoJson2DCoordinates(pointCoordinates[0], pointCoordinates[1]));
                return point;
            case "polygon":
                var polygonCoordinates =
                    JsonSerializer.Deserialize<IEnumerable<IEnumerable<double[]>>>(coordinateString);

                return CreatePolygon(polygonCoordinates);
            case "multipolygon":
                var sourceMultiPolygonCoordinates =
                    JsonSerializer.Deserialize<IEnumerable<IEnumerable<IEnumerable<double[]>>>>(coordinateString);

                var multiPolygonCoordinates = new List<GeoJsonPolygonCoordinates>();
                foreach (var polyCoordinates in sourceMultiPolygonCoordinates)
                {
                    var partPolygon = CreatePolygon(polyCoordinates);
                    multiPolygonCoordinates.Add(partPolygon.Coordinates);
                }

                return new GeoJsonMultiPolygon(new GeoJsonMultiPolygonCoordinates(multiPolygonCoordinates));
        }

        return null;
    }

    public override void Write(Utf8JsonWriter writer, GeoJsonGeometry value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            return;
        }

        ;
        writer.WriteStartObject();

        writer.WritePropertyName(_useCapitalLetter ? "Coordinates" : "coordinates");
        switch (value.Type)
        {
            case GeoJsonObjectType.Point:
                var point = (GeoJsonPoint)value;
                WritePoint(writer, point.Coordinates);
                break;
            case GeoJsonObjectType.Polygon:
                var polygon = (GeoJsonPolygon)value;

                WritePolygon(writer, polygon.Coordinates);
                break;
            case GeoJsonObjectType.MultiPolygon:
                var muliPolygon = (GeoJsonMultiPolygon)value;
                writer.WriteStartArray();
                foreach (var poly in muliPolygon.Coordinates.Polygons)
                {
                    WritePolygon(writer, poly);
                }

                writer.WriteEndArray();
                break;
        }

        writer.WriteString(_useCapitalLetter ? "Type" : "type", value.Type.ToString());

        writer.WriteEndObject();
    }
}