using Microsoft.Extensions.Logging;
using SOS.Lib.Enums;
using SOS.Lib.JsonConverters;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Processed;
using SOS.Observations.Api.Dtos;
using SOS.Observations.Api.Dtos.DataStewardship;
using SOS.Observations.Api.Managers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using SOS.Observations.Api.Dtos.DataStewardship.Extensions;
using SOS.Observations.Api.Extensions;

namespace SOS.Observations.Api.Managers
{

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
            "dataStewardship",
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

        private async Task<EventDto> GetEventByIdFromObservationIndexAsync(string id, CoordinateSys responseCoordinateSystem)
        {
            var filter = new SearchFilter(0);
            filter.EventIds = new List<string> { id };
            filter.Output.Fields = _observationEventOutputFields;

            var pageResult = await _processedObservationCoreRepository.GetChunkAsync(filter, 0, 1, true);
            var observation = pageResult.Records.FirstOrDefault();
            if (observation == null) return null;

            Observation obs = CastDynamicToObservation(observation);
            var occurrenceIds = await _processedObservationCoreRepository.GetAllAggregationItemsAsync(filter, "occurrence.occurrenceId");
            var ev = obs.ToEventDto(occurrenceIds.Select(m => m.AggregationKey), responseCoordinateSystem);
            return ev;
        }

        private async Task<EventDto> GetEventByIdFromEventIndexAsync(string id, CoordinateSys responseCoordinateSystem)
        {
            var observationEvents = await _observationEventRepository.GetEventsByIds(new List<string> { id });

            if (!observationEvents?.Any() ?? true) return null;

            var ev = observationEvents.First().ToEventDto(responseCoordinateSystem);
            return ev;
        }

        /// <remarks>
        /// This search uses the Observation index.
        /// </remarks>
        private async Task<PagedResultDto<EventDto>> GetEventsBySearchFromObservationIndexAsync(SearchFilter filter,
            int skip,
            int take,
            CoordinateSys responseCoordinateSystem)
        {
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
            var events = new List<EventDto>();
            foreach (var observation in observations)
            {
                var occurrenceIds = occurrenceIdsByEventId[observation.Event.EventId.ToLower()];
                var eventModel = observation.ToEventDto(occurrenceIds, responseCoordinateSystem);
                events.Add(eventModel);
            }

            int totalCount = eventOccurrenceIds.Count;
            var records = events;

            return new PagedResultDto<EventDto>()
            {
                Skip = skip,
                Take = take,
                TotalCount = totalCount,
                Records = records
            };
        }

        /// <remarks>
        /// This search uses the Event index.
        /// </remarks>
        private async Task<PagedResultDto<EventDto>> GetEventsBySearchFromEventIndexAsync(SearchFilter filter,
            int skip,
            int take,
            CoordinateSys responseCoordinateSystem)
        {
            await _filterManager.PrepareFilterAsync(null, null, filter);
            var eventIdPageResult = await _processedObservationCoreRepository.GetAggregationItemsAsync(filter,
                "event.eventId",
                skip,
                take,
                Lib.Models.Search.Enums.AggregationSortOrder.KeyAscending);
            int count = eventIdPageResult.Records.Count();
            int totalCount = Convert.ToInt32(eventIdPageResult.TotalCount);
            var records = Enumerable.Empty<EventDto>();
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

            return new PagedResultDto<EventDto>()
            {
                Skip = skip,
                Take = take,
                TotalCount = totalCount,
                Records = records
            };
        }

