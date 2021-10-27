using System;
using System.Buffers;
using System.Buffers.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SOS.Lib.JsonConverters
{
    /// <summary>
    /// Converter for nullable int
    /// </summary>
    public class StringTimeSpanConverter : JsonConverter<TimeSpan?>
    {
        /// <summary>
        /// Read nullable TimeSpan from string
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="type"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public override TimeSpan? Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null) return null;
            if (reader.TokenType == JsonTokenType.String)
            {
                string str = reader.GetString();
                return str == null ? null : TimeSpan.Parse(str);
            }

            throw new JsonException($"Cannot convert token of type {reader.TokenType} to {type}.");
        }

        /// <summary>
        /// Write to json
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="options"></param>
        public override void Write(Utf8JsonWriter writer, TimeSpan? value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
