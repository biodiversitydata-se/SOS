using SOS.DataStewardship.Api.Interfaces;
using Swashbuckle.AspNetCore.Annotations;
using SOS.DataStewardship.Api.Managers.Interfaces;
using SOS.DataStewardship.Api.Models.Enums;
using SOS.DataStewardship.Api.Models;
using SOS.DataStewardship.Api.Extensions;
using SOS.DataStewardship.Api.Filters;

namespace SOS.DataStewardship.Api.Endpoints.DataStewardship;

public class GetDatasetsBySearchEndpoint : IEndpointDefinition
{
    public void DefineEndpoint(WebApplication app)
    {
        app.MapPost("/datastewardship/datasets", GetDatasetsBySearchAsync)            
            .Produces<Models.PagedResult<Dataset>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            //.Produces<List<FluentValidation.Results.ValidationFailure>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError)
            .AddEndpointFilter<ValidatorFilter<DatasetFilter>>()
            .AddEndpointFilter<ValidatorFilter<PagingParameters>>();
    }

    /// <summary>
    /// Get datasets by search
    /// </summary>
    /// <param name="filter">Filter used to limit the search.</param>
    /// <param name="skip">Start index.</param>
    /// <param name="take">Number of items to return. 1000 items is the max to return in one call.</param>
    /// <returns></returns>
    [SwaggerOperation(
        Description = "Get datasets by search.",
        OperationId = "GetDatasetsBySearch",
        Tags = new[] { "DataStewardship" })]
    internal async Task<IResult> GetDatasetsBySearchAsync(IDataStewardshipManager dataStewardshipManager,
        [FromBody, SwaggerRequestBody("The search filter")] DatasetFilter filter,
        [AsParameters] PagingParameters pagingParameters,
        //[FromQuery] int? skip,
        //[FromQuery] int? take,
        [FromQuery, SwaggerParameter("The export mode")] ExportMode exportMode = ExportMode.Json)
    {
        try
        {
            var datasets = await dataStewardshipManager.GetDatasetsBySearchAsync(filter, pagingParameters.Skip.GetValueOrDefault(0), pagingParameters.Take.GetValueOrDefault(20));
            
            return exportMode.Equals(ExportMode.Csv) ? Results.File(datasets.Records.ToCsv(), "text/tab-separated-values", "dataset.csv") : Results.Ok(datasets);
        }
        catch
        {
            return Results.BadRequest("Failed");
        }
    }
}