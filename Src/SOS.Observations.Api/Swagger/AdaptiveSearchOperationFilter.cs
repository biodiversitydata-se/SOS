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
        if (context.MethodInfo.DeclaringType == typeof(ObservationsController) &&
            context.MethodInfo.Name == nameof(ObservationsController.AdaptiveSearchLocationsInternal))
        {
            var response = (OpenApiResponse)operation.Responses["200"];
            response.Headers ??= new Dictionary<string, IOpenApiHeader>();
            response.Headers["X-AdaptiveSearch-Strategy"] = CreateHeaderSchema(JsonSchemaType.String,
                "The effective search strategy after resolving Auto: ObservationsThenGrid, ObservationsThenLocationsThenGrid or LocationsThenGrid. Reported also for cache hits and grid or empty results; X-Result-Type describes the returned representation.");
            response.Headers["X-Location-Summary-Approximated"] = CreateHeaderSchema(JsonSchemaType.Boolean,
                "True if any feature has potentially approximate summary information based on representative observations. " +
                "Covers observationSummary, presentation.statusCounts and potentially presentation.isAllObservationsSensitive, independently of JSON object boundaries. " +
                "Individual values may still be exact; observationCount remains the total matching count.");
            response.Headers["X-Location-Presentation-Status-Approximated"] = CreateHeaderSchema(JsonSchemaType.Boolean,
                "True if any presentation.status or presentation.cutoffYearStatus may be approximate. Applies only to these two properties, not the entire presentation object. " +
                "Always false for Auto, empty results and grid cells. presentation.statusCounts and potentially presentation.isAllObservationsSensitive are covered by X-Location-Summary-Approximated.");
            return;
        }

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
