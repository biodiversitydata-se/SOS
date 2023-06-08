using Microsoft.OpenApi.Models;
using SOS.Lib.Extensions;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;
using System.Reflection;

namespace SOS.Lib.Swagger
{
    /// <summary>
    ///     Swagger ignore filter
    /// </summary>
    public class SwaggerForceSchemaFilter : ISchemaFilter
    {
        /// <summary>
        ///     Apply filter
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="context"></param>
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            var propertyInfos = context.Type?.GetProperties(BindingFlags.Public | BindingFlags.Instance)
               .Where(p => p.GetCustomAttribute<UseSchema>() != null);

            if (propertyInfos != null)
            {
                foreach(var propertyInfo in propertyInfos)
                {
                    var propertyName = propertyInfo.Name.ToCamelCase();
                    if (schema.Properties.ContainsKey(propertyName))
                    {
                        schema.Properties[propertyName] = propertyInfo?.GetCustomAttribute<UseSchema>()?.Schema;
                    }
                }
            }
        }
    }
}