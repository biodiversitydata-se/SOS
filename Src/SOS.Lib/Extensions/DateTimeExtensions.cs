namespace SOS.Lib.Extensions;

/// <summary>Converts UTC storage timestamps to API timestamps with an explicit offset.</summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// Treats unspecified storage timestamps, including default values, as UTC.
    /// Local timestamps are converted to UTC without changing their instant.
    /// </summary>
    public static DateTimeOffset ToUtcDateTimeOffset(this DateTime value)
    {
        var utc = value.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(value, DateTimeKind.Utc)
            : value.ToUniversalTime();
        return new DateTimeOffset(utc);
    }

    /// <summary>Converts a nullable storage timestamp, preserving null.</summary>
    public static DateTimeOffset? ToUtcDateTimeOffset(this DateTime? value) => value?.ToUtcDateTimeOffset();
}
