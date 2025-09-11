using System.Collections.Generic;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.OpenApi.Models;
using NetTopologySuite.Geometries;
using Microsoft.OpenApi.Any;

namespace SOS.Lib.Swagger;

public class PointSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type == typeof(Point))
        {
            schema.Type = "object";
            schema.Required = new HashSet<string> { "type", "coordinates" };

            schema.Properties = new Dictionary<string, OpenApiSchema>
            {
                ["type"] = new OpenApiSchema
                {
                    Type = "string",
                    Enum = new List<IOpenApiAny>
                    {
                        new OpenApiString("Point"),                        
                    }
                },
                ["coordinates"] = new OpenApiSchema
                {
                    Type = "array",
                    Items = new OpenApiSchema { Type = "number" },
                    MinItems = 2
                }
            };
        }
    }
}