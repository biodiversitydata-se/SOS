namespace SOS.Api.Contracts.Common;

/// <summary>Swedish calendar-time rules shared by API filters, processing and clients.</summary>
public static class SwedishTime
{
    /// <summary>The IANA time-zone identifier used for Swedish calendar calculations.</summary>
    public const string TimeZoneId = "Europe/Stockholm";

    /// <summary>The Swedish time zone, including historical and daylight-saving rules.</summary>
    public static TimeZoneInfo TimeZone { get; } = ResolveTimeZone();

    /// <summary>Interprets a wall-clock value as Swedish time, without using the host's local time zone.</summary>
    public static DateTimeOffset FromLocalDateTime(DateTime value)
    {
        var local = DateTime.SpecifyKind(value, DateTimeKind.Unspecified);
        // Preserve the minimum-date sentinel without underflowing when applying a positive offset.
        return local == DateTime.MinValue
            ? DateTimeOffset.MinValue
            : new DateTimeOffset(local, TimeZone.GetUtcOffset(local));
    }

    /// <summary>Converts an instant to UTC; offset-less observation dates are interpreted as Swedish time.</summary>
    public static DateTime ToUtcDateTime(DateTime value) => value.Kind == DateTimeKind.Unspecified
        ? FromLocalDateTime(value).UtcDateTime
        : value.ToUniversalTime();

    private static TimeZoneInfo ResolveTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId);
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
        }
    }
}
