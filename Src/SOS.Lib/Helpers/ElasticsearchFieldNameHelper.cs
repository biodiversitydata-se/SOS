using SOS.Lib.Extensions;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;

namespace SOS.Lib.Helpers;

/// <summary>
/// Resolves case-insensitive property paths to the names stored in Elasticsearch.
/// </summary>
public static class ElasticsearchFieldNameHelper
{
    /// <summary>
    /// Gets the canonical field name. Valid paths are cached per entity type,
    /// with all casing variants sharing an entry. Unknown paths and wildcards
    /// retain the existing camel-case fallback and are not cached.
    /// </summary>
    public static string GetFieldName<TEntity>(string field) where TEntity : class
    {
        if (string.IsNullOrEmpty(field)) return field;
        if (FieldNames<TEntity>.Cache.TryGetValue(field, out var cached)) return cached;

        var segments = field.Split('.');
        var type = typeof(TEntity);
        for (var i = 0; i < segments.Length; i++)
        {
            var property = type.GetProperty(segments[i], BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (property == null)
            {
                return string.Join('.', field.Split('.').Select(segment => segment.ToCamelCase()));
            }

            segments[i] = JsonNamingPolicy.CamelCase.ConvertName(property.Name);
            type = property.GetPropertyType();
            if (type != typeof(string) && typeof(System.Collections.IEnumerable).IsAssignableFrom(type))
            {
                type = type.IsArray ? type.GetElementType()! :
                    type.GetInterfaces().Append(type)
                        .FirstOrDefault(candidate => candidate.IsGenericType && candidate.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                        ?.GetGenericArguments()[0] ?? type;
            }
        }

        return FieldNames<TEntity>.Cache.GetOrAdd(field, string.Join('.', segments));
    }

    private static class FieldNames<TEntity>
    {
        internal static readonly ConcurrentDictionary<string, string> Cache = new(StringComparer.OrdinalIgnoreCase);
    }
}
