using System;
using System.Globalization;

namespace SOS.Lib.Helpers;

public static class DwcFormatter
{
    /// <summary>Formats event dates while preserving their explicit UTC offsets.</summary>
    public static string CreateDateIntervalString(DateTimeOffset? startDate, DateTimeOffset? endDate) =>
        CreateDateIntervalString(startDate, startDate?.TimeOfDay, endDate, endDate?.TimeOfDay);

    /// <summary>Formats an event interval, omitting clocks when the observation time is unknown.</summary>
    public static string CreateDateIntervalString(DateTimeOffset? startDate, TimeSpan? startTime, DateTimeOffset? endDate, TimeSpan? endTime)
    {
        if (!startDate.HasValue) return null;

        var startFormat = startTime.HasValue && startTime != TimeSpan.Zero
            ? "yyyy-MM-dd'T'HH:mm:sszzz" : "yyyy-MM-dd";
        var start = startDate.Value.ToString(startFormat, CultureInfo.InvariantCulture);
        if (!endDate.HasValue || startDate == endDate
            || startDate.Value.Date == endDate.Value.Date && startTime == endTime
                && (startTime is null || startTime == TimeSpan.Zero || startDate.Value.Offset == endDate.Value.Offset))
            return start;

        var endFormat = endTime.HasValue && endTime != TimeSpan.Zero
            ? "yyyy-MM-dd'T'HH:mm:sszzz" : "yyyy-MM-dd";
        return $"{start}/{endDate.Value.ToString(endFormat, CultureInfo.InvariantCulture)}";
    }

    /// <summary>Formats event clocks with their explicit UTC offsets.</summary>
    public static string CreateTimeIntervalString(DateTimeOffset? startDate, DateTimeOffset? endDate)
    {
        if (!startDate.HasValue || startDate.Value.TimeOfDay == TimeSpan.Zero) return null;
        var start = startDate.Value.ToString("HH:mm:sszzz", CultureInfo.InvariantCulture);
        if (!endDate.HasValue || startDate == endDate) return start;
        if (endDate.Value.TimeOfDay == TimeSpan.Zero) return null;
        return $"{start}/{endDate.Value.ToString("HH:mm:sszzz", CultureInfo.InvariantCulture)}";
    }

    /// <summary>
    ///     Create a date formatted according to the ISO 8601 standard.
    /// </summary>
    /// <param name="date">The date.</param>
    /// <returns>A date that is formatted according to the ISO 8601 standard.</returns>
    public static string CreateDateString(DateTime? date)
    {
        return date?.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ssK", CultureInfo.InvariantCulture);
    }

    /// <summary>
    ///     Create a time formatted according to the ISO 8601 standard.
    /// </summary>
    /// <param name="date">The date.</param>
    /// <returns>A time that is formatted according to the ISO 8601 standard.</returns>
    public static string CreateTimeString(DateTime? date)
    {
        return date?.ToUniversalTime().ToString("HH:mm:ssK", CultureInfo.InvariantCulture);
    }

    /// <summary>
    ///     Create a date interval formatting according to the ISO 8601 standard.
    /// </summary>
    /// <param name="date1">From date.</param>
    /// <param name="date2">To date.</param>
    /// <returns>A date interval that is formatted according to the ISO 8601 standard.</returns>
    public static string CreateDateIntervalString(DateTime? date1, DateTime? date2)
    {
        if (!date1.HasValue)
        {
            return null;
        }

        string startDateFormat = "yyyy-MM-dd";
        if (date1.Value.TimeOfDay.TotalSeconds > 0)
        {
            startDateFormat = "yyyy-MM-dd'T'HH:mm:ssK";
        }

        if (!date2.HasValue || date1 == date2)
        {
            return date1.Value.ToString(startDateFormat, CultureInfo.InvariantCulture);
        }

        string endDateFormat = "yyyy-MM-dd";
        if (date2.Value.TimeOfDay.TotalSeconds > 0)
        {
            endDateFormat = "yyyy-MM-dd'T'HH:mm:ssK";
        }

        return string.Format(
            "{0}/{1}",
            date1.Value.ToString(startDateFormat, CultureInfo.InvariantCulture),
            date2.Value.ToString(endDateFormat, CultureInfo.InvariantCulture));
    }

    public static string CreateDateIntervalString(DateTime? startDate, TimeSpan? startTime, DateTime? endDate, TimeSpan? endTime)
    {
        if (!startDate.HasValue)
        {
            return null;
        }

        string startDateFormat = "yyyy-MM-dd";
        if (startTime.HasValue && startTime.Value != TimeSpan.Zero)
        {
            startDateFormat = "yyyy-MM-dd'T'HH:mm:ssK";
        }

        if (!endDate.HasValue || startDate == endDate
            || startDate.Value.Year == endDate.Value.Year && startDate.Value.Month == endDate.Value.Month && startDate.Value.Day == endDate.Value.Day && startTime == endTime)
        {
            return startDate.Value.ToString(startDateFormat, CultureInfo.InvariantCulture);
        }

        string endDateFormat = "yyyy-MM-dd";
        if (endTime.HasValue && endTime.Value != TimeSpan.Zero)
        {
            endDateFormat = "yyyy-MM-dd'T'HH:mm:ssK";
        }

        return string.Format(
            "{0}/{1}",
            startDate.Value.ToString(startDateFormat, CultureInfo.InvariantCulture),
            endDate.Value.ToString(endDateFormat, CultureInfo.InvariantCulture));
    }

    /// <summary>
    ///     Create a time interval formatting according to the ISO 8601 standard.
    /// </summary>
    /// <param name="date1">From date.</param>
    /// <param name="date2">To date.</param>
    /// <returns>A time interval that is formatted according to the ISO 8601 standard.</returns>
    public static string CreateTimeIntervalString(DateTime? date1, DateTime? date2)
    {
        if (!date1.HasValue)
        {
            return null;
        }

        if (!date2.HasValue || date1 == date2)
        {
            if (Math.Abs(date1.Value.TimeOfDay.TotalSeconds) < 0.001) return null;

            return date1.Value.ToString("HH:mm:ssK", CultureInfo.InvariantCulture);
        }

        if (Math.Abs(date1.Value.TimeOfDay.TotalSeconds) < 0.001 || Math.Abs(date2.Value.TimeOfDay.TotalSeconds) < 0.001) return null;

        return string.Format(
            "{0}/{1}",
            date1.Value.ToString("HH:mm:ssK", CultureInfo.InvariantCulture),
            date2.Value.ToString("HH:mm:ssK", CultureInfo.InvariantCulture));
    }
}
