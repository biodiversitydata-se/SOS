using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SOS.Api.Contracts.Common;

/// <summary>
/// Accepts both offset-aware ISO timestamps and the offset-less or culture-formatted
/// DateTime strings emitted by older SOS clients.
/// Offset-less values are Swedish local time, independently of the host time zone.
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

        // The ISO path avoids culture parsing and preserves explicit offsets.
        if (reader.TryGetDateTime(out var isoDate))
        {
            return isoDate.Kind == DateTimeKind.Unspecified
                ? ReadSwedishTime(isoDate)
                : reader.GetDateTimeOffset();
        }

        var value = reader.GetString();
        const DateTimeStyles styles = DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.RoundtripKind;
        if (DateTime.TryParse(value, CultureInfo.InvariantCulture, styles, out var dateValue) ||
            DateTime.TryParse(value, CultureInfo.CurrentCulture, styles, out dateValue))
        {
            if (dateValue.Kind == DateTimeKind.Unspecified)
            {
                return ReadSwedishTime(dateValue);
            }
            if (DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out var offsetValue) ||
                DateTimeOffset.TryParse(value, CultureInfo.CurrentCulture, DateTimeStyles.AllowWhiteSpaces, out offsetValue))
            {
                return offsetValue;
            }
        }

        throw new JsonException($"'{value}' is not a supported date and time value.");
    }

    private static DateTimeOffset ReadSwedishTime(DateTime value)
    {
        try
        {
            return SwedishTime.FromLocalDateTime(value);
        }
        catch (ArgumentOutOfRangeException exception)
        {
            throw new JsonException($"'{value:O}' is not a supported date and time value.", exception);
        }
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
