using SOS.DataStewardship.Api.Interfaces;
using Swashbuckle.AspNetCore.Annotations;
using SOS.DataStewardship.Api.Managers.Interfaces;
using SOS.DataStewardship.Api.Models.Enums;
using SOS.DataStewardship.Api.Models;
using SOS.DataStewardship.Api.Extensions;
using SOS.DataStewardship.Api.Filters;
using SOS.DataStewardship.Api.Validators;

namespace SOS.DataStewardship.Api.Endpoints.DataStewardship;

public class GetEventsBySearchEndpoint : IEndpointDefinition
{
    public void DefineEndpoint(WebApplication app)
    {
        app.MapPost("/datastewardship/events", GetEventsBySearchAsync)
            .Produces<Models.PagedResult<EventModel>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            //.Produces<List<FluentValidation.Results.ValidationFailure>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError)
            .AddEndpointFilter<ValidatorFilter<EventsFilter>>();
            //.AddEndpointFilter<ValidatorFilter<PagingParameters>>();
    }
   
    [SwaggerOperation(
        Description = "Get events by search.",
        OperationId = "GetEventsBySearch",
        Tags = new[] { "DataStewardship" })]
    internal async Task<IResult> GetEventsBySearchAsync(IDataStewardshipManager dataStewardshipManager,
        [FromBody, SwaggerRequestBody("The search filter")] EventsFilter filter,
        [FromQuery, SwaggerParameter("Pagination start index.")] int? skip,
        [FromQuery, SwaggerParameter("Number of items to return. 1000 items is the max to return in one call.")] int? take,
        [FromQuery, SwaggerParameter("The export mode")] ExportMode exportMode = ExportMode.Json)
    {
        try
        {
            var pagingValidationResult = await new PagingParameters { Skip = skip, Take = take }.ValidateAsync();
            if (!pagingValidationResult.IsValid)
            {
                return Results.BadRequest(pagingValidationResult.Errors);
            }

            var eventModels = await dataStewardshipManager.GetEventsBySearchAsync(filter, skip.GetValueOrDefault(0), take.GetValueOrDefault(20));

            return exportMode.Equals(ExportMode.Csv) ? Results.File(eventModels.Records.ToCsv(), "text/tab-separated-values", "dataset.csv") : Results.Ok(eventModels);
        }
        catch (Exception ex)
        {
            return Results.BadRequest("Failed");
        }
    }
}