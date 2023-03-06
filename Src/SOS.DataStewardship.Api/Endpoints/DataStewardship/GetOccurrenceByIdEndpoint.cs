using SOS.DataStewardship.Api.Interfaces;
using Swashbuckle.AspNetCore.Annotations;
using SOS.DataStewardship.Api.Managers.Interfaces;
using SOS.DataStewardship.Api.Models.Enums;
using SOS.DataStewardship.Api.Models;
using SOS.DataStewardship.Api.Extensions;

namespace SOS.DataStewardship.Api.Endpoints.DataStewardship;

public class GetOccurrenceByIdEndpoint : IEndpointDefinition
{
    public void DefineEndpoint(WebApplication app)
    {
        app.MapGet("/datastewardship/occurrences/{id}", GetOccurrenceByIdAsync)
            .Produces<OccurrenceModel>(StatusCodes.Status200OK, "application/json")
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest)
            //.Produces<List<FluentValidation.Results.ValidationFailure>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);
    }
    
    [SwaggerOperation(
        Description = "Get occurrence by Id. Example: urn:lsid:artportalen.se:sighting:98571689",
        OperationId = "GetOccurrenceById",
        Tags = new[] { "DataStewardship" })]
    private async Task<IResult> GetOccurrenceByIdAsync(IDataStewardshipManager dataStewardshipManager,
        [FromRoute, SwaggerParameter("The occurrence id", Required = true)] string id,
        [FromQuery, SwaggerParameter("The export mode")] ExportMode exportMode = ExportMode.Json,
        [FromQuery, SwaggerParameter("The response coordinate system")] CoordinateSystem responseCoordinateSystem = CoordinateSystem.EPSG4326)
    {
        var occurrenceModel = await dataStewardshipManager.GetOccurrenceByIdAsync(id, responseCoordinateSystem);
        if (occurrenceModel == null) return Results.NotFound();
        return exportMode.Equals(ExportMode.Csv) ? Results.File(occurrenceModel.ToCsv(), "text/tab-separated-values", "dataset.csv") : Results.Ok(occurrenceModel);
    }
}