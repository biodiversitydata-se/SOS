using SOS.DataStewardship.Api.Extensions;
using SOS.DataStewardship.Api.Filters;
using SOS.DataStewardship.Api.Validators;
using SOS.DataStewardship.Api.Application.Managers.Interfaces;
using SOS.DataStewardship.Api.Contracts.Enums;
using SOS.DataStewardship.Api.Contracts.Models;

namespace SOS.DataStewardship.Api.Endpoints.Events;

public class GetEventsBySearchEndpoint : IEndpointDefinition
{
    public void DefineEndpoint(WebApplication app)
    {
        app.MapPost("/events", GetEventsBySearchAsync)
            .WithName("GetEventsBySearch")
            .WithTags("Events")
            .Produces<Contracts.Models.PagedResult<Contracts.Models.Event>>(StatusCodes.Status200OK)
            .Produces<HttpValidationProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .AddEndpointFilter<ValidatorFilter<EventsFilter>>()
            .AddEndpointFilter(ValidatePagingParametersAsync);
    }

    /// <summary>
    /// Get events by search.
    /// </summary>
    /// <param name="dataStewardshipManager"></param>
    /// <param name="filter">The search filter.</param>
    /// <param name="skip">Pagination start index.</param>
    /// <param name="take">Number of items to return. 1000 items is the max to return in one request.</param>
    /// <param name="exportMode">The export mode.</param>
    /// <param name="responseCoordinateSystem">The response coordinate system.</param>
    /// <returns></returns>
    private async Task<IResult> GetEventsBySearchAsync(IDataStewardshipManager dataStewardshipManager,
        [FromBody] EventsFilter filter,
        [FromQuery] int? skip,
        [FromQuery] int? take,
        [FromQuery] ExportMode exportMode = ExportMode.Json,
        [FromQuery] CoordinateSystem responseCoordinateSystem = CoordinateSystem.EPSG4326)
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