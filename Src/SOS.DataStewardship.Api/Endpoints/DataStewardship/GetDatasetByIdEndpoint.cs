using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using SOS.DataStewardship.Api.Extensions;
using Newtonsoft.Json;
using SOS.DataStewardship.Api.Application.Managers.Interfaces;
using SOS.DataStewardship.Api.Contracts.Enums;
using SOS.DataStewardship.Api.Contracts.Models;

namespace SOS.DataStewardship.Api.Endpoints.DataStewardship;

public class GetDatasetByIdEndpoint : IEndpointDefinition
{
    public void DefineEndpoint(WebApplication app)
    {
        app.MapGet("/datastewardship/datasets/{id}", GetDatasetByIdAsync)
            .Produces<Dataset>(StatusCodes.Status200OK, "application/json")
            .Produces<Dataset>(StatusCodes.Status200OK, "text/csv")
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<HttpValidationProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }
    
    [SwaggerOperation(        
        Description = "Get dataset by id. Example: Artportalen - Fladdermöss",
        OperationId = "GetDatasetById",            
        Tags = new[] { "DataStewardship" })]
    [SwaggerResponse(404, "Not Found - The requested dataset doesn't exist")]
    private async Task<IResult> GetDatasetByIdAsync(IDataStewardshipManager dataStewardshipManager,
        [FromRoute, SwaggerParameter("The dataset id", Required = true)][Required] string id,
        [FromQuery, SwaggerParameter("The export mode")] ExportMode exportMode = ExportMode.Json)
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