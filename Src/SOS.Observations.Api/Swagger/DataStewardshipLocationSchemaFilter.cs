using Microsoft.OpenApi;
using SOS.Api.Contracts.DataStewardship.Responses;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SOS.Observations.Api.Swagger;

/// <summary>Allows missing administrative areas without making the shared area enums nullable.</summary>
public sealed class DataStewardshipLocationSchemaFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type != typeof(DsLocationDto) || schema.Properties is not { } properties) return;

        // A bare enum $ref cannot express this property's nullability in OpenAPI 3.0.
        foreach (var name in new[] { "county", "province", "municipality", "parish" })
        {
            properties[name] = new OpenApiSchema
            {
                Type = JsonSchemaType.String | JsonSchemaType.Null,
                AnyOf = [properties[name], new OpenApiSchema { Enum = [null] }]
            };
        }
    }
}
