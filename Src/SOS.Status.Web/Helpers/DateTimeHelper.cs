using SOS.Shared.Api.Dtos.Status;

namespace SOS.Status.Web.Helpers;

public class DateTimeHelper
{
    public static string FormatTimeSpan(TimeSpan timeSpan)
    {
        if (timeSpan.TotalMinutes < 1)
            return "0 min";

        var parts = new List<string>();

        if (timeSpan.TotalHours >= 1)
            parts.Add($"{(int)timeSpan.TotalHours} h");

        if (timeSpan.Minutes > 0)
            parts.Add($"{timeSpan.Minutes} min");

        return string.Join(" ", parts);
    }

    public static string GetDaysAgo(DateTime dateTime)
    {
        return $"{(int)Math.Floor((DateTime.UtcNow - dateTime).TotalDays)} days ago";
    }

    public static string GetTimeAgo(DateTime dateTime)
    {
        return GetTimeAgo(DateTime.UtcNow - dateTime);
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