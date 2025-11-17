using Microsoft.AspNetCore.Mvc;
using SOS.Status.Web.Client.Dtos.SosObsApi;
using SOS.Status.Web.Services;

namespace SOS.Status.Web.Endpoints;

public static class ObservationSearchEndpoints
{
    extension(IEndpointRouteBuilder app)
    {
        public IEndpointRouteBuilder MapObservationSearchEndpoints()
        {
            app.MapPost("/api/observations/search",
                async ([FromServices] ObservationSearchService service,
                       [FromBody] SearchFilterInternalDto filter,
                       [FromQuery] int skip = 0,
                       [FromQuery] int take = 100,
                       [FromQuery] string sortBy = "",
                       [FromQuery] string sortOrder = "Asc",
                       [FromQuery] bool validateSearchFilter = false,
                       [FromQuery] string translationCultureCode = "sv-SE",
                       [FromQuery] bool sensitiveObservations = false) =>
                {
                    try
                    {
                        var result = await service.SearchObservations(
                            filter,
                            skip,
                            take,
                            sortBy,
                            sortOrder,
                            validateSearchFilter,
                            translationCultureCode,
                            sensitiveObservations);

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

