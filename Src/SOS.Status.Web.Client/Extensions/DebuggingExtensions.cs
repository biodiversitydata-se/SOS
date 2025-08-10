using System.Text.Json;

namespace SOS.Status.Web.Client.Extensions;

public static class DebuggingExtensions
{
    private static readonly JsonSerializerOptions options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static string ToJson(this object? obj)
        => JsonSerializer.Serialize(obj, options);
}