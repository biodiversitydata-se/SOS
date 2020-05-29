using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Nest;

namespace SOS.Lib.JsonConverters
{
    /// <summary>
    ///     Json converter for IGeoShape
    /// </summary>
    public class GeoShapeConverter : JsonConverter<IGeoShape>
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

        private void WritePoint(Utf8JsonWriter writer, GeoCoordinate coordinate)
        {
            writer.WriteStartArray();

            writer.WriteNumberValue(coordinate.Longitude);
            writer.WriteNumberValue(coordinate.Latitude);

            writer.WriteEndArray();
        }

        private void WritePolygon(Utf8JsonWriter writer, IEnumerable<IEnumerable<GeoCoordinate>> coordinates)
        {
            writer.WriteStartArray();

            foreach (var linestring in coordinates)
            {
                writer.WriteStartArray();
                foreach (var point in linestring)
                {
                    WritePoint(writer, point);
                }

                writer.WriteEndArray();
            }

            writer.WriteEndArray();
        }

        public override bool CanConvert(Type type)
        {
            return typeof(IGeoShape).IsAssignableFrom(type);
        }

        public override IGeoShape Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
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

            switch (type?.ToLower())
            {
                case "point":
                    var pointCoordinates = JsonSerializer.Deserialize<double[]>(coordinateString);
                    return new PointGeoShape(new GeoCoordinate(pointCoordinates[1], pointCoordinates[0]));
                case "polygon":
                    var polygonCoordinates =
                        JsonSerializer.Deserialize<IEnumerable<IEnumerable<double[]>>>(coordinateString);
                    return new PolygonGeoShape(polygonCoordinates
                        .Select(ls => ls
                            .Select(pnt => new GeoCoordinate(pnt[1], pnt[0]))));
                case "multipolygon":
                    var muliPolygonCoordinates =
                        JsonSerializer.Deserialize<IEnumerable<IEnumerable<IEnumerable<double[]>>>>(coordinateString);
                    return new MultiPolygonGeoShape(muliPolygonCoordinates
                        .Select(poly => poly
                            .Select(ls => ls
                                .Select(pnt => new GeoCoordinate(pnt[1], pnt[0])))));
            }

            return null;
        }

        public override void Write(Utf8JsonWriter writer, IGeoShape value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                return;
            }

            ;
            writer.WriteStartObject();

            var type = value.Type;

            writer.WritePropertyName("coordinates");
            switch (type?.ToLower())
            {
                case "point":
                    var point = (PointGeoShape) value;
                    WritePoint(writer, point.Coordinates);
                    break;
                case "polygon":
                    var polygon = (PolygonGeoShape) value;
                    WritePolygon(writer, polygon.Coordinates);
                    break;
                case "multipolygon":
                    var muliPolygon = (MultiPolygonGeoShape) value;
                    writer.WriteStartArray();
                    foreach (var poly in muliPolygon.Coordinates)
                    {
                        WritePolygon(writer, poly);
                    }

                    writer.WriteEndArray();
                    break;
            }

            writer.WriteString("type", type);

            writer.WriteEndObject();
        }
    }
}