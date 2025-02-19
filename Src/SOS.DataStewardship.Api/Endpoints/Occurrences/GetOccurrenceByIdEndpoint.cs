﻿using Swashbuckle.AspNetCore.Annotations;
using SOS.DataStewardship.Api.Extensions;
using SOS.DataStewardship.Api.Application.Managers.Interfaces;
using SOS.DataStewardship.Api.Contracts.Enums;

namespace SOS.DataStewardship.Api.Endpoints.Occurrences;

public class GetOccurrenceByIdEndpoint : IEndpointDefinition
{
    public void DefineEndpoint(WebApplication app)
    {
        app.MapGet("/occurrences/{id}", GetOccurrenceByIdAsync)
            .Produces<Contracts.Models.Occurrence>(StatusCodes.Status200OK, "application/json")
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<HttpValidationProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }

    [SwaggerOperation(
        Description = "Get occurrence by Id. Example: urn:lsid:artportalen.se:sighting:98571689",
        OperationId = "GetOccurrenceById",
        Tags = new[] { "Occurrences" })]
    private async Task<IResult> GetOccurrenceByIdAsync(IDataStewardshipManager dataStewardshipManager,
        [FromRoute, SwaggerParameter("The occurrence id", Required = true)] string id,
        [FromQuery, SwaggerParameter("The export mode")] ExportMode exportMode = ExportMode.Json,
        [FromQuery, SwaggerParameter("The response coordinate system")] CoordinateSystem responseCoordinateSystem = CoordinateSystem.EPSG4326)
    {
        var occurrenceModel = await dataStewardshipManager.GetOccurrenceByIdAsync(id, responseCoordinateSystem);
        if (occurrenceModel == null) return NotFoundResult(id);
        return exportMode.Equals(ExportMode.Csv) ? Results.File(occurrenceModel.ToCsv(), "text/tab-separated-values", "dataset.csv") : Results.Ok(occurrenceModel);
    }

    private IResult NotFoundResult(string id)
    {
        return Results.Problem(
            type: "https://www.rfc-editor.org/rfc/rfc7231#section-6.5.4",
            title: "Resource not found",
            detail: $"Occurrence with occurrenceId={id} was not found.",
            statusCode: StatusCodes.Status404NotFound);
    }
}