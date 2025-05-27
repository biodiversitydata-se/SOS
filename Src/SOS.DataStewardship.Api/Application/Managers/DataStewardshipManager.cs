using SOS.DataStewardship.Api.Application.Managers.Interfaces;
using SOS.DataStewardship.Api.Contracts.Enums;
using SOS.DataStewardship.Api.Contracts.Models;
using SOS.DataStewardship.Api.Extensions;

namespace SOS.DataStewardship.Api.Application.Managers;

public class DataStewardshipManager : IDataStewardshipManager
{
    private readonly IDatasetRepository _observationDatasetRepository;
    private readonly IEventRepository _observationEventRepository;
    private readonly IProcessedObservationCoreRepository _processedObservationCoreRepository;
    private readonly IFilterManager _filterManager;
    private readonly ILogger<DataStewardshipManager> _logger;

    private readonly List<string> _observationOccurrenceOutputFields = new List<string>
    {
        "basisOfRecord",
        "dataStewardship",
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

    private async Task<Contracts.Models.Event> GetEventByIdFromEventIndexAsync(string id, CoordinateSystem responseCoordinateSystem)
    {
        var observationEvents = await _observationEventRepository.GetEventsByIds(new List<string> { id });

        if (!observationEvents?.Any() ?? true) return null;

        var ev = observationEvents.First().ToEventModel(responseCoordinateSystem);
        return ev;
    }

    /// <remarks>
    /// This search uses the Event index.
    /// </remarks>
    private async Task<PagedResult<Contracts.Models.Event>> GetEventsBySearchFromEventIndexAsync(EventsFilter eventsFilter, 
        int skip, 
        int take, 
        CoordinateSystem responseCoordinateSystem)
    {        
        var filter = eventsFilter.ToSearchFilter();
        await _filterManager.PrepareFilterAsync(null, null, filter);
        var eventIdPageResult = await _processedObservationCoreRepository.GetAggregationItemsAsync(filter, 
            "event.eventId", 
            skip, 
            take,
            precisionThreshold: null,
            Lib.Models.Search.Enums.AggregationSortOrder.KeyAscending,
            aggregateCardinality: true);
        int count = eventIdPageResult.Records.Count();
        int totalCount = Convert.ToInt32(eventIdPageResult.TotalCount);
        var records = Enumerable.Empty<Contracts.Models.Event>();
        if (eventIdPageResult.Records.Any())
        {
            var sortOrders = new List<SortOrderFilter>
            {
                new SortOrderFilter { SortBy = "eventId", SortOrder = SearchSortOrder.Asc }
            };

            var observationEvents = await _observationEventRepository.GetEventsByIds(eventIdPageResult.Records.Select(m => m.AggregationKey), sortOrders);
            var events = observationEvents.Select(m => m.ToEventModel(responseCoordinateSystem)).ToList();
            records = events;
        }        

        return new Contracts.Models.PagedResult<Contracts.Models.Event>()
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
        var aggregationResult = await _processedObservationCoreRepository.GetAggregationItemsAsync(filter, 
            "dataStewardship.datasetIdentifier",
            skip,
            take,
            precisionThreshold: null,
            Lib.Models.Search.Enums.AggregationSortOrder.KeyAscending,
            aggregateCardinality: true);        
        int count = aggregationResult.Records.Count();
        int totalCount = Convert.ToInt32(aggregationResult.TotalCount);
        var records = Enumerable.Empty<Dataset>();
        if (aggregationResult.Records.Any())
        {
            var sortOrders = new List<SortOrderFilter>
            {
                new SortOrderFilter { SortBy = "identifier", SortOrder = SearchSortOrder.Asc }                
            };

            var observationDatasets = await _observationDatasetRepository.GetDatasetsByIds(aggregationResult.Records.Select(m => m.AggregationKey), sortOrders: sortOrders);
            records = observationDatasets.Select(m => m.ToDataset()).ToList();
        }

        return new PagedResult<Dataset>()
        {
            Skip = skip,
            Take = take,
            Count = count,
            TotalCount = totalCount,
            Records = records
        };
    }

    public async Task<Contracts.Models.Event> GetEventByIdAsync(string id, CoordinateSystem responseCoordinateSystem)
    {        
        var evnt = await GetEventByIdFromEventIndexAsync(id, responseCoordinateSystem);

        if (evnt == null)
        {
            _logger.LogInformation($"Could not find event with id: {id}.");
        }

        return evnt;
    }

    public async Task<PagedResult<Contracts.Models.Event>> GetEventsBySearchAsync(EventsFilter eventsFilter, 
        int skip, 
        int take, 
        CoordinateSystem responseCoordinateSystem)
    {
        //return await GetEventsBySearchFromEventIndexSortByDateAsync(eventsFilter, skip, take, responseCoordinateSystem);
         return await GetEventsBySearchFromEventIndexAsync(eventsFilter, skip, take, responseCoordinateSystem);
        // return await GetEventsBySearchFromObservationIndexAsync(eventsFilter, skip, take);
    }

    public async Task<Contracts.Models.Occurrence> GetOccurrenceByIdAsync(string id, CoordinateSystem responseCoordinateSystem)
    {
        var filter = new SearchFilter(0) {
            IsPartOfDataStewardshipDataset = true
        };
        filter.Output.Fields = _observationOccurrenceOutputFields;

        var observation = await _processedObservationCoreRepository.GetObservationAsync<Observation>(id, filter, true);

        if (observation == null)
        {
            _logger.LogInformation($"Could not find occurrence with id: {id}.");
            return null!;
        }

        var occurrence = observation.ToOccurrenceModel(responseCoordinateSystem);
        return occurrence;
    }

    public async Task<PagedResult<Contracts.Models.Occurrence>> GetOccurrencesBySearchAsync(OccurrenceFilter occurrenceFilter, int skip, int take, CoordinateSystem responseCoordinateSystem)
    {
        var filter = occurrenceFilter.ToSearchFilter();
        filter.IsPartOfDataStewardshipDataset = true;
        filter.Output.Fields = _observationOccurrenceOutputFields;
        await _filterManager.PrepareFilterAsync(null, null, filter);
        var pageResult = await _processedObservationCoreRepository.GetChunkAsync<Observation>(filter, skip, take, true);
        var observations = pageResult.Records;
        var occurrences = observations.Select(x => x.ToOccurrenceModel(responseCoordinateSystem)).ToList();

        return new PagedResult<Contracts.Models.Occurrence>()
        {
            Skip = skip,
            Take = take,
            Count = occurrences.Count,
            TotalCount = Convert.ToInt32(pageResult.TotalCount),
            Records = occurrences
        };
    }
}