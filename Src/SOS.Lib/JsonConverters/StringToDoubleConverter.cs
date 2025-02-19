﻿using System;
using System.Buffers;
using System.Buffers.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SOS.Lib.JsonConverters
{
    /// <summary>
    /// Converter for nullable double
    /// </summary>
    public class StringToDoubleConverter : JsonConverter<double>
    {
        /// <summary>
        /// Read nullable double from string
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="type"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public override double Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var span = reader.HasValueSequence ? reader.ValueSequence.ToArray() : reader.ValueSpan;
                if (Utf8Parser.TryParse(span, out double number, out var bytesConsumed) && span.Length == bytesConsumed)
                {
                    return number;
                }

                if (double.TryParse(reader.GetString(), out number))
                {
                    return number;
                }
            }

            return reader.GetDouble();
        }

        /// <summary>
        /// Write to json
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="options"></param>
        public override void Write(Utf8JsonWriter writer, double value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
