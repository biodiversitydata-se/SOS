using SOS.Api.Contracts.Aggregations.Requests;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SOS.Observations.Api.Swagger;

/// <summary>
/// Removes search-filter properties that are accepted by the public API runtime but are not
/// part of the Azure Public API contract.
/// </summary>
public class AzurePublicSearchFilterDocumentFilter : IDocumentFilter
{
    private const string AzurePublicDocumentPrefix = "AzurePublicSosObservations";
    private const string ArtportalenPropertyName = "artportalen";
    private const string ArtportalenFilterSchemaName = "ArtportalenFilterDto";

    private static readonly HashSet<string> PublicSearchFilterSchemaNames =
    [
        "SearchFilterBaseDto",
        "SearchFilterDto",
        "SearchFilterAggregationDto"
    ];

    /// <inheritdoc />
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        RemoveArtportalenFilter(context.DocumentName, swaggerDoc.Components?.Schemas);
    }

    internal static void RemoveArtportalenFilter(
        string documentName,
        IDictionary<string, IOpenApiSchema>? schemas)
    {
        if (!documentName.StartsWith(AzurePublicDocumentPrefix, StringComparison.Ordinal) || schemas == null)
        {
            return;
        }

        foreach (var schemaName in PublicSearchFilterSchemaNames)
        {
            if (schemas.TryGetValue(schemaName, out var schema))
            {
                schema.Properties?.Remove(ArtportalenPropertyName);
            }
        }

        schemas.Remove(ArtportalenFilterSchemaName);
    }
}
