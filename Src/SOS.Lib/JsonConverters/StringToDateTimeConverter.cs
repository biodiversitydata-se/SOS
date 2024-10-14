using System;
using System.Buffers;
using System.Buffers.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SOS.Lib.JsonConverters
{
    /// <summary>
    /// Converter for nullable DateTime
    /// </summary>
    public class StringToDateTimeConverter : JsonConverter<DateTime>
    {
        /// <summary>
        /// Read nullable double from string
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="type"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public override DateTime Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var span = reader.HasValueSequence ? reader.ValueSequence.ToArray() : reader.ValueSpan;
                if (Utf8Parser.TryParse(span, out DateTime dateTime, out var bytesConsumed) && span.Length == bytesConsumed)
                {
                    return dateTime;
                }

                if (DateTime.TryParse(reader.GetString(), out dateTime))
                {
                    return dateTime;
                }
            }

            return reader.GetDateTime();
        }

        /// <summary>
        /// Write to json
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="options"></param>
        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
