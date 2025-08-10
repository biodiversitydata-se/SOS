namespace SOS.Status.Web.Client.Helpers;

public class DateTimeHelper
{
    public static string FormatTimeSpan(TimeSpan? timeSpan, TimeSpan? minThreshold = null)
    {
        if (timeSpan == null)
            return "";

        var ts = timeSpan.Value;
        bool isNegative = ts.Ticks < 0;
        ts = ts.Duration(); // Absolute value

        // Check against minimum threshold
        if (minThreshold.HasValue && ts < minThreshold.Value)
            return "";
        //return "0";

        string result;

        if (ts.TotalMinutes < 1)
        {
            result = $"{ts.Seconds} s";
        }
        else
        {
            var parts = new List<string>();

            if (ts.TotalHours >= 1)
                parts.Add($"{(int)ts.TotalHours} h");

            if (ts.Minutes > 0)
                parts.Add($"{ts.Minutes} min");

            result = string.Join(" ", parts);
        }

        if (isNegative)
            result = "-" + result;

        return result;
    }

    public static string GetDaysAgo(DateTime dateTime)
    {
        return $"{(int)Math.Floor((DateTime.UtcNow - dateTime).TotalDays)} days ago";
    }

    public static string GetTimeAgo(DateTime? dateTime)
    {
        if (dateTime == null) return "";

        return GetTimeAgo(DateTime.UtcNow - dateTime.Value);
    }

    public static string GetTimeAgo(TimeSpan timeSpan)
    {
        int daysAgo = (int)Math.Floor(timeSpan.TotalDays);
        if (daysAgo >= 1) return $"{daysAgo} days ago";

        int hoursAgo = (int)Math.Floor(timeSpan.TotalHours);
        if (hoursAgo >= 1) return $"{hoursAgo} hours ago";

        int minutesAgo = (int)Math.Floor(timeSpan.TotalMinutes);
        if (minutesAgo >= 1) return $"{minutesAgo} minutes ago";

        int secondsAgo = (int)Math.Floor(timeSpan.TotalSeconds);
        return $"{secondsAgo} seconds ago";
    }
}