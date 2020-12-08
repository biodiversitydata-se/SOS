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
    public class EndDayConverter : JsonConverter<DateTime?>
    {
        private DateTime? SetEndOfDay(DateTime? date)
        {
            if (!date.HasValue)
            {
                return null;
            }

            return new DateTime(date.Value.Year, date.Value.Month, date.Value.Day, 23,59,59);
        }

        /// <summary>
        /// Read nullable double from string
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="type"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public override DateTime? Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                DateTime? returnDate = null;

                var span = reader.HasValueSequence ? reader.ValueSequence.ToArray() : reader.ValueSpan;
                if (Utf8Parser.TryParse(span, out DateTime dateTime, out var bytesConsumed) &&
                    span.Length == bytesConsumed)
                {
                    returnDate = dateTime;
                }

                if (DateTime.TryParse(reader.GetString(), out dateTime))
                {
                    returnDate = dateTime;
                }

                return returnDate.HasValue && span.Length < 11 ? SetEndOfDay(returnDate) : returnDate;
            }

            return SetEndOfDay(reader.GetDateTime());
        }

        /// <summary>
        /// Write to json
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="options"></param>
        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
