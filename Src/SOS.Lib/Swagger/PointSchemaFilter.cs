using System.Collections.Generic;
using Swashbuckle.AspNetCore.SwaggerGen;
using NetTopologySuite.Geometries;
using Microsoft.OpenApi;
using System.Text.Json.Nodes;

namespace SOS.Lib.Swagger;

public class PointSchemaFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema is OpenApiSchema openApiSchema && context.Type == typeof(Point))
        {
            openApiSchema.Type = JsonSchemaType.Object;
            openApiSchema.Required = new HashSet<string> { "type", "coordinates" };

            openApiSchema.Properties = new Dictionary<string, IOpenApiSchema>
            {
                ["type"] = new OpenApiSchema
                {
                    Type = JsonSchemaType.String,
                    Enum = new List<JsonNode> { JsonValue.Create("Point") }
                },
                ["coordinates"] = new OpenApiSchema
                {
                    Type = JsonSchemaType.Array,
                    Items = new OpenApiSchema { Type = JsonSchemaType.Number },
                    MinItems = 2
                }
            };            
        }
    }
}