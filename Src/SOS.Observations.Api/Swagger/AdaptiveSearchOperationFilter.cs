using Microsoft.OpenApi;
using GeoJSON.Text.Feature;
using SOS.Api.Contracts.Observations.Responses;
using SOS.Observations.Api.Controllers;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SOS.Observations.Api.Swagger;

public class AdaptiveSearchOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.MethodInfo.DeclaringType != typeof(ObservationsController)
            || context.MethodInfo.Name != nameof(ObservationsController.AdaptiveSearchInternal))
        {
            return;
        }

        var jsonSchema = context.SchemaGenerator.GenerateSchema(typeof(AdaptiveSearchResultDto<Observation>), context.SchemaRepository);
        var jsonByLocationSchema = context.SchemaGenerator.GenerateSchema(typeof(AdaptiveSearchResultByLocationDto<Observation, Location>), context.SchemaRepository);
        var geoJsonSchema = context.SchemaGenerator.GenerateSchema(typeof(FeatureCollection), context.SchemaRepository);

        operation.Responses["200"] = new OpenApiResponse
        {
            Description = "Adaptive search response. The payload shape depends on outputFormat and the total number of matching observations.",
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["application/json"] = new OpenApiMediaType
                {
                    Schema = new OpenApiSchema
                    {
                        OneOf =
                        [
                            jsonSchema,
                            jsonByLocationSchema
                        ]
                    }
                },
                ["application/geo+json"] = new OpenApiMediaType
                {
                    Schema = geoJsonSchema
                }
            },
            Headers = new Dictionary<string, IOpenApiHeader>
            {
                ["X-Result-Type"] = CreateHeaderSchema(
                    JsonSchemaType.String,
                    "Result type. GeoJSON responses return either 'observations' or 'geogrid'."),
                ["X-Observations-TotalCount"] = CreateHeaderSchema(
                    JsonSchemaType.Integer,
                    "Total number of matching observations.",
                    format: "int64"),
                ["X-Observations-Count"] = CreateHeaderSchema(
                    JsonSchemaType.Integer,
                    "Number of observation records included in GeoJSON observation responses.",
                    format: "int32"),
                ["X-Result-Count"] = CreateHeaderSchema(
                    JsonSchemaType.Integer,
                    "Number of returned features or grouped locations.",
                    format: "int32"),
                ["X-Zoom"] = CreateHeaderSchema(
                    JsonSchemaType.Integer,
                    "Calculated geotile zoom level for aggregated GeoJSON responses.",
                    format: "int32")
            }
        };
    }

    private static OpenApiHeader CreateHeaderSchema(JsonSchemaType schemaType, string description, string? format = null)
    {
        return new OpenApiHeader
        {
            Description = description,
            Schema = new OpenApiSchema
            {
                Type = schemaType,
                Format = format
            }
        };
    }
}
