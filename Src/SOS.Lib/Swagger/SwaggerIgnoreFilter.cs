using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SOS.Lib.Swagger;

/// <summary>
///     Swagger ignore filter
/// </summary>
public class SwaggerIgnoreFilter : ISchemaFilter
{
    /// <summary>
    ///     Apply filter
    /// </summary>
    /// <param name="schema"></param>
    /// <param name="context"></param>
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema.Properties is not { } properties) return;

        var excludeProperties = context.Type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(property => property.GetCustomAttribute<JsonIgnoreAttribute>()?.Condition == JsonIgnoreCondition.Always
                || property.GetCustomAttribute<SwaggerExcludeAttribute>() != null);
        foreach (var property in excludeProperties)
        {
            var propertyName = property.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name
                ?? JsonNamingPolicy.CamelCase.ConvertName(property.Name);
            properties.Remove(propertyName);
        }
    }
}
