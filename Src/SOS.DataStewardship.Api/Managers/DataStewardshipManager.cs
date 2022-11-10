using SOS.DataStewardship.Api.Extensions;
using SOS.DataStewardship.Api.Managers.Interfaces;
using SOS.DataStewardship.Api.Models;
using SOS.Lib.JsonConverters;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SOS.DataStewardship.Api.Managers;

public class DataStewardshipManager : IDataStewardshipManager
{    
    private readonly IObservationDatasetRepository _observationDatasetRepository;
    private readonly IObservationEventRepository _observationEventRepository;
    private readonly IProcessedObservationCoreRepository _processedObservationCoreRepository;
    private readonly IFilterManager _filterManager;
    private readonly ILogger<DataStewardshipManager> _logger;

    public DataStewardshipManager(IObservationDatasetRepository observationDatasetRepository,
        IProcessedObservationCoreRepository processedObservationCoreRepository,
        IObservationEventRepository observationEventRepository,
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
        if (observationDataset == null) return null;
        var dataset = observationDataset.FirstOrDefault()?.ToDataset();
        return dataset;
    }

    public async Task<Models.PagedResult<Dataset>> GetDatasetsBySearchAsync(DatasetFilter datasetFilter, int skip, int take)
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
        
        return new Models.PagedResult<Dataset>()
        {
            Skip = skip,
            Take = take,
            Count = count,
            TotalCount = datasetIdAggregationItems.Count(),
            Records = records
        };
    }

    public async Task<EventModel> GetEventByIdAsync(string id)
    {
        // todo - decide if the observation or event index should be used.
        var resFromObs = await GetEventByIdFromObservationIndexAsync(id);
        var resFromEvent = await GetEventByIdFromEventIndexAsync(id);
        return resFromEvent;
    }

    private async Task<EventModel> GetEventByIdFromObservationIndexAsync(string id)
    {
        var filter = new SearchFilter(0);
        filter.EventIds = new List<string> { id };
        var pageResult = await _processedObservationCoreRepository.GetChunkAsync(filter, 0, 1, true);
        var observation = pageResult.Records.FirstOrDefault();
        Observation obs = CastDynamicToObservation(observation);
        var occurrenceIds = await _processedObservationCoreRepository.GetAllAggregationItemsAsync(filter, "occurrence.occurrenceId");
        var ev = obs.ToEventModel(occurrenceIds.Select(m => m.AggregationKey));
        return ev;
    }

    private async Task<EventModel> GetEventByIdFromEventIndexAsync(string id)
    {
        var observationEvents = await _observationEventRepository.GetEventsByIds(new List<string> { id });
        var ev = observationEvents.First().ToEventModel();        
        return ev;
    }

    public async Task<Models.PagedResult<EventModel>> GetEventsBySearchAsync(EventsFilter eventsFilter, int skip, int take)
    {
        // todo - decide if the observation or event index should be used.
        var resFromObs = await GetEventsBySearchFromObservationIndexAsync(eventsFilter, skip, take);
        var resFromEvent = await GetEventsBySearchFromEventIndexAsync(eventsFilter, skip, take);

        return resFromEvent;
    }

    /// <remarks>
    /// This search uses the Observation index.
    /// </remarks>
    private async Task<Models.PagedResult<EventModel>> GetEventsBySearchFromObservationIndexAsync(EventsFilter eventsFilter, int skip, int take)
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
            var eventModel = observation.ToEventModel(occurrenceIds);
            events.Add(eventModel);
        }

        int count = events.Count();
        int totalCount = eventOccurrenceIds.Count;
        var records = events;

        return new Models.PagedResult<EventModel>()
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
    private async Task<Models.PagedResult<EventModel>> GetEventsBySearchFromEventIndexAsync(EventsFilter eventsFilter, int skip, int take)
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
            var events = observationEvents.Select(m => m.ToEventModel()).ToList();
            records = events;
        }
        
        return new Models.PagedResult<EventModel>()
        {
            Skip = skip,
            Take = take,
            Count = count,
            TotalCount = totalCount,
            Records = records
        };
    }

    private readonly List<string> _observationEventOutputFields = new List<string>()
        {
            "occurrence",
            "location",
            "event",
            "dataStewardshipDatasetId",
            "institutionCode",
        };

    public async Task<OccurrenceModel> GetOccurrenceByIdAsync(string id)
    {
        var filter = new SearchFilter(0);                
        IEnumerable<dynamic> observations = await _processedObservationCoreRepository.GetObservationAsync(id, filter, true);
        var observation = observations.FirstOrDefault();
        Observation obs = CastDynamicToObservation(observation);
        var occurrence = obs.ToOccurrenceModel();
        return occurrence;
    }

    public async Task<Models.PagedResult<OccurrenceModel>> GetOccurrencesBySearchAsync(OccurrenceFilter occurrenceFilter, int skip, int take)
    {
        var filter = occurrenceFilter.ToSearchFilter();
        await _filterManager.PrepareFilterAsync(null, null, filter);
        var pageResult = await _processedObservationCoreRepository.GetChunkAsync(filter, skip, take, true);
        var observations = CastDynamicsToObservations(pageResult.Records);
        var occurrences = observations.Select(x => x.ToOccurrenceModel()).ToList();

        return new Models.PagedResult<OccurrenceModel>()
        {
            Skip = skip,
            Take = take,
            Count = occurrences.Count,
            TotalCount = Convert.ToInt32(pageResult.TotalCount),
            Records = occurrences
        };
    }

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
}