using Swashbuckle.AspNetCore.Annotations;
using SOS.DataStewardship.Api.Extensions;
using SOS.DataStewardship.Api.Application.Managers.Interfaces;
using SOS.DataStewardship.Api.Contracts.Enums;
using SOS.DataStewardship.Api.Contracts.Models;

namespace SOS.DataStewardship.Api.Endpoints.DataStewardship;

public class GetDatasetByIdEndpoint : IEndpointDefinition
{
    public void DefineEndpoint(WebApplication app)
    {
        app.MapGet("/datasets/{id}", GetDatasetByIdAsync)
            .WithName("GetDatasetById")
            .WithTags("Datasets")
            .Produces<Dataset>(StatusCodes.Status200OK, "application/json")
            .Produces<Dataset>(StatusCodes.Status200OK, "text/csv")
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<HttpValidationProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }

    /// <summary>
    /// Get dataset by id.
    /// </summary>
    /// <remarks>Example: "Artportalen - Fladdermöss"</remarks>
    /// <param name="dataStewardshipManager"></param>
    /// <param name="id">The dataset id.</param>
    /// <param name="exportMode">The export mode.</param>
    /// <returns></returns>
    [SwaggerResponse(404, "Not Found - The requested dataset doesn't exist")]
    private async Task<IResult> GetDatasetByIdAsync(IDataStewardshipManager dataStewardshipManager,
        [FromRoute] string id,
        [FromQuery] ExportMode exportMode = ExportMode.Json)
    {
        var dataset = await dataStewardshipManager.GetDatasetByIdAsync(id);
        if (dataset == null) return NotFoundResult(id);

        return exportMode.Equals(ExportMode.Csv) ? 
            Results.File(dataset.ToCsv(), "text/tab-separated-values", "dataset.csv") : 
            Results.Ok(dataset);        
    }

    private IResult NotFoundResult(string id)
    {
        return Results.Problem(
            type: "https://www.rfc-editor.org/rfc/rfc7231#section-6.5.4",
            title: "Resource not found",
            detail: $"Dataset with identifier={id} was not found.",
            statusCode: StatusCodes.Status404NotFound);
    }
}