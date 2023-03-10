﻿using SOS.DataStewardship.Api.Application.Managers.Interfaces;
using SOS.DataStewardship.Api.Contracts.Enums;
using SOS.DataStewardship.Api.Contracts.Models;
using SOS.DataStewardship.Api.Extensions;
using SOS.Lib.JsonConverters;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SOS.DataStewardship.Api.Application.Managers;

public class DataStewardshipManager : IDataStewardshipManager
{
    private readonly IDatasetRepository _observationDatasetRepository;
    private readonly IEventRepository _observationEventRepository;
    private readonly IProcessedObservationCoreRepository _processedObservationCoreRepository;
    private readonly IFilterManager _filterManager;
    private readonly ILogger<DataStewardshipManager> _logger;

    private JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            new JsonStringEnumConverter(),
            new GeoShapeConverter(),
            new NetTopologySuite.IO.Converters.GeoJsonConverterFactory()
        }
    };

    private readonly ICollection<string> _observationEventOutputFields = new[]
       {
            "dataStewardshipDatasetId",
            "event.eventId",
            "event.parentEventId",
            "event.eventRemarks",
            "event.media",
            "event.startDate",
            "event.endDate",
            "event.SamplingProtocol",
            "institutionCode",
            "institutionId",
            "location",
            "occurrence.RecordedBy"
        };

    private readonly ICollection<string> _observationOccurrenceOutputFields = new[]
    {
        "basisOfRecord",
        "dataStewardshipDatasetId",
        "event.endDate",
        "event.eventId",
        "event.startDate",
        "location.coordinateUncertaintyInMeters",
        "location.point",
        "occurrence.isPositiveObservation",
        "occurrence.activity",
        "occurrence.lifeStage",
        "occurrence.media",
        "occurrence.occurrenceId",
        "occurrence.occurrenceRemarks",
        "occurrence.organismQuantityInt",
        "occurrence.organismQuantityUnit",
        "occurrence.sex",
        "taxon"
    };

    private List<Observation> CastDynamicsToObservations(IEnumerable<dynamic> dynamicObjects)
    {
        if (dynamicObjects == null) return null;
        return JsonSerializer.Deserialize<List<Observation>>(JsonSerializer.Serialize(dynamicObjects, _jsonSerializerOptions), _jsonSerializerOptions);
    }

    private Observation CastDynamicToObservation(dynamic dynamicObject)
    {
        if (dynamicObject == null) return null;
        return JsonSerializer.Deserialize<Observation>(JsonSerializer.Serialize(dynamicObject, _jsonSerializerOptions), _jsonSerializerOptions);
    }

    private async Task<EventModel> GetEventByIdFromObservationIndexAsync(string id, CoordinateSystem responseCoordinateSystem)
    {
        var filter = new SearchFilter(0);
        filter.EventIds = new List<string> { id };
        filter.Output.Fields = _observationEventOutputFields;

        var pageResult = await _processedObservationCoreRepository.GetChunkAsync(filter, 0, 1, true);
        var observation = pageResult.Records.FirstOrDefault();
        if (observation == null) return null;

        Observation obs = CastDynamicToObservation(observation);
        var occurrenceIds = await _processedObservationCoreRepository.GetAllAggregationItemsAsync(filter, "occurrence.occurrenceId");
        var ev = obs.ToEventModel(occurrenceIds.Select(m => m.AggregationKey), responseCoordinateSystem);
        return ev;
    }

    private async Task<EventModel> GetEventByIdFromEventIndexAsync(string id, CoordinateSystem responseCoordinateSystem)
    {
        var observationEvents = await _observationEventRepository.GetEventsByIds(new List<string> { id });

        if (!observationEvents?.Any() ?? true) return null;

        var ev = observationEvents.First().ToEventModel(responseCoordinateSystem);
        return ev;
    }

    /// <remarks>
    /// This search uses the Observation index.
    /// </remarks>
    private async Task<Contracts.Models.PagedResult<EventModel>> GetEventsBySearchFromObservationIndexAsync(EventsFilter eventsFilter, 
        int skip, 
        int take,
        CoordinateSystem responseCoordinateSystem)
    {
        var filter = eventsFilter.ToSearchFilter();
        await _filterManager.PrepareFilterAsync(null, null, filter);
        var eventOccurrenceIds = await _processedObservationCoreRepository.GetEventOccurrenceItemsAsync(filter);
        var occurrenceIdsByEventId = eventOccurrenceIds.ToDictionary(m => m.EventId, m => m.OccurrenceIds);
        var eventIds = occurrenceIdsByEventId
            .OrderBy(m => m.Key) // todo - support sorting by other properties?
            .Skip(skip)
            .Take(take)
            .ToList();

        var firstOccurrenceIdInEvents = eventIds.Select(m => m.Value.First());
        var observations = await _processedObservationCoreRepository.GetObservationsAsync(firstOccurrenceIdInEvents, _observationEventOutputFields, false);
        var events = new List<EventModel>();
        foreach (var observation in observations)
        {
            var occurrenceIds = occurrenceIdsByEventId[observation.Event.EventId.ToLower()];
            var eventModel = observation.ToEventModel(occurrenceIds, responseCoordinateSystem);
            events.Add(eventModel);
        }

        int count = events.Count();
        int totalCount = eventOccurrenceIds.Count;
        var records = events;

        return new Contracts.Models.PagedResult<EventModel>()
        {
            Skip = skip,
            Take = take,
            Count = count,
            TotalCount = totalCount,
            Records = records
        };
    }

    /// <remarks>
    /// This search uses the Event index.
    /// </remarks>
    private async Task<Contracts.Models.PagedResult<EventModel>> GetEventsBySearchFromEventIndexAsync(EventsFilter eventsFilter, 
        int skip, 
        int take, 
        CoordinateSystem responseCoordinateSystem)
    {
        var filter = eventsFilter.ToSearchFilter();
        await _filterManager.PrepareFilterAsync(null, null, filter);
        var eventIdPageResult = await _processedObservationCoreRepository.GetAggregationItemsAsync(filter, "event.eventId", "event.startDate", skip, take);
        int count = eventIdPageResult.Records.Count();
        int totalCount = Convert.ToInt32(eventIdPageResult.TotalCount);
        var records = Enumerable.Empty<EventModel>();
        if (eventIdPageResult.Records.Any())
        {
            var observationEvents = await _observationEventRepository.GetEventsByIds(eventIdPageResult.Records.Select(m => m.AggregationKey));
            var events = observationEvents.Select(m => m.ToEventModel(responseCoordinateSystem)).ToList();
            records = events;
        }

        return new Contracts.Models.PagedResult<EventModel>()
        {
            Skip = skip,
            Take = take,
            Count = count,
            TotalCount = totalCount,
            Records = records
        };
    }

    public DataStewardshipManager(IDatasetRepository observationDatasetRepository,
        IProcessedObservationCoreRepository processedObservationCoreRepository,
        IEventRepository observationEventRepository,
        IFilterManager filterManager,
        ILogger<DataStewardshipManager> logger)
    {
        _observationDatasetRepository = observationDatasetRepository;
        _processedObservationCoreRepository = processedObservationCoreRepository;
        _observationEventRepository = observationEventRepository;
        _filterManager = filterManager;
        _logger = logger;
    }

    public async Task<Dataset> GetDatasetByIdAsync(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        var observationDataset = await _observationDatasetRepository.GetDatasetsByIds(new string[] { id });
        if (!observationDataset?.Any() ?? true)
        {
            _logger.LogInformation($"Could not find dataset with id: {id}.");
            return null;
        }
        var dataset = observationDataset.FirstOrDefault().ToDataset();

        return dataset;
    }

    public async Task<Contracts.Models.PagedResult<Dataset>> GetDatasetsBySearchAsync(DatasetFilter datasetFilter, int skip, int take)
    {
        var filter = datasetFilter.ToSearchFilter();
        await _filterManager.PrepareFilterAsync(null, null, filter);
        var datasetIdAggregationItems = await _processedObservationCoreRepository.GetAllAggregationItemsAsync(filter, "dataStewardshipDatasetId");
        var datasetIdItems = datasetIdAggregationItems
            .Skip(skip)
            .Take(take);

        int count = datasetIdItems.Count();
        int totalCount = datasetIdAggregationItems.Count();
        var records = Enumerable.Empty<Dataset>();
        if (datasetIdItems.Any())
        {
            var observationDatasets = await _observationDatasetRepository.GetDatasetsByIds(datasetIdItems.Select(m => m.AggregationKey));
            records = observationDatasets.Select(m => m.ToDataset()).ToList();
        }

        return new Contracts.Models.PagedResult<Dataset>()
        {
            Skip = skip,
            Take = take,
            Count = count,
            TotalCount = totalCount,
            Records = records
        };
    }

    public async Task<EventModel> GetEventByIdAsync(string id, CoordinateSystem responseCoordinateSystem)
    {
        // todo - decide if the observation or event index should be used.
        //var evnt = await GetEventByIdFromObservationIndexAsync(id);
        var evnt = await GetEventByIdFromEventIndexAsync(id, responseCoordinateSystem);

        if (evnt == null)
        {
            _logger.LogInformation($"Could not find event with id: {id}.");
        }

        return evnt;
    }

    public async Task<Contracts.Models.PagedResult<EventModel>> GetEventsBySearchAsync(EventsFilter eventsFilter, 
        int skip, 
        int take, 
        CoordinateSystem responseCoordinateSystem)
    {
        // todo - decide if the observation or event index should be used.
        //  var resFromObs = await GetEventsBySearchFromObservationIndexAsync(eventsFilter, skip, take);
        var resFromEvent = await GetEventsBySearchFromEventIndexAsync(eventsFilter, skip, take, responseCoordinateSystem);

        return resFromEvent;
    }

    public async Task<OccurrenceModel> GetOccurrenceByIdAsync(string id, CoordinateSystem responseCoordinateSystem)
    {
        var filter = new SearchFilter(0);
        filter.Output.Fields = _observationOccurrenceOutputFields;

        IEnumerable<dynamic> observations = await _processedObservationCoreRepository.GetObservationAsync(id, filter, true);
        var observation = observations?.FirstOrDefault();

        if (observation == null)
        {
            _logger.LogInformation($"Could not find occurrence with id: {id}.");
            return null!;
        }

        Observation obs = CastDynamicToObservation(observation);
        var occurrence = obs.ToOccurrenceModel(responseCoordinateSystem);
        return occurrence;
    }

    public async Task<Contracts.Models.PagedResult<OccurrenceModel>> GetOccurrencesBySearchAsync(OccurrenceFilter occurrenceFilter, int skip, int take, CoordinateSystem responseCoordinateSystem)
    {
        var filter = occurrenceFilter.ToSearchFilter();
        filter.Output.Fields = _observationOccurrenceOutputFields;
        await _filterManager.PrepareFilterAsync(null, null, filter);
        var pageResult = await _processedObservationCoreRepository.GetChunkAsync(filter, skip, take, true);
        var observations = CastDynamicsToObservations(pageResult.Records);
        var occurrences = observations.Select(x => x.ToOccurrenceModel(responseCoordinateSystem)).ToList();

        return new Contracts.Models.PagedResult<OccurrenceModel>()
        {
            Skip = skip,
            Take = take,
            Count = occurrences.Count,
            TotalCount = Convert.ToInt32(pageResult.TotalCount),
            Records = occurrences
        };
    }
}