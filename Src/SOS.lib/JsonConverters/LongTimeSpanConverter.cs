using System;
using System.Buffers;
using System.Buffers.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SOS.Lib.JsonConverters
{
    /// <summary>
    /// Converter for nullable TimeSpan.
    /// </summary>
    public class LongTimeSpanConverter : JsonConverter<TimeSpan?>
    {
        /// <summary>
        /// Read nullable TimeSpan from long.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="type"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public override TimeSpan? Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Number)
            {
                var span = reader.HasValueSequence ? reader.ValueSequence.ToArray() : reader.ValueSpan;
                if (Utf8Parser.TryParse(span, out long number, out var bytesConsumed) && span.Length == bytesConsumed)
                {
                    return TimeSpan.FromTicks(number);
                }

                return null;
            }

            return null;
        }

        /// <summary>
        /// Write to json
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="options"></param>
        public override void Write(Utf8JsonWriter writer, TimeSpan? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
            {
                writer.WriteNumberValue(value.Value.Ticks);
            }
        }
    }
}