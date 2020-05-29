using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SOS.Observations.Api.Swagger
{
    /// <summary>
    ///     Swagger ignore filter
    /// </summary>
    internal class SwaggerIgnoreFilter : ISchemaFilter
    {
        /// <summary>
        ///     Apply filter
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="context"></param>
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            var excludeProperties = context.Type?.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.GetCustomAttributes(typeof(JsonIgnoreAttribute), true)?.Any() == true);
            if (excludeProperties != null)
            {
                foreach (var property in excludeProperties)
                {
                    var propertyName = $"{property.Name.Substring(0, 1).ToLower()}{property.Name.Substring(1)}";
                    if (schema.Properties.ContainsKey(propertyName))
                    {
                        schema.Properties.Remove(propertyName);
                    }
                }
            }
        }
    }
}