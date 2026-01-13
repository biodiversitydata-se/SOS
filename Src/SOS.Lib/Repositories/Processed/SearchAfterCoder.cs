using AgileObjects.AgileMapper.Extensions;
using CSharpFunctionalExtensions;
using Elastic.Clients.Elasticsearch;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;
using System.Text;
using System.Text.Json;

namespace SOS.Lib.Repositories.Processed;

public static class SearchAfterCoder
{
    public static string? Encode(IReadOnlyCollection<FieldValue>? sortValues)
    {
        if (sortValues == null || !sortValues.Any()) return null;

        // Extrahera de råa värdena (t.ex. strängar, siffror)
        var values = sortValues.Select(fv => fv.Value).ToArray();
        var json = JsonSerializer.Serialize(values);
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
    }

    public static string? Encode(ICollection<FieldValue>? sortValues)
    {
        if (sortValues == null || !sortValues.Any()) return null;
        
        var values = sortValues.Select(fv => fv.Value).ToArray();
        var json = JsonSerializer.Serialize(values);
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
    }

    public static ICollection<FieldValue>? Decode(string? cursor)
    {
        if (string.IsNullOrWhiteSpace(cursor)) return null;

        try
        {
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(cursor));
            var objects = JsonSerializer.Deserialize<object[]>(json);

            return objects?.Select(MapElement).ToList();
        }
        catch
        {
            return null;
        }
    }

    private static FieldValue MapElement(object? obj)
    {
        if (obj is JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.String => FieldValue.String(element.GetString()!),
                JsonValueKind.Number => element.TryGetInt64(out var l) ? FieldValue.Long(l) : FieldValue.Double(element.GetDouble()),
                JsonValueKind.True => FieldValue.Boolean(true),
                JsonValueKind.False => FieldValue.Boolean(false),
                _ => FieldValue.String(element.ToString())
            };
        }
        return FieldValue.String(obj?.ToString() ?? string.Empty);
    }
}