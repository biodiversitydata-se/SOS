using SOS.Api.Contracts.Observations.Requests;
using Microsoft.AspNetCore.Mvc;
using SOS.Api.Contracts.Taxonomy.Requests;
using SOS.Status.Web.Services;

namespace SOS.Status.Web.Endpoints;

public static class TaxonDiagramEndpoints
{
    extension(IEndpointRouteBuilder app)
    {
        public IEndpointRouteBuilder MapTaxonDiagramEndpoints()
        {
            app.MapGet("/api/taxon-diagram",
                async ([FromServices] TaxonDiagramService client,
                       [FromQuery] int[] taxonIds,
                       [FromQuery] TaxonRelationsTreeIterationMode treeIterationMode = TaxonRelationsTreeIterationMode.BothParentsAndChildren,
                       [FromQuery] bool includeSecondaryRelations = true,
                       [FromQuery] TaxonomyType taxonomyType = TaxonomyType.Dyntaxa,
                       [FromQuery] string translationCultureCode = "sv-SE") =>
                {
                    try
                    {
                        var result = await client.GetTaxonRelationsDiagramAsync(
                            taxonIds,
                            treeIterationMode,
                            includeSecondaryRelations,
                            taxonomyType,
                            translationCultureCode);

                        return result is null
                            ? Results.NoContent()
                            : Results.Text(result, "text/plain");
                    }
                    catch (ArgumentException ex)
                    {
                        return Results.Text(ex.Message, "text/plain", statusCode: StatusCodes.Status400BadRequest);
                    }
                });


            return app;
        }
    }
}

