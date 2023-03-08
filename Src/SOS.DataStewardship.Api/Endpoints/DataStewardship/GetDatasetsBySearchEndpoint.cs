using Swashbuckle.AspNetCore.Annotations;
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
        app.MapPost("/datastewardship/datasets", GetDatasetsBySearchAsync)            
            .Produces<Contracts.Models.PagedResult<Dataset>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            //.Produces<List<FluentValidation.Results.ValidationFailure>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError)
            .AddEndpointFilter<ValidatorFilter<DatasetFilter>>()
            .AddEndpointFilter<ValidatorFilter<PagingParameters>>();
    }
    
    [SwaggerOperation(
        Description = "Get datasets by search.",
        OperationId = "GetDatasetsBySearch",
        Tags = new[] { "DataStewardship" })]
    private async Task<IResult> GetDatasetsBySearchAsync(IDataStewardshipManager dataStewardshipManager,
        [FromBody, SwaggerRequestBody("The search filter")] DatasetFilter filter,
        [AsParameters] PagingParameters pagingParameters,        
        [FromQuery, SwaggerParameter("The export mode")] ExportMode exportMode = ExportMode.Json)
    {        
        var datasets = await dataStewardshipManager
            .GetDatasetsBySearchAsync(filter, pagingParameters.Skip.GetValueOrDefault(0), pagingParameters.Take.GetValueOrDefault(20));            
        
        return exportMode.Equals(ExportMode.Csv) ? 
            Results.File(datasets.Records.ToCsv(), "text/tab-separated-values", "dataset.csv") : 
            Results.Ok(datasets);        
    }
}