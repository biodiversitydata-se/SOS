using SOS.DataStewardship.Api.Models;
using SOS.DataStewardship.Api.Modules.Interfaces;
using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;
using SOS.DataStewardship.Api.Managers.Interfaces;
using SOS.DataStewardship.Api.Models.Enums;
using SOS.DataStewardship.Api.Extensions;

namespace SOS.DataStewardship.Api.Modules;

public class DataStewardshipModule : IModule
{
    /*
     * API specification
     * =================
     * Complete: https://github.com/Lund-University-Biodiversity-data/datahost-api/blob/main/api/openapi.yaml
     * Template: https://github.com/Lund-University-Biodiversity-data/datahost-api/blob/main/api/templateOpenapi.yaml
     * 
     * Todo
     * ====               
     * 1. Create integration tests
     * 2. Implement Artportalen dataset database tables
     * 3. Implement harvest Artportalen dataset database tables
     * 4. Implement harvest Artportalen data stewardship harvesting.
     * 5. Implement sample event DwC-A data stewardship harvesting.
     * 6. Optimize ES search projection. Now every fields are retrieved just for convenience during development. SearchExtensionsObservation.ToProjection().
     * 7. Implement logging
     * 8. Move SOS.Lib.Models.Processed.Dataset.ObservationDataset.Organisation to SOS.Lib.Models.Processed.Dataset.Organisation?
     */

    public void MapEndpoints(WebApplication application)
    {        
        application.MapGet("/datastewardship/datasets/{id}", GetDatasetByIdAsync)
            .Produces<Dataset>(StatusCodes.Status200OK, "application/json")
            .Produces<Dataset>(StatusCodes.Status200OK, "text/csv")
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError)
            .WithName("GetDatasetById")
            .WithTags("DataStewardship")
            .WithMetadata(new SwaggerOperationAttribute(summary: "", description: "Get dataset by id. Example: ArtportalenDataHost - Dataset Bats"));
            

        application.MapPost("/datastewardship/datasets", GetDatasetsBySearchAsync)
            .Produces<Models.PagedResult<Dataset>>(StatusCodes.Status200OK)
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
            .WithMetadata(new SwaggerOperationAttribute(summary: "", description: "Get event by id. Example: urn:lsid:swedishlifewatch.se:dataprovider:Artportalen:event:10002293427000658739"));

        application.MapPost("/datastewardship/events", GetEventsBySearchAsync)
            .Produces<Models.PagedResult<EventModel>>(StatusCodes.Status200OK)            
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
            .WithMetadata(new SwaggerOperationAttribute(summary: "", description: "Get occurrence by id. Example: urn:lsid:artportalen.se:sighting:98571689"));

        application.MapPost("/datastewardship/occurrences", GetOccurrencesBySearchAsync)
            .Produces<Models.PagedResult<OccurrenceModel>>(StatusCodes.Status200OK, "application/json")
            .Produces<Models.PagedResult<OccurrenceModel>>(StatusCodes.Status200OK, "text/csv")
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
        [FromRoute][Required] string id,
        [FromQuery] ExportMode exportMode = ExportMode.Json)
    {
        try
        {
            var dataset = await dataStewardshipManager.GetDatasetByIdAsync(id);
            if (dataset == null) return Results.NotFound();      
            
            return exportMode.Equals(ExportMode.Csv) ? Results.File(dataset.ToCsv(), "text/tab-separated-values", "dataset.csv") : Results.Ok(dataset);
        }
        catch
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
        [FromQuery] int? take,
        [FromQuery] ExportMode exportMode = ExportMode.Json)
    {
        try
        {
            var datasets = await dataStewardshipManager.GetDatasetsBySearchAsync(filter, skip.GetValueOrDefault(0), take.GetValueOrDefault(20));
            if (!datasets?.Records?.Any() ?? true) return Results.NotFound();

            return exportMode.Equals(ExportMode.Csv) ? Results.File(datasets.Records.ToCsv(), "text/tab-separated-values", "dataset.csv") : Results.Ok(datasets);
        }
        catch
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
        [FromRoute][Required] string id,
        [FromQuery] ExportMode exportMode = ExportMode.Json)
    {
        try
        {
            var eventModel = await dataStewardshipManager.GetEventByIdAsync(id);
            if (eventModel == null) return Results.NotFound();

            return exportMode.Equals(ExportMode.Csv) ? Results.File(eventModel.ToCsv(), "text/tab-separated-values", "dataset.csv") : Results.Ok(eventModel);
        }
        catch
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
        [FromQuery] int? take,
        [FromQuery] ExportMode exportMode = ExportMode.Json)
    {
        try
        {
            var eventModels = await dataStewardshipManager.GetEventsBySearchAsync(filter, skip.GetValueOrDefault(0), take.GetValueOrDefault(20));
            if (!eventModels?.Records?.Any() ?? true) return Results.NotFound();

            return exportMode.Equals(ExportMode.Csv) ? Results.File(eventModels.Records.ToCsv(), "text/tab-separated-values", "dataset.csv") : Results.Ok(eventModels);
        }
        catch
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
        [FromRoute][Required] string id,
        [FromQuery] ExportMode exportMode = ExportMode.Json,
        [FromQuery] CoordinateSystem responseCoordinateSystem = CoordinateSystem.EPSG4326)
    {
        try
        {
            var occurrenceModel = await dataStewardshipManager.GetOccurrenceByIdAsync(id, responseCoordinateSystem);
            if (occurrenceModel == null) return Results.NotFound();
            return exportMode.Equals(ExportMode.Csv) ? Results.File(occurrenceModel.ToCsv(), "text/tab-separated-values", "dataset.csv") : Results.Ok(occurrenceModel);
        }
        catch
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
        [FromQuery] int? take,
        [FromQuery] ExportMode exportMode = ExportMode.Json,
        [FromQuery] CoordinateSystem responseCoordinateSystem = CoordinateSystem.EPSG4326)
    {
        try
        {
            var occurrences = await dataStewardshipManager.GetOccurrencesBySearchAsync(filter, 
                skip.GetValueOrDefault(0), 
                take.GetValueOrDefault(20),
                responseCoordinateSystem);
            return occurrences.Equals(ExportMode.Csv) ? Results.File(occurrences.Records.ToCsv(), "text/tab-separated-values", "dataset.csv") : Results.Ok(occurrences);
        }
        catch
        {
            return Results.BadRequest("Failed");
        }
    }
}