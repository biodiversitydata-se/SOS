using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Nest;
using Newtonsoft.Json;

namespace SOS.Lib.JsonConverters
{
    /// <summary>
    ///     Json converter for IGeoShape
    /// </summary>
    public class NewtonsoftGeoShapeConverter : JsonConverter<IGeoShape>
    {
        private string ReadCoordinateData(ref JsonReader reader)
        {
            var result = new StringBuilder();
            var openBrackets = 0;
            var addComma = false;

            // while (openBrackets != 0 || firstLoop)
            do
            {
                if (reader.TokenType == JsonToken.StartArray)
                {
                    if (addComma)
                    {
                        result.Append(',');
                        addComma = false;
                    }

                    openBrackets++;
                }

                //if (reader.TokenType == JsonToken.Number && addComma)
                if (reader.TokenType is JsonToken.Float or JsonToken.Integer && addComma)
                {
                    result.Append(',');
                }

                if (reader.TokenType == JsonToken.StartArray)
                {
                    result.Append("[");
                }
                else if (reader.TokenType == JsonToken.EndArray)
                {
                    result.Append("]");
                }
                else if (reader.TokenType is JsonToken.Float or JsonToken.Integer)
                {
                    var str = Convert.ToDouble(reader.Value).ToString(CultureInfo.InvariantCulture);
                    result.Append(str);
                }

                addComma = reader.TokenType is JsonToken.Float or JsonToken.Integer && !addComma;

                if (reader.TokenType == JsonToken.EndArray)
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

        private void WritePoint(JsonWriter writer, GeoCoordinate coordinate)
        {
            writer.WriteStartArray();

            writer.WriteValue(coordinate.Longitude);
            writer.WriteValue(coordinate.Latitude);

            writer.WriteEndArray();
        }

        private void WritePolygon(JsonWriter writer, IEnumerable<IEnumerable<GeoCoordinate>> coordinates)
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

        public override void WriteJson(JsonWriter writer, IGeoShape value, JsonSerializer serializer)
        {
            if (value == null)
            {
                return;
            }

            writer.WriteStartObject();

            var type = value.Type;

            writer.WritePropertyName("coordinates");
            switch (type?.ToLower())
            {
                case "point":
                    var point = (PointGeoShape)value;
                    WritePoint(writer, point.Coordinates);
                    break;
                case "polygon":
                    var polygon = (PolygonGeoShape)value;
                    WritePolygon(writer, polygon.Coordinates);
                    break;
                case "multipolygon":
                    var muliPolygon = (MultiPolygonGeoShape)value;
                    writer.WriteStartArray();
                    foreach (var poly in muliPolygon.Coordinates)
                    {
                        WritePolygon(writer, poly);
                    }

                    writer.WriteEndArray();
                    break;
            }

            writer.WritePropertyName("type");
            writer.WriteValue(type);

            writer.WriteEndObject();
        }

        public override IGeoShape ReadJson(JsonReader reader, Type objectType, IGeoShape existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.StartObject)
            {
                throw new JsonException();
            }

            var coordinateString = string.Empty;
            var type = string.Empty;

            reader.Read();

            while (reader.TokenType != JsonToken.EndObject)
            {
                //var property = reader.GetString();
                var property = Convert.ToString(reader.Value);

                reader.Read();

                switch (property.ToLower())
                {
                    case "coordinates":
                        coordinateString = ReadCoordinateData(ref reader);
                        break;
                    case "type":
                        type = Convert.ToString(reader.Value);
                        break;
                }

                reader.Read();
            }

            switch (type?.ToLower())
            {
                case "point":
                    var pointCoordinates = JsonConvert.DeserializeObject<double[]>(coordinateString);
                    return new PointGeoShape(new GeoCoordinate(pointCoordinates[1], pointCoordinates[0]));
                case "polygon":
                    var polygonCoordinates =
                        JsonConvert.DeserializeObject<IEnumerable<IEnumerable<double[]>>>(coordinateString);
                    return new PolygonGeoShape(polygonCoordinates
                        .Select(ls => ls
                            .Select(pnt => new GeoCoordinate(pnt[1], pnt[0]))));
                case "multipolygon":
                    var muliPolygonCoordinates =
                        JsonConvert.DeserializeObject<IEnumerable<IEnumerable<IEnumerable<double[]>>>>(coordinateString);
                    return new MultiPolygonGeoShape(muliPolygonCoordinates
                        .Select(poly => poly
                            .Select(ls => ls
                                .Select(pnt => new GeoCoordinate(pnt[1], pnt[0])))));
            }

            return null;
        }
    }
}