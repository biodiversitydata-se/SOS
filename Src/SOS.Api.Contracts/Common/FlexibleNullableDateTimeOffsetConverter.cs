using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SOS.Api.Contracts.Common;

/// <summary>
/// Accepts both offset-aware ISO timestamps and the offset-less or culture-formatted
/// DateTime strings emitted by older SOS clients.
/// </summary>
public sealed class FlexibleNullableDateTimeOffsetConverter : JsonConverter<DateTimeOffset?>
{
    /// <summary>Reads a nullable date-time, accepting ISO timestamps and date strings from older SOS clients.</summary>
    public override DateTimeOffset? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException("Expected a date and time string.");
        }

        var value = reader.GetString();
        if (DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out var offsetValue) ||
            DateTimeOffset.TryParse(value, CultureInfo.CurrentCulture, DateTimeStyles.AllowWhiteSpaces, out offsetValue))
        {
            return offsetValue;
        }

        if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out var dateValue) ||
            DateTime.TryParse(value, CultureInfo.CurrentCulture, DateTimeStyles.AllowWhiteSpaces, out dateValue))
        {
            return new DateTimeOffset(DateTime.SpecifyKind(dateValue, DateTimeKind.Local));
        }

        throw new JsonException($"'{value}' is not a supported date and time value.");
    }

    /// <summary>Writes an ISO timestamp with its offset, or JSON null when no date-time is supplied.</summary>
    public override void Write(Utf8JsonWriter writer, DateTimeOffset? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            writer.WriteStringValue(value.Value);
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}
