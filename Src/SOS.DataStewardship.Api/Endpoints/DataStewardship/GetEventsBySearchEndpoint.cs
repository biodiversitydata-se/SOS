using Swashbuckle.AspNetCore.Annotations;
using SOS.DataStewardship.Api.Extensions;
using SOS.DataStewardship.Api.Filters;
using SOS.DataStewardship.Api.Validators;
using SOS.DataStewardship.Api.Application.Managers.Interfaces;
using SOS.DataStewardship.Api.Contracts.Enums;
using SOS.DataStewardship.Api.Contracts.Models;

namespace SOS.DataStewardship.Api.Endpoints.DataStewardship;

public class GetEventsBySearchEndpoint : IEndpointDefinition
{
    public void DefineEndpoint(WebApplication app)
    {
        app.MapPost("/datastewardship/events", GetEventsBySearchAsync)
            .Produces<Contracts.Models.PagedResult<EventModel>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            //.Produces<List<FluentValidation.Results.ValidationFailure>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError)
            .AddEndpointFilter<ValidatorFilter<EventsFilter>>()
            .AddEndpointFilter(ValidatePagingParametersAsync);        
    }
   
    [SwaggerOperation(
        Description = "Get events by search.",
        OperationId = "GetEventsBySearch",
        Tags = new[] { "DataStewardship" })]
    private async Task<IResult> GetEventsBySearchAsync(IDataStewardshipManager dataStewardshipManager,
        [FromBody, SwaggerRequestBody("The search filter")] EventsFilter filter,
        [FromQuery, SwaggerParameter("Pagination start index.")] int? skip,
        [FromQuery, SwaggerParameter("Number of items to return. 1000 items is the max to return in one call.")] int? take,
        [FromQuery, SwaggerParameter("The export mode")] ExportMode exportMode = ExportMode.Json,
        [FromQuery, SwaggerParameter("The response coordinate system")] CoordinateSystem responseCoordinateSystem = CoordinateSystem.EPSG4326)
    {                
        var eventModels = await dataStewardshipManager
            .GetEventsBySearchAsync(filter, skip.GetValueOrDefault(0), take.GetValueOrDefault(20), responseCoordinateSystem);

        return exportMode.Equals(ExportMode.Csv) ? 
            Results.File(eventModels.Records.ToCsv(), "text/tab-separated-values", "dataset.csv") : 
            Results.Ok(eventModels);        
    }

    private async ValueTask<object> ValidatePagingParametersAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var pagingParameters = new PagingParameters { Skip = context.GetArgument<int?>(2), Take = context.GetArgument<int?>(3) };
        return await new PagingValidator().ValidateAsync(pagingParameters, context, next);
    }
}