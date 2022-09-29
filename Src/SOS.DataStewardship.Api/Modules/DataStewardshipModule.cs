using SOS.DataStewardship.Api.Models.SampleData;
using SOS.DataStewardship.Api.Models;
using SOS.DataStewardship.Api.Modules.Interfaces;
using System.ComponentModel.DataAnnotations;
using Nest;

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


    public void MapEndpoints(WebApplication application)
    {
        application.MapGet("/datastewardship/datasets/{id}", GetDatasetByIdAsync)
            .Produces<Dataset>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);
        //.WithName("GetAllBooks")
        //.WithTags("Getters");

        application.MapPost("/datastewardship/datasets", GetDatasetsBySearchAsync)
            .Produces<List<Dataset>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);


        application.MapGet("/datastewardship/events/{id}", GetEventsByIdAsync)
            .Produces<EventModel>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);

        application.MapPost("/datastewardship/events", GetEventsBySearchAsync)
            .Produces<List<EventModel>>(StatusCodes.Status200OK)            
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);

        application.MapGet("/datastewardship/occurrences/{id}", GetOccurrenceByIdAsync)
            .Produces<OccurrenceModel>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);

        application.MapPost("/datastewardship/occurrences", GetOccurrencesBySearchAsync)
            .Produces<List<OccurrenceModel>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);
    }

    /// <summary>
    /// Get Dataset by id.
    /// </summary>
    /// <param name="id">The dataset id.</param>
    /// <returns></returns>
    internal async Task<IResult> GetDatasetByIdAsync([FromRoute][Required] string id)
    {
        try
        {
            var datasetExample = DataStewardshipArtportalenSampleData.DatasetBats;
            return Results.Ok(datasetExample);
        }
        catch (Exception ex)
        {
            return Results.BadRequest("Failed");
        }
    }

    /// <summary>
    /// Get datasets by search
    /// </summary>
    /// <param name="body">Filter used to limit the search.</param>
    /// <param name="skip">Start index.</param>
    /// <param name="take">Number of items to return. 1000 items is the max to return in one call.</param>
    /// <returns></returns>
    internal async Task<IResult> GetDatasetsBySearchAsync([FromBody] DatasetFilter body, [FromQuery] int? skip, [FromQuery] int? take)
    {
        try
        {
            var datasetExample = DataStewardshipArtportalenSampleData.DatasetBats;
            return Results.Ok(new List<Dataset> { datasetExample });
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
    internal async Task<IResult> GetEventsByIdAsync([FromRoute][Required] string id)
    {
        try
        {
            var eventExample = DataStewardshipArtportalenSampleData.EventBats1;
            return Results.Ok(eventExample);
        }
        catch (Exception ex)
        {
            return Results.BadRequest("Failed");
        }
    }

    /// <summary>
    /// Get events by search.
    /// </summary>
    /// <param name="body">Filter used to limit the search.</param>
    /// <param name="skip">Start index.</param>
    /// <param name="take">Number of items to return. 1000 items is the max to return in one call.</param>
    /// <returns></returns>
    internal async Task<IResult> GetEventsBySearchAsync([FromBody] EventsFilter body, [FromQuery] int? skip, [FromQuery] int? take)
    {
        try
        {
            return Results.Ok(new List<EventModel>
            {
                DataStewardshipArtportalenSampleData.EventBats1,
                DataStewardshipArtportalenSampleData.EventBats2
            });
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
    internal async Task<IResult> GetOccurrenceByIdAsync([FromRoute][Required] string id)
    {
        try
        {
            var occurrenceExample = DataStewardshipArtportalenSampleData.EventBats1Occurrence1;
            return Results.Ok(occurrenceExample);
        }
        catch (Exception ex)
        {
            return Results.BadRequest("Failed");
        }
    }

    /// <summary>
    /// Get occurrences by search.
    /// </summary>
    /// <param name="body">Filter used to limit the search.</param>
    /// <param name="skip">Start index.</param>
    /// <param name="take">Number of items to return. 1000 items is the max to return in one call.</param>
    /// <returns></returns>
    internal async Task<IResult> GetOccurrencesBySearchAsync([FromBody] EventsFilter body, [FromQuery] int? skip, [FromQuery] int? take)
    {
        try
        {            
            return Results.Ok(new List<OccurrenceModel>
            {
                DataStewardshipArtportalenSampleData.EventBats1Occurrence1,
                DataStewardshipArtportalenSampleData.EventBats1Occurrence2,
                DataStewardshipArtportalenSampleData.EventBats1Occurrence3,
                DataStewardshipArtportalenSampleData.EventBats2Occurrence1,
                DataStewardshipArtportalenSampleData.EventBats2Occurrence2,
            });
        }
        catch (Exception ex)
        {
            return Results.BadRequest("Failed");
        }
    }
}