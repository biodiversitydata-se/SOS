using Swashbuckle.AspNetCore.Annotations;
using SOS.DataStewardship.Api.Extensions;
using SOS.DataStewardship.Api.Application.Managers.Interfaces;
using SOS.DataStewardship.Api.Contracts.Enums;

namespace SOS.DataStewardship.Api.Endpoints.Events;

public class GetEventByIdEndpoint : IEndpointDefinition
{
    public void DefineEndpoint(WebApplication app)
    {
        app.MapGet("/events/{id}", GetEventByIdAsync)
            .WithName("GetEventById")
            .WithTags("Events")
            .Produces<Contracts.Models.Event>(StatusCodes.Status200OK, "application/json")
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<HttpValidationProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }

    /// <summary>
    /// Get event by Id.
    /// </summary>
    /// <remarks>Example: "urn:lsid:swedishlifewatch.se:dataprovider:Artportalen:event:10002293427000658739"</remarks>
    /// <param name="dataStewardshipManager"></param>
    /// <param name="id">The event id.</param>
    /// <param name="exportMode">The export mode.</param>
    /// <param name="responseCoordinateSystem">The response coordinate system.</param>
    /// <returns></returns>
    [SwaggerResponse(404, "Not Found - The requested event doesn't exist")]
    private async Task<IResult> GetEventByIdAsync(IDataStewardshipManager dataStewardshipManager,
        [FromRoute] string id,
        [FromQuery] ExportMode exportMode = ExportMode.Json,
        [FromQuery] CoordinateSystem responseCoordinateSystem = CoordinateSystem.EPSG4326)
    {
        var eventModel = await dataStewardshipManager.GetEventByIdAsync(id, responseCoordinateSystem);
        if (eventModel == null) return NotFoundResult(id);

        return exportMode.Equals(ExportMode.Csv) ? Results.File(eventModel.ToCsv(), "text/tab-separated-values", "dataset.csv") : Results.Ok(eventModel);
    }

    private IResult NotFoundResult(string id)
    {
        return Results.Problem(
            type: "https://www.rfc-editor.org/rfc/rfc7231#section-6.5.4",
            title: "Resource not found",
            detail: $"Event with eventId={id} was not found.",
            statusCode: StatusCodes.Status404NotFound);
    }
}