using Microsoft.OpenApi;
using SOS.Api.Contracts.Observations.Responses;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SOS.Observations.Api.Swagger;

/// <summary>The observation location's type is returned as a number, not an enum name.</summary>
public sealed class ObservationLocationSchemaFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type != typeof(Location) || schema.Properties is not { } properties) return;

        properties["type"] = new OpenApiSchema
        {
            Type = JsonSchemaType.Integer,
            Format = "int32",
            Description = "Location type. 1=Point, 2=Polygon."
        };
    }
}
