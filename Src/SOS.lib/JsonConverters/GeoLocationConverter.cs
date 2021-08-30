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
            return new GeoLocation(property1 == "lat" ? val1 : val2, property2 == "lon" ? val2 : val1);
        }


        public override void Write(Utf8JsonWriter writer, GeoLocation value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                return;
            }

            writer.WriteStartObject();
            writer.WriteNumber("lat", value.Latitude);
            writer.WriteNumber("lon", value.Longitude);
            writer.WriteEndObject();
        }
    }
}