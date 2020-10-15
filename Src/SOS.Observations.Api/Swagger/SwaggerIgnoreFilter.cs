using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.OpenApi.Models;
using SOS.Lib.Swagger;
using SOS.Observations.Api.Dtos;
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
                .Where(p => (p.GetCustomAttributes(typeof(JsonIgnoreAttribute), true)?.Any() == true) || p.GetCustomAttribute<SwaggerExcludeAttribute>() != null);
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

            //// Remove "Dto" suffix.
            //if (context.Type != null && !ImplementsInterface(context.Type, typeof(IEnumerable)) && TypeEndsWithDto(context.Type))
            //{
            //    schema.Title = GetNameWithoutDtoSuffix(context.Type);
            //}
        }

        private bool TypeEndsWithDto(Type type)
        {
            if (type.IsGenericType)
            {
                return type.GetGenericArguments().Any(TypeEndsWithDto);
            }

            return type.Name.EndsWith("Dto");
        }

        private string GetNameWithoutDtoSuffix(Type type)
        {
            if (type.IsGenericType)
            {
                var name = type.Name.Substring(0, type.Name.IndexOf('`'));
                if (name.EndsWith("Dto"))
                {
                    name = name.Substring(0, name.Length - 3);
                }

                var types = string.Join("", type.GetGenericArguments().Select(GetNameWithoutDtoSuffix));
                return $"{types}{name}";
            }

            if (type.Name.EndsWith("Dto"))
            {
                return type.Name.Substring(0, type.Name.Length - 3);
            }

            return type.Name;
        }

        private bool ImplementsInterface(Type type, Type interfaceType)
        {
            var implementedInterfaces = type.GetTypeInfo().ImplementedInterfaces.ToList();
            if (implementedInterfaces.Count == 0) return false;
            return implementedInterfaces.Any(m => m == interfaceType);
        }
    }
}