using SOS.DataStewardship.Api.Models;
using SOS.DataStewardship.Api.Modules.Interfaces;
using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;
using SOS.DataStewardship.Api.Managers.Interfaces;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SOS.DataStewardship.Api.Modules;

public class DataStewardshipModule : IModule
{
    /*
     * Todo
     * ====
     * 1. Create sample request-response for Artportalen data
     * 2. Create sample request-response for other data provider
     * 3. Harvest additional Artportalen metadata
     * 4. Implement controller actions.
     * 5. Create integration tests         
     */

    private JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
    {
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };
                   

    public void MapEndpoints(WebApplication application)
    {        
        application.MapGet("/datastewardship/datasets/{id}", GetDatasetByIdAsync)
            .Produces<Dataset>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError)
            .WithName("GetDatasetById")
            .WithTags("DataStewardship")
            .WithMetadata(new SwaggerOperationAttribute(summary: "", description: "Get dataset by id."));
            

        application.MapPost("/datastewardship/datasets", GetDatasetsBySearchAsync)
            .Produces<List<Dataset>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError)
            .WithName("GetDatasetsBySearch")
            .WithTags("DataStewardship")
            .WithMetadata(new SwaggerOperationAttribute(summary: "", description: "Get datasets by search."));


        application.MapGet("/datastewardship/events/{id}", GetEventByIdAsync)
            .Produces<EventModel>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError)
            .WithName("GetEventById")
            .WithTags("DataStewardship")
            .WithMetadata(new SwaggerOperationAttribute(summary: "", description: "Get event by id."));

        application.MapPost("/datastewardship/events", GetEventsBySearchAsync)
            .Produces<List<EventModel>>(StatusCodes.Status200OK)            
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError)
            .WithName("GetEventsBySearch")
            .WithTags("DataStewardship")
            .WithMetadata(new SwaggerOperationAttribute(summary: "", description: "Get events by search."));

        application.MapGet("/datastewardship/occurrences/{id}", GetOccurrenceByIdAsync)
            .Produces<OccurrenceModel>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError)
            .WithName("GetOccurrenceById")
            .WithTags("DataStewardship")
            .WithMetadata(new SwaggerOperationAttribute(summary: "", description: "Get occurrence by id."));

        application.MapPost("/datastewardship/occurrences", GetOccurrencesBySearchAsync)
            .Produces<List<OccurrenceModel>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError)
            .WithName("GetOccurrencesBySearch")
            .WithTags("DataStewardship")
            .WithMetadata(new SwaggerOperationAttribute(summary: "", description: "Get occurrences by search."));
    }

    /// <summary>
    /// Get Dataset by id.
    /// </summary>
    /// <param name="id">The dataset id.</param>
    /// <returns></returns>
    internal async Task<IResult> GetDatasetByIdAsync(IDataStewardshipManager dataStewardshipManager, 
        [FromRoute][Required] string id)
    {
        try
        {
            var dataset = await dataStewardshipManager.GetDatasetByIdAsync(id);
            if (dataset == null) return Results.NotFound();
            return Results.Json(dataset, _jsonSerializerOptions);
            //return Results.Ok(dataset); // The Json setup in Progam.cs doesn't seem to work. Why?
        }
        catch (Exception ex)
        {
            return Results.BadRequest("Failed");
        }
    }

    /// <summary>
    /// Get datasets by search
    /// </summary>
    /// <param name="filter">Filter used to limit the search.</param>
    /// <param name="skip">Start index.</param>
    /// <param name="take">Number of items to return. 1000 items is the max to return in one call.</param>
    /// <returns></returns>
    internal async Task<IResult> GetDatasetsBySearchAsync(IDataStewardshipManager dataStewardshipManager, 
        [FromBody] DatasetFilter filter, 
        [FromQuery] int? skip, 
        [FromQuery] int? take)
    {
        try
        {
            var datasets = await dataStewardshipManager.GetDatasetsBySearchAsync(filter, skip.GetValueOrDefault(0), take.GetValueOrDefault(20));
            return Results.Json(datasets, _jsonSerializerOptions);
            //return Results.Ok(datasets); // The Json setup in Progam.cs doesn't seem to work. Why?
        }
        catch (Exception ex)
        {
            return Results.BadRequest("Failed");
        }
    }

    /// <summary>
    /// Get event by ID
    /// </summary>
    /// <param name="id">EventId of the event to get.</param>
    /// <returns></returns>
    internal async Task<IResult> GetEventByIdAsync(IDataStewardshipManager dataStewardshipManager, 
        [FromRoute][Required] string id)
    {
        try
        {
            var eventModel = await dataStewardshipManager.GetEventByIdAsync(id);
            if (eventModel == null) return Results.NotFound();
            return Results.Json(eventModel, _jsonSerializerOptions);
            //return Results.Ok(dataset); // The Json setup in Progam.cs doesn't seem to work. Why?
        }
        catch (Exception ex)
        {
            return Results.BadRequest("Failed");
        }
    }

    /// <summary>
    /// Get events by search.
    /// </summary>
    /// <param name="filter">Filter used to limit the search.</param>
    /// <param name="skip">Start index.</param>
    /// <param name="take">Number of items to return. 1000 items is the max to return in one call.</param>
    /// <returns></returns>
    internal async Task<IResult> GetEventsBySearchAsync(IDataStewardshipManager dataStewardshipManager, 
        [FromBody] EventsFilter filter,
        [FromQuery] int? skip, 
        [FromQuery] int? take)
    {
        try
        {
            var eventModels = await dataStewardshipManager.GetEventsBySearchAsync(filter, skip.GetValueOrDefault(0), take.GetValueOrDefault(20));
            return Results.Json(eventModels, _jsonSerializerOptions);
            //return Results.Ok(eventModels); // The Json setup in Progam.cs doesn't seem to work. Why?
        }
        catch (Exception ex)
        {
            return Results.BadRequest("Failed");
        }
    }

    /// <summary>
    /// Get occurrence by ID
    /// </summary>
    /// <param name="id">OccurrenceId of the occurrence to get.</param>
    /// <returns></returns>
    internal async Task<IResult> GetOccurrenceByIdAsync(IDataStewardshipManager dataStewardshipManager, 
        [FromRoute][Required] string id)
    {
        try
        {
            var occurrenceModel = await dataStewardshipManager.GetOccurrenceByIdAsync(id);
            if (occurrenceModel == null) return Results.NotFound();            
            return Results.Json(occurrenceModel, _jsonSerializerOptions);
            //return Results.Ok(occurrenceModel); // The Json setup in Progam.cs doesn't seem to work. Why?
        }
        catch (Exception ex)
        {
            return Results.BadRequest("Failed");
        }
    }

    /// <summary>
    /// Get occurrences by search.
    /// </summary>
    /// <param name="filter">Filter used to limit the search.</param>
    /// <param name="skip">Start index.</param>
    /// <param name="take">Number of items to return. 1000 items is the max to return in one call.</param>
    /// <returns></returns>
    internal async Task<IResult> GetOccurrencesBySearchAsync(IDataStewardshipManager dataStewardshipManager, 
        [FromBody] OccurrenceFilter filter, 
        [FromQuery] int? skip, 
        [FromQuery] int? take)
    {
        try
        {
            var occurrences = await dataStewardshipManager.GetOccurrencesBySearchAsync(filter, 
                skip.GetValueOrDefault(0), 
                take.GetValueOrDefault(20));

            return Results.Ok(occurrences);            
        }
        catch (Exception ex)
        {
            return Results.BadRequest("Failed");
        }
    }
}