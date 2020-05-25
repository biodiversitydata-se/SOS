using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using NetTopologySuite.Geometries;

namespace SOS.Lib.JsonConverters
{
    /// <summary>
    /// Json converter for Geometry
    /// </summary>
    public class GeometryConverter : JsonConverter<Geometry>
    {
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
            if (!coordinates?.Any() ?? true)
            {
                return null;
            }

            var exteriorRing = geomFactory.CreateLinearRing(coordinates.First()
                .Select(p => new Coordinate(p[0], p[1])).ToArray());

            var holes = coordinates.Skip(1).Select(h => geomFactory.CreateLinearRing(h
                .Select(p => new Coordinate(p[0], p[1])).ToArray())).ToArray();
            var polygon = geomFactory.CreatePolygon(exteriorRing, holes);
            return polygon;
        }

        public override bool CanConvert(Type type)
        {
            return typeof(Geometry).IsAssignableFrom(type);
        }
        public override Geometry Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
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
                var property = reader.GetString();
                reader.Read();

                switch (property)
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

            var geomFactory = new GeometryFactory();
            switch (type?.ToLower())
            {
                case "point":
                    var pointCoordinates = JsonSerializer.Deserialize<double[]>(coordinateString);
                    var point = geomFactory.CreatePoint(new Coordinate(pointCoordinates[0], pointCoordinates[1]));
                    return point;
                case "polygon":
                    var polygonCoordinates = JsonSerializer.Deserialize<IEnumerable<IEnumerable<double[]>>>(coordinateString);
                    
                    return CreatePolygon(geomFactory, polygonCoordinates);
                case "multipolygon":
                    var muliPolygonCoordinates = JsonSerializer.Deserialize<IEnumerable<IEnumerable<IEnumerable<double[]>>>>(coordinateString);

                    return geomFactory.CreateMultiPolygon(muliPolygonCoordinates
                        .Select(p => CreatePolygon(geomFactory, p)).ToArray());
            }

            return null;
        }

        public override void Write(Utf8JsonWriter writer, Geometry value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                return;
            };
            writer.WriteStartObject();

            var type = value.GeometryType;
            writer.WriteString("type", type);
            
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
            }

            writer.WriteEndObject();
        }
    }
}
