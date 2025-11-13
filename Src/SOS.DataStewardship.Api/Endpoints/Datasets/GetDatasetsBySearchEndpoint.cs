using SOS.DataStewardship.Api.Extensions;
using SOS.DataStewardship.Api.Filters;
using SOS.DataStewardship.Api.Application.Managers.Interfaces;
using SOS.DataStewardship.Api.Contracts.Enums;
using SOS.DataStewardship.Api.Contracts.Models;

namespace SOS.DataStewardship.Api.Endpoints.DataStewardship;

public class GetDatasetsBySearchEndpoint : IEndpointDefinition
{
    public void DefineEndpoint(WebApplication app)
    {
        app.MapPost("/datasets", GetDatasetsBySearchAsync)
            .WithName("GetDatasetsBySearch")
            .WithTags("Datasets")
            .Produces<Contracts.Models.PagedResult<Dataset>>(StatusCodes.Status200OK)
            .Produces<HttpValidationProblemDetails>(StatusCodes.Status400BadRequest)            
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .AddEndpointFilter<ValidatorFilter<DatasetFilter>>()
            .AddEndpointFilter<ValidatorFilter<PagingParameters>>();
    }

    /// <summary>
    /// Get datasets by search.
    /// </summary>
    /// <param name="dataStewardshipManager"></param>
    /// <param name="filter">The search filter.</param>
    /// <param name="pagingParameters"></param>
    /// <param name="exportMode">The export mode.</param>
    /// <returns></returns>
    private async Task<IResult> GetDatasetsBySearchAsync(IDataStewardshipManager dataStewardshipManager,
        [FromBody] DatasetFilter filter,
        [AsParameters] PagingParameters pagingParameters,        
        [FromQuery] ExportMode exportMode = ExportMode.Json)
    {
        var datasets = await dataStewardshipManager
            .GetDatasetsBySearchAsync(filter, pagingParameters.Skip.GetValueOrDefault(0), pagingParameters.Take.GetValueOrDefault(20));            
        
        return exportMode.Equals(ExportMode.Csv) ? 
            Results.File(datasets.Records.ToCsv(), "text/tab-separated-values", "dataset.csv") : 
            Results.Ok(datasets);        
    }
}