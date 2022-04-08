using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Nest;
using Newtonsoft.Json;
using SOS.Lib.Models.Shared;

namespace SOS.Lib.JsonConverters
{
    /// <summary>
    ///     Json converter for GeoJsonGeometry
    /// </summary>
    public class NewtonsoftGeoJsonGeometryConverter : JsonConverter<GeoJsonGeometry>
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

        public override void WriteJson(JsonWriter writer, GeoJsonGeometry? value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }

        public override GeoJsonGeometry? ReadJson(JsonReader reader, Type objectType, GeoJsonGeometry? existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;

            if (reader.TokenType != JsonToken.StartObject)
            {
                throw new JsonException();
            }

            var coordinateString = string.Empty;
            var type = string.Empty;

            reader.Read();

            while (reader.TokenType != JsonToken.EndObject)
            {                
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
                    return new GeoJsonGeometry()
                    {
                        Type = "Point",
                        Coordinates = new System.Collections.ArrayList(pointCoordinates)
                    };
                case "polygon":
                case "holepolygon":
                    IEnumerable<IEnumerable<double[]>>? polygonCoordinatesEnumerable =
                        JsonConvert.DeserializeObject<IEnumerable<IEnumerable<double[]>>>(coordinateString);
                    List<double[][]> polygonCoordinates = polygonCoordinatesEnumerable
                        .Select((x, i) => x.Select((y, j) => y).ToArray()).ToList();
                    return new GeoJsonGeometry()
                    {
                        Type = "Polygon",
                        Coordinates = new System.Collections.ArrayList(polygonCoordinates)
                    };

                case "multipolygon":
                    var multipolygonCoordinatesEnumerable =
                        JsonConvert.DeserializeObject<IEnumerable<IEnumerable<IEnumerable<double[]>>>>(coordinateString);
                    List<double[][][]>? multipolygonCoordinates = multipolygonCoordinatesEnumerable
                        .Select(x => x.Select(y => y.Select(m => m).ToArray()).ToArray()).ToList();
                    return new GeoJsonGeometry()
                    {
                        Type = "Multipolygon",
                        Coordinates = new System.Collections.ArrayList(multipolygonCoordinates)
                    };
            }

            return null;
        }
    }
}