using Swashbuckle.AspNetCore.Annotations;
using SOS.DataStewardship.Api.Extensions;
using SOS.DataStewardship.Api.Filters;
using SOS.DataStewardship.Api.Validators;
using SOS.DataStewardship.Api.Application.Managers.Interfaces;
using SOS.DataStewardship.Api.Contracts.Enums;
using SOS.DataStewardship.Api.Contracts.Models;

namespace SOS.DataStewardship.Api.Endpoints.DataStewardship;

public class GetOccurrencesBySearchEndpoint : IEndpointDefinition
{
    public void DefineEndpoint(WebApplication app)
    {
        app.MapPost("/datastewardship/occurrences", GetOccurrencesBySearchAsync)
            .Produces<Contracts.Models.PagedResult<OccurrenceModel>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            //.Produces<List<FluentValidation.Results.ValidationFailure>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError)
            .AddEndpointFilter<ValidatorFilter<OccurrenceFilter>>()
            .AddEndpointFilter(ValidatePagingParametersAsync);
    }
    
    [SwaggerOperation(
        Description = "Get occurrences by search.",
        OperationId = "GetOccurrencesBySearch",
        Tags = new[] { "DataStewardship" })]
    internal async Task<IResult> GetOccurrencesBySearchAsync(IDataStewardshipManager dataStewardshipManager,
        [FromBody, SwaggerRequestBody("The search filter")] OccurrenceFilter filter,
        [FromQuery, SwaggerParameter("Pagination start index.")] int? skip,
        [FromQuery, SwaggerParameter("Number of items to return. 1000 items is the max to return in one call.")] int? take,
        [FromQuery, SwaggerParameter("The export mode")] ExportMode exportMode = ExportMode.Json,
        [FromQuery, SwaggerParameter("The response coordinate system")] CoordinateSystem responseCoordinateSystem = CoordinateSystem.EPSG4326)
    {
        var occurrences = await dataStewardshipManager.GetOccurrencesBySearchAsync(filter,
            skip.GetValueOrDefault(0),
            take.GetValueOrDefault(20),
            responseCoordinateSystem);
        
        return occurrences.Equals(ExportMode.Csv) ? Results.File(occurrences.Records.ToCsv(), "text/tab-separated-values", "dataset.csv") : Results.Ok(occurrences);
    }

    private async ValueTask<object> ValidatePagingParametersAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var pagingParameters = new PagingParameters { Skip = context.GetArgument<int?>(2), Take = context.GetArgument<int?>(3) };
        return await new PagingValidator().ValidateAsync(pagingParameters, context, next);
    }
}