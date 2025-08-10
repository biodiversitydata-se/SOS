using MudBlazor;

namespace SOS.Status.Web.Client.Helpers;

public static class ColorHelper
{
    public static readonly string SuccessColor = "#4BB543";
    //public static readonly string SuccessColor = "#28a745";        
    //public static readonly string SuccessColor = "#5cb85c";

    public static readonly string WarningColor = "#ffc107";
    public static readonly string DangerColor = "#dc3545";

    public static Color GetHealthColor(string? status)
    {
        Console.WriteLine($"GetHealthColor(\"{status}\")");

        return status?.ToLowerInvariant() switch
        {
            null => Color.Success,
            "healthy" => Color.Success,
            "degraded" => Color.Warning,
            "unhealthy" => Color.Error,
            _ => Color.Success
        };
    }

    public static Color GetHealthColor(params string?[] statuses)
    {
        int GetSeverity(string? status) => status?.ToLowerInvariant() switch
        {
            "healthy" => 0,
            "degraded" => 1,
            "unhealthy" => 2,
            _ => 0 // Null or unknown status is considered healthy
        };

        var worst = statuses.Max(GetSeverity);

        return worst switch
        {
            2 => Color.Error,
            1 => Color.Warning,
            _ => Color.Success
        };
    }
}
