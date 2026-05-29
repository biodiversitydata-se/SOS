using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi;
using SOS.Lib.Models.Processed.Observation;
using SOS.Shared.Api.Dtos;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using Location = SOS.Lib.Models.Processed.Observation.Location;

namespace SOS.Observations.Api.Swagger;

public class AdaptiveSearchOperationFilter : IOperationFilter
{
    private const string ControllerName = "Observations";
    private const string MethodName = "AdaptiveSearchInternal";

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (!IsAdaptiveSearchOperation(context.MethodInfo, context.ApiDescription))
        {
            return;
        }

        var jsonSchema = context.SchemaGenerator.GenerateSchema(typeof(AdaptiveSearchResultDto<Observation>), context.SchemaRepository);
        var jsonByLocationSchema = context.SchemaGenerator.GenerateSchema(typeof(AdaptiveSearchResultByLocationDto<Observation, Location>), context.SchemaRepository);
        var geoJsonSchema = context.SchemaGenerator.GenerateSchema(typeof(GeoJsonFeatureCollectionDto), context.SchemaRepository);

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

    private static bool IsAdaptiveSearchOperation(MethodInfo methodInfo, ApiDescription apiDescription)
    {
        return string.Equals(methodInfo.Name, MethodName, StringComparison.Ordinal)
               && string.Equals(apiDescription.ActionDescriptor.RouteValues["controller"], ControllerName, StringComparison.Ordinal);
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
