using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nest;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;
using SOS.Lib.JsonConverters;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Processed.Configuration;
using SOS.Lib.Models.Processed.DataStewardship.Common;
using SOS.Lib.Models.Processed.DataStewardship.Event;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Models.Search.Result;
using SOS.Lib.Repositories.Processed.Interfaces;
using Event = SOS.Lib.Models.Processed.DataStewardship.Event.Event;

namespace SOS.Lib.Repositories.Processed
{
    /// <summary>
    ///     Observation event repository.
    /// </summary>
    public class EventRepository : ProcessRepositoryBase<Event, string>,
        IEventRepository
    {
        private const int ElasticSearchMaxRecords = 10000;

        /// <summary>
        /// Add the collection
        /// </summary>
        /// <returns></returns>
        private async Task<bool> AddCollectionAsync()
        {
            var createIndexResponse = await Client.Indices.CreateAsync(IndexName, s => s
                .Settings(s => s
                    .NumberOfShards(NumberOfShards)
                    .NumberOfReplicas(NumberOfReplicas)
                    .Setting("max_terms_count", 110000)
                    .Setting(UpdatableIndexSettings.MaxResultWindow, 100000)
                )
                .Map<Event>(m => m
                    .AutoMap<Event>()
                    .Properties(ps => ps
                        .KeyWordLowerCase(kwlc => kwlc.Id, false)
                        .KeyWordLowerCase(kwlc => kwlc.EventId)
                        .KeyWordLowerCase(kwlc => kwlc.ParentEventId, false)
                        .KeyWordLowerCase(kwlc => kwlc.EventType, false)
                        .KeyWordLowerCase(kwlc => kwlc.SamplingProtocol, false)
                        .KeyWordLowerCase(kwlc => kwlc.SamplingEffort, false)
                        .KeyWordLowerCase(kwlc => kwlc.SampleSizeValue, false)
                        .KeyWordLowerCase(kwlc => kwlc.SampleSizeUnit, false)
                        .KeyWordLowerCase(kwlc => kwlc.PlainStartDate)
                        .KeyWordLowerCase(kwlc => kwlc.PlainStartTime, false)
                        .KeyWordLowerCase(kwlc => kwlc.PlainEndDate)
                        .KeyWordLowerCase(kwlc => kwlc.PlainEndTime, false)                        
                        .KeyWordLowerCase(kwlc => kwlc.Habitat, false)
                        .Date(d => d
                            .Name(nm => nm.EndDate)
                        )
                        .Date(d => d
                            .Name(nm => nm.StartDate)
                        )
                        .Text(t => t
                            .Name(nm => nm.EventRemarks)
                            .IndexOptions(IndexOptions.Docs)
                        )
                        .Object<Location>(l => l
                            .AutoMap()
                            .Name(nm => nm.Location)
                            .Properties(ps => ps.GetMapping())
                        )
                        .Object<DataStewardshipInfo>(l => l
                            .AutoMap()
                            .Name(nm => nm.DataStewardship)
                            .Properties(ps => ps
                                .KeyWordLowerCase(kwlc => kwlc.DatasetIdentifier)
                                .KeyWordLowerCase(kwlc => kwlc.DatasetTitle)
                            )
                        )
                        .Object<VocabularyValue>(t => t
                            .Name(nm => nm.DiscoveryMethod)
                            .Properties(ps => ps
                                .KeyWordLowerCase(kwlc => kwlc.Value)
                                .Number(nr => nr
                                    .Name(nm => nm.Id)
                                    .Type(NumberType.Integer)
                                )
                            )
                        )
                        .Nested<ExtendedMeasurementOrFact>(n => n
                            .AutoMap()
                            .Name(nm => nm.MeasurementOrFacts)
                            .Properties(ps => ps
                                .KeyWordLowerCase(kwlc => kwlc.OccurrenceID)
                                .KeyWordLowerCase(kwlc => kwlc.MeasurementRemarks, false)
                                .KeyWordLowerCase(kwlc => kwlc.MeasurementAccuracy, false)
                                .KeyWordLowerCase(kwlc => kwlc.MeasurementDeterminedBy, false)
                                .KeyWordLowerCase(kwlc => kwlc.MeasurementDeterminedDate, false)
                                .KeyWordLowerCase(kwlc => kwlc.MeasurementID, false)
                                .KeyWordLowerCase(kwlc => kwlc.MeasurementMethod, false)
                                .KeyWordLowerCase(kwlc => kwlc.MeasurementType, false)
                                .KeyWordLowerCase(kwlc => kwlc.MeasurementTypeID, false)
                                .KeyWordLowerCase(kwlc => kwlc.MeasurementUnit, false)
                                .KeyWordLowerCase(kwlc => kwlc.MeasurementUnitID, false)
                                .KeyWordLowerCase(kwlc => kwlc.MeasurementValue, false)
                                .KeyWordLowerCase(kwlc => kwlc.MeasurementValueID, false)
                            )
                        )
                        .Nested<Multimedia>(n => n
                            .AutoMap()
                            .Name(nm => nm.Media)
                            .Properties(ps => ps
                                .KeyWordLowerCase(kwlc => kwlc.Description, false)
                                .KeyWordLowerCase(kwlc => kwlc.Audience, false)
                                .KeyWordLowerCase(kwlc => kwlc.Contributor, false)
                                .KeyWordLowerCase(kwlc => kwlc.Created, false)
                                .KeyWordLowerCase(kwlc => kwlc.Creator, false)
                                .KeyWordLowerCase(kwlc => kwlc.DatasetID, false)
                                .KeyWordLowerCase(kwlc => kwlc.Format, false)
                                .KeyWordLowerCase(kwlc => kwlc.Identifier, false)
                                .KeyWordLowerCase(kwlc => kwlc.License, false)
                                .KeyWordLowerCase(kwlc => kwlc.Publisher, false)
                                .KeyWordLowerCase(kwlc => kwlc.References, false)
                                .KeyWordLowerCase(kwlc => kwlc.RightsHolder, false)
                                .KeyWordLowerCase(kwlc => kwlc.Source, false)
                                .KeyWordLowerCase(kwlc => kwlc.Title, false)
                                .KeyWordLowerCase(kwlc => kwlc.Type, false)
                            )
                        )
                        .Object<Organisation>(t => t
                            .AutoMap()
                            .Name(nm => nm.RecorderOrganisation)
                            .Properties(ps => ps
                                .KeyWordLowerCase(kwlc => kwlc.OrganisationCode, false)
                                .KeyWordLowerCase(kwlc => kwlc.OrganisationID, false)
                            )
                        )
                        .Object<WeatherVariable>(t => t
                            .AutoMap()
                            .Name(nm => nm.Weather)
                        )                        
                    )
                )
            );
            
            return createIndexResponse.Acknowledged && createIndexResponse.IsValid ? true : throw new Exception($"Failed to create ObservationEvent index. Error: {createIndexResponse.DebugInformation}");
        }

        public async Task<List<Event>> GetEventsByIds(IEnumerable<string> ids, IEnumerable<SortOrderFilter> sortOrders = null)
        {
            if (ids == null || !ids.Any()) throw new ArgumentException("ids is empty");

            var sortDescriptor = await Client.GetSortDescriptorAsync<Event>(IndexName, sortOrders);
            var query = new List<Func<QueryContainerDescriptor<Event>, QueryContainer>>();
            query.TryAddTermsCriteria("eventId", ids);
            var searchResponse = await Client.SearchAsync<Event>(s => s
                .Index(IndexName)
                .Query(q => q
                    .Bool(b => b
                        .Filter(query)
                    )
                )
                .Size(ids?.Count() ?? 0)
                .Sort(sort => sortDescriptor)
                .TrackTotalHits(false)
            );

            if (!searchResponse.IsValid) throw new InvalidOperationException(searchResponse.DebugInformation);
            var events = searchResponse.Documents.ToList();
            return events;
        }

        /// <summary>
        /// Delete collection
        /// </summary>
        /// <returns></returns>
        public async Task<bool> DeleteCollectionAsync()
        {
            var res = await Client.Indices.DeleteAsync(IndexName);
            return res.IsValid;
        }

        /// <summary>
        /// Write data to elastic search
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        private BulkAllObserver WriteToElastic(IEnumerable<Event> items)
        {
            if (!items.Any())
            {
                return null;
            }

            //check
            var currentAllocation = Client.Cat.Allocation();
            if (currentAllocation != null && currentAllocation.IsValid)
            {
                var diskUsageDescription = "Current diskusage in cluster:";
                foreach (var record in currentAllocation.Records)
                {
                    if (int.TryParse(record.DiskPercent, out int percentageUsed))
                    {
                        diskUsageDescription += percentageUsed + "% ";
                        if (percentageUsed > 90)
                        {
                            Logger.LogError($"Disk usage too high in cluster ({percentageUsed}%), aborting indexing");
                            return null;
                        }
                    }
                }

                Logger.LogDebug(diskUsageDescription);
            }

            var count = 0;
            return Client.BulkAll(items, b => b
                    .Index(IndexName)
                    // how long to wait between retries
                    .BackOffTime("30s")
                    // how many retries are attempted if a failure occurs                        .
                    .BackOffRetries(2)
                    // how many concurrent bulk requests to make
                    .MaxDegreeOfParallelism(Environment.ProcessorCount)
                    // number of items per bulk request
                    .Size(WriteBatchSize)
                    .DroppedDocumentCallback((r, o) =>
                    {
                        Logger.LogError($"EventId: {o?.EventId}, Error: {r?.Error?.Reason}");
                    })
                )
                .Wait(TimeSpan.FromDays(1),
                    next => { Logger.LogDebug($"Indexing events for search:{count += next.Items.Count}"); });
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="elasticClientManager"></param>
        /// <param name="elasticConfiguration"></param>
        /// <param name="processedConfigurationCache"></param>
        /// <param name="logger"></param>
        public EventRepository(
            IElasticClientManager elasticClientManager,
            ElasticSearchConfiguration elasticConfiguration,
            ICache<string, ProcessedConfiguration> processedConfigurationCache,
            ILogger<EventRepository> logger) : base(true, elasticClientManager, processedConfigurationCache, elasticConfiguration, logger)
        {
            LiveMode = true;
            _id = nameof(Observation); // The active instance should be the same as the ProcessedObservationRepository which uses the Observation type.
        }

        /// <inheritdoc />
        public async Task<int> AddManyAsync(IEnumerable<Event> items)
        {
            return await Task.Run(() => {
                // Save valid processed data
                Logger.LogDebug($"Start indexing ObservationEvent batch for searching with {items.Count()} items");
                var indexResult = WriteToElastic(items);
                Logger.LogDebug("Finished indexing ObservationEvent batch for searching");
                if (indexResult == null || indexResult.TotalNumberOfFailedBuffers > 0) return 0;
                return items.Count();
            });
        }

        public async Task<bool> DeleteAllDocumentsAsync()
        {
            try
            {
                var res = await Client.DeleteByQueryAsync<Event>(q => q
                    .Index(IndexName)
                    .Query(q => q.MatchAll())
                );

                return res.IsValid;
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<bool> ClearCollectionAsync()
        {
            await DeleteCollectionAsync();
            return await AddCollectionAsync();
        }

        /// <inheritdoc />
        public async Task<bool> DisableIndexingAsync()
        {
            var updateSettingsResponse =
                await Client.Indices.UpdateSettingsAsync(IndexName,
                    p => p.IndexSettings(g => g.RefreshInterval(-1)));

            return updateSettingsResponse.Acknowledged && updateSettingsResponse.IsValid;
        }

        /// <inheritdoc />
        public async Task EnableIndexingAsync()
        {
            await Client.Indices.UpdateSettingsAsync(IndexName,
                p => p.IndexSettings(g => g.RefreshInterval(new Time(5000))));
        }

        /// <inheritdoc />
        public string UniqueIndexName => IndexHelper.GetIndexName<Event>(IndexPrefix, true, LiveMode ? ActiveInstance : InActiveInstance, false);

        /// <inheritdoc />
        public async Task<bool> VerifyCollectionAsync()
        {
            var response = await Client.Indices.ExistsAsync(IndexName);

            if (!response.Exists)
            {
                await AddCollectionAsync();
            }

            return !response.Exists;
        }

        public async Task<List<AggregationItemList<TKey, TValue>>> GetAllAggregationItemsListAsync<TKey, TValue>(EventSearchFilter filter, string aggregationFieldKey, string aggregationFieldList)
        {
            var indexName = IndexName;
            var (query, excludeQuery) = GetCoreQueries(filter);
            var aggregationDictionary = new Dictionary<TKey, List<TValue>>();
            CompositeKey nextPageKey = null;            
            do
            {
                var searchResponse = await PageAggregationItemListAsync(indexName, aggregationFieldKey, aggregationFieldList, query, excludeQuery, nextPageKey, MaxNrElasticSearchAggregationBuckets);
                var compositeAgg = searchResponse.Aggregations.Composite("compositeAggregation");
                foreach (var bucket in compositeAgg.Buckets)
                {
                    TKey keyValue = (TKey)bucket.Key[aggregationFieldKey];
                    TValue listValue = (TValue)bucket.Key[aggregationFieldList];
                    if (!aggregationDictionary.ContainsKey(keyValue))
                        aggregationDictionary[keyValue] = new List<TValue>();
                    aggregationDictionary[keyValue].Add(listValue);
                }

                nextPageKey = compositeAgg.Buckets.Count >= MaxNrElasticSearchAggregationBuckets ? compositeAgg.AfterKey : null;
            } while (nextPageKey != null);

            var items = aggregationDictionary.Select(m => new AggregationItemList<TKey, TValue> { AggregationKey = m.Key, Items = m.Value }).ToList();
            return items;
        }

        private async Task<ISearchResponse<dynamic>> PageAggregationItemListAsync(
            string indexName,
            string aggregationFieldKey,
            string aggregationFieldList,
            ICollection<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> query,
            ICollection<Func<QueryContainerDescriptor<object>, QueryContainer>> excludeQuery,
            CompositeKey nextPage,
            int take)
        {
            ISearchResponse<dynamic> searchResponse;

            searchResponse = await Client.SearchAsync<dynamic>(s => s
                .Index(indexName)
                .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQuery)
                        .Filter(query)
                    )
                )
                .Aggregations(a => a
                    .Composite("compositeAggregation", g => g
                        .After(nextPage ?? null)
                        .Size(take)
                        .Sources(src => src
                            .Terms(aggregationFieldKey, tt => tt
                                .Field(aggregationFieldKey)
                                .Order(SortOrder.Descending)
                            )
                            .Terms(aggregationFieldList, tt => tt
                                .Field(aggregationFieldList)
                            )
                        )
                    )
                )
                .Size(0)
                .Source(s => s.ExcludeAll())
                .TrackTotalHits(false)
            );

            searchResponse.ThrowIfInvalid();
            return searchResponse;
        }

        public async Task<List<AggregationItem>> GetAllAggregationItemsAsync(EventSearchFilter filter, string aggregationField)
        {
            var indexName = IndexName;
            var (query, excludeQuery) = GetCoreQueries(filter);
            var items = new List<AggregationItem>();
            CompositeKey nextPageKey = null;
            var take = MaxNrElasticSearchAggregationBuckets;
            do
            {
                var searchResponse = await PageAggregationItemAsync(indexName, aggregationField, query, excludeQuery, nextPageKey, take);
                var compositeAgg = searchResponse.Aggregations.Composite("compositeAggregation");
                foreach (var bucket in compositeAgg.Buckets)
                {
                    items.Add(new AggregationItem
                    {
                        AggregationKey = bucket.Key["termAggregation"].ToString(),
                        DocCount = Convert.ToInt32(bucket.DocCount.GetValueOrDefault(0))
                    });
                }

                nextPageKey = compositeAgg.Buckets.Count >= take ? compositeAgg.AfterKey : null;
            } while (nextPageKey != null);

            return items;
        }

        private async Task<ISearchResponse<dynamic>> PageAggregationItemAsync(
            string indexName,
            string aggregationField,
            ICollection<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> query,
            ICollection<Func<QueryContainerDescriptor<object>, QueryContainer>> excludeQuery,
            CompositeKey nextPage,
            int take)
        {
            ISearchResponse<dynamic> searchResponse;

            searchResponse = await Client.SearchAsync<dynamic>(s => s
                .Index(indexName)
                .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQuery)
                        .Filter(query)
                    )
                )
                .Aggregations(a => a
                    .Composite("compositeAggregation", g => g
                        .After(nextPage ?? null)
                        .Size(take)
                        .Sources(src => src
                            .Terms("termAggregation", tt => tt
                                .Field(aggregationField)
                            )
                        )
                    )
                )
                .Size(0)
                .Source(s => s.ExcludeAll())
                .TrackTotalHits(false)
            );

            searchResponse.ThrowIfInvalid();

            return searchResponse;
        }
                
        public async Task<PagedResult<dynamic>> GetChunkAsync(EventSearchFilter filter, int skip, int take, bool getAllFields = false)
        {
            string indexName = IndexName;            
            var (query, excludeQuery) = GetCoreQueries(filter);
            var sortDescriptor = await Client.GetSortDescriptorAsync<Event>(indexName, filter.SortOrders);

            var searchResponse = await Client.SearchAsync<dynamic>(s => s
                .Index(indexName)
                .Source(getAllFields ? p => new SourceFilterDescriptor<dynamic>() : filter.OutputIncludeFields.ToProjection(filter.OutputExcludeFields))
                .From(skip)
                .Size(take)
                .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQuery)
                        .Filter(query)
                    )
                )
                .Sort(sort => sortDescriptor)
            );

            searchResponse.ThrowIfInvalid();
            var totalCount = searchResponse.HitsMetadata.Total.Value;
            var includeRealCount = totalCount >= ElasticSearchMaxRecords;            

            if (includeRealCount)
            {
                var countResponse = await Client.CountAsync<dynamic>(s => s
                    .Index(indexName)
                    .Query(q => q
                        .Bool(b => b
                            .MustNot(excludeQuery)
                            .Filter(query)
                        )
                    )
                );
                countResponse.ThrowIfInvalid();
                totalCount = countResponse.Count;
            }

            var records = CastDynamicsToEvents(searchResponse.Documents);

            return new PagedResult<dynamic>
            {
                Records = searchResponse.Documents,                
                Skip = skip,
                Take = take,
                TotalCount = totalCount
            };
        }        

        protected (ICollection<Func<QueryContainerDescriptor<dynamic>, QueryContainer>>, ICollection<Func<QueryContainerDescriptor<object>, QueryContainer>>)
            GetCoreQueries(EventSearchFilter filter)
        {            
            var query = filter.ToQuery<dynamic>();
            var excludeQuery = filter.ToExcludeQuery();

            return (query, excludeQuery);
        }

        /// <summary>
        /// Cast dynamic to observation
        /// </summary>
        /// <param name="dynamicObjects"></param>
        /// <returns></returns>
        public static List<Event> CastDynamicsToEvents(IEnumerable<dynamic> dynamicObjects)
        {
            if (dynamicObjects == null) return null;            

            return JsonSerializer.Deserialize<List<Event>>(
                JsonSerializer.Serialize(dynamicObjects, jsonSerializerOptions), jsonSerializerOptions);
        }

        protected readonly static JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNameCaseInsensitive = true,
            Converters = {
                new JsonStringEnumConverter(),
                new GeoShapeConverter(),
                new NetTopologySuite.IO.Converters.GeoJsonConverterFactory()
            }
        };

        public async Task WaitForIndexCreation(long expectedRecordsCount, TimeSpan? timeout = null)
        {
            Logger.LogInformation($"Begin waiting for index creation. Index={IndexName}, ExpectedRecordsCount={expectedRecordsCount}, Timeout={timeout}");
            if (timeout == null) timeout = TimeSpan.FromMinutes(10);
            var sleepTime = TimeSpan.FromSeconds(5);
            int nrIterations = (int)(Math.Ceiling(timeout.Value.TotalSeconds / sleepTime.TotalSeconds));
            long docCount = await IndexCountAsync();
            var iterations = 0;

            // Compare number of documents processed with actually db count
            // If docCount is less than process count, indexing is not ready yet
            while (docCount < expectedRecordsCount && iterations < nrIterations)
            {
                iterations++; // Safety to prevent infinite loop.                                
                await Task.Delay(sleepTime);
                docCount = await IndexCountAsync();
            }

            Logger.LogInformation($"Finish waiting for index creation. Index={IndexName}.");
        }

        public async Task<long> IndexCountAsync()
        {
            try
            {
                var countResponse = await Client.CountAsync<Event>(s => s
                    .Index(IndexName)
                );

                countResponse.ThrowIfInvalid();
                return countResponse.Count;
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
                return -1;
            }
        }
    }
}