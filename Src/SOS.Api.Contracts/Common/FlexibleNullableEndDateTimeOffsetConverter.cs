using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SOS.Api.Contracts.Common;

/// <summary>
/// Reads a date-only search end date as 23:59:59 in Swedish time.
/// Explicit times and offsets, and ISO timestamp serialization, are preserved.
/// </summary>
public sealed class FlexibleNullableEndDateTimeOffsetConverter : JsonConverter<DateTimeOffset?>
{
    private static readonly FlexibleNullableDateTimeOffsetConverter DateTimeConverter = new();

    /// <summary>Reads a nullable end date, interpreting date-only strings as 23:59:59 in Swedish time.</summary>
    public override DateTimeOffset? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var value = reader.GetString();
            // DateOnly.TryParse also accepts some timestamps at midnight.
            // Only short date strings had end-of-day semantics in the original filter.
            if (value is not null && value.AsSpan().Trim().Length <= 10 &&
                (DateOnly.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out var date) ||
                 DateOnly.TryParse(value, CultureInfo.CurrentCulture, DateTimeStyles.AllowWhiteSpaces, out date)))
            {
                try
                {
                    return SwedishTime.FromLocalDateTime(date.ToDateTime(new TimeOnly(23, 59, 59)));
                }
                catch (ArgumentOutOfRangeException exception)
                {
                    throw new JsonException($"'{value}' is not a supported date and time value.", exception);
                }
            }
        }

        return DateTimeConverter.Read(ref reader, typeToConvert, options);
    }

    /// <summary>Writes an ISO timestamp with its offset, or JSON null when no end date is supplied.</summary>
    public override void Write(Utf8JsonWriter writer, DateTimeOffset? value, JsonSerializerOptions options) =>
        DateTimeConverter.Write(writer, value, options);
}
