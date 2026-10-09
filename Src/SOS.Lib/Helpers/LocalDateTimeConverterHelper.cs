using SOS.Api.Contracts.Common;
using SOS.Lib.Extensions;
using System.Globalization;
using System.Text.Json.Nodes;

namespace SOS.Lib.Helpers;

/// <summary>
/// Converts observation response timestamps to Swedish time with an explicit UTC offset.
/// </summary>
public static class LocalDateTimeConverterHelper
{
    public static void ConvertToLocalTime(IEnumerable<JsonNode> processedRecords)
    {
        if (processedRecords == null) return;

        foreach (var record in processedRecords)
        {
            if (record is not JsonObject observation) continue;
            ConvertDate(observation, "created");
            ConvertDate(observation, "modified");
            if (observation["event"] is JsonObject ev)
            {
                ConvertDate(ev, "startDate");
                ConvertDate(ev, "endDate");
            }
            if (observation["occurrence"] is JsonObject occurrence)
                ConvertDate(occurrence, "reportedDate");
        }
    }

    private static void ConvertDate(JsonObject record, string property)
    {
        if (record[property] is not JsonValue value) return;

        DateTimeOffset instant;
        if (value.TryGetValue<string>(out var text))
            instant = DateTimeOffset.Parse(text, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
        else if (value.TryGetValue<DateTimeOffset>(out var offsetDate)) instant = offsetDate;
        else instant = value.GetValue<DateTime>().ToUtcDateTimeOffset();

        record[property] = TimeZoneInfo.ConvertTime(instant, SwedishTime.TimeZone);
    }
}
