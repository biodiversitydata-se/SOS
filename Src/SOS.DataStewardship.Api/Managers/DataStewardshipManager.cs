using Nest;
using SOS.DataStewardship.Api.Extensions;
using SOS.DataStewardship.Api.Managers.Interfaces;
using SOS.DataStewardship.Api.Models;
using SOS.DataStewardship.Api.Models.SampleData;
using SOS.Lib.JsonConverters;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SOS.DataStewardship.Api.Managers;

public class DataStewardshipManager : IDataStewardshipManager
{    
    private readonly IObservationDatasetRepository _observationDatasetRepository;
    private readonly IProcessedObservationCoreRepository _processedObservationCoreRepository;
    private readonly IFilterManager _filterManager;
    private readonly ILogger<DataStewardshipManager> _logger;

    public DataStewardshipManager(IObservationDatasetRepository observationDatasetRepository,
        IProcessedObservationCoreRepository processedObservationCoreRepository,
        IFilterManager filterManager,
        ILogger<DataStewardshipManager> logger)
    {
        _observationDatasetRepository = observationDatasetRepository;
        _processedObservationCoreRepository = processedObservationCoreRepository;
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

        //return DataStewardshipArtportalenSampleData.DatasetBats;
    }

    public async Task<List<Dataset>> GetDatasetsBySearchAsync(DatasetFilter datasetFilter, int skip, int take)
    {        
        var filter = datasetFilter.ToSearchFilter();
        await _filterManager.PrepareFilterAsync(null, null, filter);
        var datasetIdAggregationItems = await _processedObservationCoreRepository.GetAllAggregationItemsAsync(filter, "dataStewardshipDatasetId");                
        var datasetIdItems = datasetIdAggregationItems
            .Skip(skip)
            .Take(take);
        if (!datasetIdItems.Any()) return new List<Dataset>();
        var observationDatasets = await _observationDatasetRepository.GetDatasetsByIds(datasetIdItems.Select(m => m.AggregationKey));
        var datasets = observationDatasets.Select(m => m.ToDataset()).ToList();
        return datasets;
        
        //return new List<Dataset> { DataStewardshipArtportalenSampleData.DatasetBats };
    }

    public async Task<EventModel> GetEventByIdAsync(string id)
    {
        var filter = new SearchFilter(0);        
        filter.EventIds = new List<string> { id };
        var pageResult = await _processedObservationCoreRepository.GetChunkAsync(filter, 0, 1, true);
        var observation = pageResult.Records.FirstOrDefault();
        Observation obs = CastDynamicToObservation(observation);
        var occurrenceIds = await _processedObservationCoreRepository.GetAllAggregationItemsAsync(filter, "occurrence.occurrenceId");
        var ev = obs.ToEventModel(occurrenceIds.Select(m => m.AggregationKey));
        return ev;
        //return DataStewardshipArtportalenSampleData.EventBats1;
    }    

    public async Task<List<EventModel>> GetEventsBySearchAsync(EventsFilter eventsFilter, int skip, int take)
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

        return events;

        // old implementation
        //var filter = eventsFilter.ToSearchFilter();
        //await _filterManager.PrepareFilterAsync(null, null, filter);
        //var pageResult = await _processedObservationCoreRepository.GetChunkAsync(filter, 0, 10000, true); // todo - when there are more than 10000 observations this solutions is no good.        
        //var observations = CastDynamicsToObservations(pageResult.Records);
        //var observationsByEventId = observations
        //    .GroupBy(m => m.Event.EventId)
        //    .ToDictionary(m => m.Key, m => m.ToList());

        //var events = new List<EventModel>();
        //foreach (var pair in observationsByEventId)
        //{
        //    var eventModel = pair.Value.First().ToEventModel(pair.Value.Select(m => m.Occurrence.OccurrenceId));
        //    events.Add(eventModel);
        //}

        //return events
        //    .Skip(skip)
        //    .Take(take)
        //    .ToList();
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
        //return DataStewardshipArtportalenSampleData.EventBats1Occurrence1;
    }

    public async Task<List<OccurrenceModel>> GetOccurrencesBySearchAsync(OccurrenceFilter occurrenceFilter, int skip, int take)
    {
        var filter = occurrenceFilter.ToSearchFilter();
        await _filterManager.PrepareFilterAsync(null, null, filter);
        var pageResult = await _processedObservationCoreRepository.GetChunkAsync(filter, skip, take, true);
        var observations = CastDynamicsToObservations(pageResult.Records);
        var occurrences = observations.Select(x => x.ToOccurrenceModel()).ToList();
        return occurrences;
        //return new List<OccurrenceModel> {
        //    DataStewardshipArtportalenSampleData.EventBats1Occurrence1,
        //    DataStewardshipArtportalenSampleData.EventBats1Occurrence2,
        //    DataStewardshipArtportalenSampleData.EventBats1Occurrence3,
        //    DataStewardshipArtportalenSampleData.EventBats2Occurrence1,
        //    DataStewardshipArtportalenSampleData.EventBats2Occurrence2,
        //};
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