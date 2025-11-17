using System.Text.Json;

namespace SOS.Status.Web.Client.Extensions;

public static class DebuggingExtensions
{
    private static readonly JsonSerializerOptions options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    extension(object? obj)
    {
        public string ToJson()
        => JsonSerializer.Serialize(obj, options);
    }
}