        private async Task<PagedResultDto<EventDto>> GetEventsBySearchFromEventIndexSortByDateAsync(SearchFilter filter,
            int skip,
            int take,
            CoordinateSys responseCoordinateSystem)
        {
            await _filterManager.PrepareFilterAsync(null, null, filter);
            var records = Enumerable.Empty<EventDto>();
            var allEventIds = await _processedObservationCoreRepository.GetAggregationItemsAsync(filter, "event.eventId");
            int totalCount = allEventIds.Count();
            if (allEventIds.Any())
            {
                EventSearchFilter eventSearchFilter = new EventSearchFilter();
                eventSearchFilter.EventIds = allEventIds.Select(m => m.AggregationKey).ToList();
                eventSearchFilter.SortOrders = new List<SortOrderFilter>
            {
                new SortOrderFilter { SortBy = "startDate", SortOrder = SearchSortOrder.Desc },
                new SortOrderFilter { SortBy = "eventId", SortOrder = SearchSortOrder.Asc }
            };
                var chunk = await _observationEventRepository.GetChunkAsync(eventSearchFilter, skip, take);
                records = EventRepository
                    .CastDynamicsToEvents(chunk.Records)
                    .Select(m => m.ToEventDto(responseCoordinateSystem))
                    .ToList();
            }

            return new PagedResultDto<EventDto>()
            {
                Skip = skip,
                Take = take,
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

        public async Task<DatasetDto> GetDatasetByIdAsync(string id)
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

        public async Task<PagedResultDto<DatasetDto>> GetDatasetsBySearchAsync(SearchFilter filter, int skip, int take)
        {
            await _filterManager.PrepareFilterAsync(null, null, filter);
            var aggregationResult = await _processedObservationCoreRepository.GetAggregationItemsAsync(filter,
                "dataStewardship.datasetIdentifier",
                skip,
                take,
                Lib.Models.Search.Enums.AggregationSortOrder.KeyAscending);

            int totalCount = Convert.ToInt32(aggregationResult.TotalCount);
            var records = Enumerable.Empty<DatasetDto>();
            if (aggregationResult.Records.Any())
            {
                var sortOrders = new List<SortOrderFilter>
                {
                new SortOrderFilter { SortBy = "identifier", SortOrder = SearchSortOrder.Asc }
            };

                var observationDatasets = await _observationDatasetRepository.GetDatasetsByIds(aggregationResult.Records.Select(m => m.AggregationKey), sortOrders);
                records = observationDatasets.Select(m => m.ToDto()).ToList();
            }

            return new PagedResultDto<DatasetDto>()
            {
                Skip = skip,
                Take = take,
                TotalCount = totalCount,
                Records = records
            };
        }

        public async Task<EventDto> GetEventByIdAsync(string id, CoordinateSys responseCoordinateSystem)
        {
            var evnt = await GetEventByIdFromEventIndexAsync(id, responseCoordinateSystem);

            if (evnt == null)
            {
                _logger.LogInformation($"Could not find event with id: {id}.");
            }

            return evnt;
        }

        public async Task<PagedResultDto<EventDto>> GetEventsBySearchAsync(SearchFilter filter,
            int skip,
            int take,
            CoordinateSys responseCoordinateSystem)
        {
            //return await GetEventsBySearchFromEventIndexSortByDateAsync(eventsFilter, skip, take, responseCoordinateSystem);
            return await GetEventsBySearchFromEventIndexAsync(filter, skip, take, responseCoordinateSystem);
            // return await GetEventsBySearchFromObservationIndexAsync(eventsFilter, skip, take);
        }

        public async Task<OccurrenceDto> GetOccurrenceByIdAsync(string id, CoordinateSys responseCoordinateSystem)
        {
            var filter = new SearchFilter(0)
            {
                IsPartOfDataStewardshipDataset = true
            };
            filter.Output.Fields = _observationOccurrenceOutputFields;

            IEnumerable<dynamic> observations = await _processedObservationCoreRepository.GetObservationAsync(id, filter, true);
            var observation = observations?.FirstOrDefault();

            if (observation == null)
            {
                _logger.LogInformation($"Could not find occurrence with id: {id}.");
                return null!;
            }

            Observation obs = CastDynamicToObservation(observation);
            var occurrence = obs.ToDto(responseCoordinateSystem);
            return occurrence;
        }

        public async Task<PagedResultDto<OccurrenceDto>> GetOccurrencesBySearchAsync(SearchFilter filter, int skip, int take, CoordinateSys responseCoordinateSystem)
        {
            filter.IsPartOfDataStewardshipDataset = true;
            filter.Output.Fields = _observationOccurrenceOutputFields;
            await _filterManager.PrepareFilterAsync(null, null, filter);
            var pageResult = await _processedObservationCoreRepository.GetChunkAsync(filter, skip, take, true);
            var observations = CastDynamicsToObservations(pageResult.Records);
            var occurrences = observations.Select(o => o.ToDto(responseCoordinateSystem)).ToList();

            return new PagedResultDto<OccurrenceDto>()
            {
                Skip = skip,
                Take = take,
                TotalCount = Convert.ToInt32(pageResult.TotalCount),
                Records = occurrences
            };
        }
    }
}
