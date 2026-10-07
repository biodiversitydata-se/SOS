using SOS.Api.Contracts.Common.Geo;
using SOS.Api.Contracts.Aggregations.Requests;
using SOS.Api.Contracts.Common.Responses;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Observations.Api.Managers.Interfaces;
using SOS.Api.Contracts.DataStewardship.Responses;
using SOS.Api.Contracts.Observations.Requests;
using SOS.Lib.Extensions.Dto.DataStewardship;
using SOS.Lib.Extensions.Dto;

namespace SOS.Observations.Api.Managers;


public class DataStewardshipManager : IDataStewardshipManager
{
    private readonly IDatasetRepository _observationDatasetRepository;
    private readonly IEventRepository _observationEventRepository;
    private readonly IProcessedObservationCoreRepository _processedObservationCoreRepository;
    private readonly IFilterManager _filterManager;
    private readonly ILogger<DataStewardshipManager> _logger;

    private readonly ICollection<string> _observationOccurrenceOutputFields = new[]
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

    private async Task<DsEventDto> GetEventByIdFromEventIndexAsync(string id, CoordinateSys responseCoordinateSystem)
    {
        var observationEvents = await _observationEventRepository.GetEventsByIds(new List<string> { id });

        if (!observationEvents?.Any() ?? true) return null;

        var ev = observationEvents.First().ToEventDto(responseCoordinateSystem);
        return ev;
    }

    /// <remarks>
    /// This search uses the Event index.
    /// </remarks>
    private async Task<PagedResultDto<DsEventDto>> GetEventsBySearchFromEventIndexAsync(SearchFilter filter,
        int skip,
        int take,
        CoordinateSys responseCoordinateSystem)
    {
        await _filterManager.PrepareFilterAsync(null, null, filter);
        var eventIdPageResult = await _processedObservationCoreRepository.GetAggregationItemsAsync(filter,
            "event.eventId",
            skip,
            take,
            precisionThreshold: 40000,
            AggregationSortOrder.KeyAscending,
            false,
            true);
        int count = eventIdPageResult.Records.Count();
        int totalCount = Convert.ToInt32(eventIdPageResult.TotalCount);
        var records = Enumerable.Empty<DsEventDto>();
        if (eventIdPageResult.Records.Any())
        {
            var sortOrders = new List<SortOrderFilter>
        {
            new SortOrderFilter { SortBy = "eventId", SortOrder = SearchSortOrder.Asc }
        };

            var observationEvents = await _observationEventRepository.GetEventsByIds(eventIdPageResult.Records.Select(m => m.AggregationKey), sortOrders);
            var events = observationEvents.Select(m => m.ToEventDto(responseCoordinateSystem)).ToList();
            records = events;
        }

        return new PagedResultDto<DsEventDto>()
        {
            Skip = skip,
            Take = take,
            TotalCount = totalCount,
            Records = records.ToList()
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

    public async Task<DsDatasetDto> GetDatasetByIdAsync(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        var observationDataset = await _observationDatasetRepository.GetDatasetsByIds(new string[] { id });
        if (!observationDataset?.Any() ?? true)
        {
            _logger.LogInformation($"Could not find dataset with id: {id}.");
            return null;
        }
        var dataset = observationDataset.FirstOrDefault().ToDto();

        return dataset;
    }

    public async Task<PagedResultDto<DsDatasetDto>> GetDatasetsByFilterAsync(string filter, int skip, int take)
    {
        var sortOrders = new List<SortOrderFilter>
        {
            new SortOrderFilter { SortBy = "identifier", SortOrder = SearchSortOrder.Asc }
        };

        var observationDatasets = await _observationDatasetRepository.GetDatasetsByFilterAsync(
            filter,
            skip,
            take,
            sortOrders: sortOrders);

        return new PagedResultDto<DsDatasetDto>()
        {
            Skip = observationDatasets.Skip,
            Take = observationDatasets.Take,
            TotalCount = observationDatasets.TotalCount,
            Records = observationDatasets.Records?.Select(m => m.ToDto()).ToList() ?? []
        };
    }

    public async Task<PagedResultDto<DsDatasetDto>> GetDatasetsBySearchAsync(SearchFilter filter, int skip, int take)
    {
        await _filterManager.PrepareFilterAsync(null, null, filter);
        var aggregationResult = await _processedObservationCoreRepository.GetAggregationItemsAsync(filter,
            "dataStewardship.datasetIdentifier",
            skip,
            take,
            precisionThreshold: 40000,
            AggregationSortOrder.KeyAscending,
            false,
            true);

        int totalCount = Convert.ToInt32(aggregationResult.TotalCount);
        var records = Enumerable.Empty<DsDatasetDto>();
        if (aggregationResult.Records.Any())
        {
            var sortOrders = new List<SortOrderFilter>
            {
                new SortOrderFilter { SortBy = "identifier", SortOrder = SearchSortOrder.Asc }
            };

            var observationDatasets = await _observationDatasetRepository.GetDatasetsByIds(
                ids: aggregationResult.Records.Select(m => m.AggregationKey),
                excludeFields: filter?.Output?.ExcludeFields,
                sortOrders: sortOrders);
            records = observationDatasets.Select(m => m.ToDto()).ToList();
        }

        return new PagedResultDto<DsDatasetDto>()
        {
            Skip = skip,
            Take = take,
            TotalCount = totalCount,
            Records = records.ToList()
        };
    }

    public async Task<DsEventDto> GetEventByIdAsync(string id, CoordinateSys responseCoordinateSystem)
    {
        var evnt = await GetEventByIdFromEventIndexAsync(id, responseCoordinateSystem);

        if (evnt == null)
        {
            _logger.LogInformation($"Could not find event with id: {id}.");
        }

        return evnt;
    }

    public async Task<PagedResultDto<DsEventDto>> GetEventsBySearchAsync(SearchFilter filter,
        int skip,
        int take,
        CoordinateSys responseCoordinateSystem)
    {
        return await GetEventsBySearchFromEventIndexAsync(filter, skip, take, responseCoordinateSystem);
    }

    public async Task<DsOccurrenceDto> GetOccurrenceByIdAsync(string id, CoordinateSys responseCoordinateSystem)
    {
        var filter = new SearchFilter(0)
        {
            IsPartOfDataStewardshipDataset = true
        };
        filter.Output.Fields = _observationOccurrenceOutputFields?.ToList();            
        var observation = await _processedObservationCoreRepository.GetObservationAsync<Observation>(id, filter, true);

        if (observation == null)
        {
            _logger.LogInformation($"Could not find occurrence with id: {id}.");
            return null!;
        }
        
        var occurrence = observation.ToDto(responseCoordinateSystem);
        return occurrence;
    }

    public async Task<PagedResultDto<DsOccurrenceDto>> GetOccurrencesBySearchAsync(SearchFilter filter, int skip, int take, CoordinateSys responseCoordinateSystem)
    {
        filter.IsPartOfDataStewardshipDataset = true;
        filter.Output.Fields = _observationOccurrenceOutputFields?.ToList();
        await _filterManager.PrepareFilterAsync(null, null, filter);
        var pageResult = await _processedObservationCoreRepository.GetChunkAsync<Observation>(filter, skip, take,
            new SearchExecutionOptions { GetAllFields = true });
        var occurrences = pageResult.Records.Select(observation => observation.ToDto(responseCoordinateSystem)).ToList();

        return new PagedResultDto<DsOccurrenceDto>()
        {
            Skip = skip,
            Take = take,
            TotalCount = Convert.ToInt32(pageResult.TotalCount),
            Records = occurrences
        };
    }
}
