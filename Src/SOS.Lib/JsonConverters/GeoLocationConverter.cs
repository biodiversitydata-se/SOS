using Elastic.Clients.Elasticsearch;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SOS.Lib.JsonConverters
{
    /// <summary>
    ///     JSON converter for GeoLocation.
    /// </summary>
    public class GeoLocationConverter : JsonConverter<GeoLocation>
    {
        public override bool CanConvert(Type type)
        {
            return typeof(GeoLocation).IsAssignableFrom(type);
        }

        public override GeoLocation Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            reader.Read();
            var property1 = reader.GetString();
            reader.Read();
            var val1 = reader.GetDouble();
            reader.Read();
            var property2 = reader.GetString();
            reader.Read();
            var val2 = reader.GetDouble();
            reader.Read();
            return GeoLocation.LatitudeLongitude(new LatLonGeoLocation
            {
                Lat = property1 == "lat" ? val1 : val2,
                Lon = property2 == "lon" ? val2 : val1
            });
        }


        public override void Write(Utf8JsonWriter writer, GeoLocation value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                return;
            }
            writer.WriteStartObject();
            if (value.TryGetLatitudeLongitude(out var latLon)) {
                writer.WriteNumber("lat", latLon.Lat);
                writer.WriteNumber("lon", latLon.Lon);
            }
            writer.WriteEndObject();
        }
    }
}