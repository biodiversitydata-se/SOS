using Microsoft.AspNetCore.Mvc;
using SOS.Status.Web.Client.Dtos;
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
                       [FromQuery] string translationCultureCode = "sv-SE") =>
                {
                    try
                    {
                        var result = await client.GetTaxonRelationsDiagramAsync(
                            taxonIds,
                            treeIterationMode,
                            includeSecondaryRelations,
                            translationCultureCode);

                        return Results.Ok(result);
                    }
                    catch (ArgumentException ex)
                    {
                        return Results.BadRequest(ex.Message);
                    }
                });


            return app;
        }
    }
}

