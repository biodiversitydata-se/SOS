using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Elasticsearch.Net;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Nest;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Exceptions;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.DarwinCore;
using SOS.Lib.Models.DataQuality;
using SOS.Lib.Models.Gis;
using SOS.Lib.Models.Processed.AggregatedResult;
using SOS.Lib.Models.Processed.Configuration;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Processed.Interfaces;
using DateTime = System.DateTime;
using Result = CSharpFunctionalExtensions.Result;
using Area = SOS.Lib.Models.Processed.Observation.Area;
using SOS.Lib.Enums.VocabularyValues;

namespace SOS.Lib.Repositories.Processed
{
    /// <summary>
    ///     Species data service
    /// </summary>
    public class ProcessedObservationRepository : ProcessRepositoryBase<Observation, string>,
        IProcessedObservationRepository
    {                
        private const int ElasticSearchMaxRecords = 10000;
        private readonly IElasticClientManager _elasticClientManager;
        private readonly ElasticSearchConfiguration _elasticConfiguration;
        private readonly TelemetryClient _telemetry;
        private IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Http context accessor.
        /// </summary>
        public IHttpContextAccessor HttpContextAccessor
        {
            get => _httpContextAccessor;
            set => _httpContextAccessor = value;
        }

        private IElasticClient Client => _elasticClientManager.Clients.Length == 1 ? _elasticClientManager.Clients.FirstOrDefault() : _elasticClientManager.Clients[CurrentInstance];

        private IElasticClient InActiveClient => _elasticClientManager.Clients.Length == 1 ? _elasticClientManager.Clients.FirstOrDefault() : _elasticClientManager.Clients[InActiveInstance];

        /// <summary>
        /// Add the collection
        /// </summary>
        /// <param name="protectedIndex"></param>
        /// <returns></returns>
        private async Task<bool> AddCollectionAsync(bool protectedIndex)
        {
            var createIndexResponse = await Client.Indices.CreateAsync(protectedIndex ? ProtectedIndexName : PublicIndexName, s => s
                .IncludeTypeName(false)
                .Settings(s => s
                    .NumberOfShards(_elasticConfiguration.NumberOfShards)
                    .NumberOfReplicas(_elasticConfiguration.NumberOfReplicas)
                    .Setting("max_terms_count", 110000)
                    .Setting(UpdatableIndexSettings.MaxResultWindow, 100000)
                )
                .Map<Observation>(m => m
                    .AutoMap()
                    .Properties(ps => ps
                        .Keyword(kw => kw
                            .Name(nm => nm.DatasetName)
                        )
                        .Object<Location>(l => l
                            .Name(nm => nm.Location)
                            .Properties(ps => ps
                                .GeoShape(gs => gs
                                    .Name(nn => nn.Point)
                                )
                                .GeoPoint(gp => gp
                                    .Name(nn => nn.PointLocation)
                                )
                                .GeoShape(gs => gs
                                    .Name(nn => nn.PointWithBuffer)
                                )
                                .GeoShape(gs => gs
                                    .Name(nn => nn.PointWithDisturbanceBuffer)
                                )
                                .Object<Area>(c => c
                                    .Name(nm => nm.County)
                                    .Properties(ps => ps
                                        .Keyword(kw => kw
                                            .Name(nm => nm.Name)
                                        )
                                    )
                                )
                                .Object<Area>(c => c
                                    .Name(nm => nm.Municipality)
                                    .Properties(ps => ps
                                        .Keyword(kw => kw
                                            .Name(nm => nm.Name)
                                        )
                                    )
                                )
                                .Object<Area>(c => c
                                    .Name(nm => nm.Parish)
                                    .Properties(ps => ps
                                        .Keyword(kw => kw
                                            .Name(nm => nm.Name)
                                        )
                                    )
                                )
                                .Object<Area>(c => c
                                    .Name(nm => nm.Province)
                                    .Properties(ps => ps
                                        .Keyword(kw => kw
                                            .Name(nm => nm.Name)
                                        )
                                    )
                                )
                            )
                        )
                        .Object<Occurrence>(t => t
                            .Name(nm => nm.Occurrence)
                            .Properties(ps => ps
                                .Object<VocabularyValue>(c => c
                                    .Name(nm => nm.OccurrenceStatus)
                                    .Properties(ps => ps
                                        .Keyword(kw => kw
                                            .Name(nm => nm.Value)
                                        )
                                    )
                                )
                            )
                        )
                        .Object<Taxon>(t => t
                            .Name(nm => nm.Taxon)
                            .Properties(ps => ps
                                .Object<TaxonAttributes>(c => c
                                    .Name(nm => nm.Attributes)
                                    .Properties(ps => ps
                                        .Keyword(kw => kw
                                            .Name(nm => nm.OrganismGroup)
                                        )
                                        .Keyword(kw => kw
                                            .Name(nm => nm.RedlistCategory)
                                        )
                                        .Keyword(kw => kw
                                            .Name(nm => nm.SwedishHistory)
                                        )
                                    )
                                )
                            )
                        )
                    )
                )
            );

            return createIndexResponse.Acknowledged && createIndexResponse.IsValid;
        }

        /// <summary>
        /// Add geo tile taxon result to dictionary
        /// </summary>
        /// <param name="compositeAgg"></param>
        /// <param name="taxaByGeoTile"></param>
        /// <returns></returns>
        private static int AddGeoTileTaxonResultToDictionary(
            CompositeBucketAggregate compositeAgg,
            Dictionary<string, Dictionary<int, long?>> taxaByGeoTile)
        {
            foreach (var bucket in compositeAgg.Buckets)
            {
                var geoTile = (string)bucket.Key["geoTile"];
                var taxonId = Convert.ToInt32((long)bucket.Key["taxon"]);
                if (!taxaByGeoTile.ContainsKey(geoTile)) taxaByGeoTile.Add(geoTile, new Dictionary<int, long?>());
                taxaByGeoTile[geoTile].Add(taxonId, bucket.DocCount);
            }

            return compositeAgg.Buckets.Count;
        }

        /// <summary>
        /// Cast dynamic to observation
        /// </summary>
        /// <param name="dynamicObjects"></param>
        /// <returns></returns>
        private List<Observation> CastDynamicsToObservations(IEnumerable<dynamic> dynamicObjects)
        {
            if (dynamicObjects == null) return null;
            return JsonSerializer.Deserialize<List<Observation>>(JsonSerializer.Serialize(dynamicObjects),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        /// <summary>
        /// Delete collection
        /// </summary>
        /// <param name="protectedIndex"></param>
        /// <returns></returns>
        private async Task<bool> DeleteCollectionAsync(bool protectedIndex)
        {
            var res = await Client.Indices.DeleteAsync(protectedIndex ? ProtectedIndexName : PublicIndexName);
            return res.IsValid;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="indexName"></param>
        /// <param name="query"></param>
        /// <param name="excludeQuery"></param>
        /// <returns></returns>
        private async Task<Dictionary<int, int>> GetAllObservationCountByTaxonIdAsync(
            string indexName,
            ICollection<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> query,
            ICollection<Func<QueryContainerDescriptor<object>, QueryContainer>> excludeQuery)
        {
            var observationCountByTaxonId = new Dictionary<int, int>();
            CompositeKey nextPageKey = null;
            var pageTaxaAsyncTake = MaxNrElasticSearchAggregationBuckets;
            do
            {
                var searchResponse = await PageTaxaCompositeAggregationAsync(indexName, query, excludeQuery, nextPageKey, pageTaxaAsyncTake);
                var compositeAgg = searchResponse.Aggregations.Composite("taxonComposite");
                foreach (var bucket in compositeAgg.Buckets)
                {
                    var taxonId = Convert.ToInt32((long)bucket.Key["taxonId"]);
                    observationCountByTaxonId.Add(taxonId, Convert.ToInt32(bucket.DocCount.GetValueOrDefault(0)));
                }

                nextPageKey = compositeAgg.Buckets.Count >= pageTaxaAsyncTake ? compositeAgg.AfterKey : null;
            } while (nextPageKey != null);

            return observationCountByTaxonId;
        }

        /// <summary>
        /// Get core queries
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        private Tuple<ICollection<Func<QueryContainerDescriptor<dynamic>, QueryContainer>>, 
            ICollection<Func<QueryContainerDescriptor<object>, QueryContainer>>> GetCoreQueries(SearchFilterBase filter)
        {
            var query = filter.ToQuery();
            var excludeQuery = filter.ToExcludeQuery();

            return new Tuple<ICollection<Func<QueryContainerDescriptor<dynamic>, QueryContainer>>, 
                ICollection<Func<QueryContainerDescriptor<object>, QueryContainer>>>(query, excludeQuery);
        }

        /// <summary>
        /// Aggregate observations by GeoTile and Taxon.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="excludeQuery"></param>
        /// <param name="zoom">The precision to use in the GeoTileGrid aggregation.</param>
        /// <param name="nextPage">The key is a combination of GeoTile string and TaxonId. Should be null in the first request.</param>
        /// <returns></returns>
        private async Task<ISearchResponse<dynamic>> PageGeoTileAndTaxaAsync(
            ICollection<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> query,
            ICollection<Func<QueryContainerDescriptor<object>, QueryContainer>> excludeQuery,
            int zoom,
            CompositeKey nextPage)
        {
            ISearchResponse<dynamic> searchResponse;

            if (nextPage == null) // First request
            {
                searchResponse = await Client.SearchAsync<dynamic>(s => s
                    .Index(PublicIndexName)
                    .Size(0)
                    .Aggregations(a => a.Composite("geoTileTaxonComposite", g => g
                        .Size(MaxNrElasticSearchAggregationBuckets + 1)
                        .Sources(src => src
                            .GeoTileGrid("geoTile", h => h
                                .Field("location.pointLocation")
                                .Precision((GeoTilePrecision)zoom).Order(SortOrder.Ascending))
                            .Terms("taxon", tt => tt
                                .Field("taxon.id").Order(SortOrder.Ascending)
                            ))))
                    .Query(q => q
                        .Bool(b => b
                            .MustNot(excludeQuery)
                            .Filter(query)
                        )
                    ));
            }
            else
            {
                searchResponse = await Client.SearchAsync<dynamic>(s => s
                    .Index(PublicIndexName)
                    .Size(0)
                    .Aggregations(a => a.Composite("geoTileTaxonComposite", g => g
                        .Size(MaxNrElasticSearchAggregationBuckets + 1)
                        .After(nextPage)
                        .Sources(src => src
                            .GeoTileGrid("geoTile", h => h
                                .Field("location.pointLocation")
                                .Precision((GeoTilePrecision)zoom).Order(SortOrder.Ascending))
                            .Terms("taxon", tt => tt
                                .Field("taxon.id").Order(SortOrder.Ascending)
                            ))))
                    .Query(q => q
                        .Bool(b => b
                            .MustNot(excludeQuery)
                            .Filter(query)
                        )
                    ));
            }

            if (!searchResponse.IsValid)
            {
                throw new InvalidOperationException(searchResponse.DebugInformation);
            }

            return searchResponse;
        }

        private async Task<ISearchResponse<dynamic>> PageTaxaCompositeAggregationAsync(
            string indexName,
            ICollection<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> query,
            ICollection<Func<QueryContainerDescriptor<object>, QueryContainer>> excludeQuery,
            CompositeKey nextPage,
            int take)
        {
            ISearchResponse<dynamic> searchResponse;

            if (nextPage == null) // First request
            {
                searchResponse = await Client.SearchAsync<dynamic>(s => s
                    .Index(indexName)
                    .Size(0)
                    .Aggregations(a => a.Composite("taxonComposite", g => g
                        .Size(take)
                        .Sources(src => src
                            .Terms("taxonId", tt => tt
                                .Field("taxon.id")
                            ))))
                    .Query(q => q
                        .Bool(b => b
                            .MustNot(excludeQuery)
                            .Filter(query)
                        )
                    ));
            }
            else
            {
                searchResponse = await Client.SearchAsync<dynamic>(s => s
                    .Index(indexName)
                    .Size(0)
                    .Aggregations(a => a.Composite("taxonComposite", g => g
                        .Size(take)
                        .After(nextPage)
                        .Sources(src => src
                            .Terms("taxonId", tt => tt
                                .Field("taxon.id")
                            ))))
                    .Query(q => q
                        .Bool(b => b
                            .MustNot(excludeQuery)
                            .Filter(query)
                        )
                    ));
            }

            if (!searchResponse.IsValid)
            {
                throw new InvalidOperationException(searchResponse.DebugInformation);
            }

            return searchResponse;
        }

        /// <summary>
        /// Get public index name and also protected index name if user is authorized
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        private string GetCurrentIndex(SearchFilterBase filter)
        {
            if (((filter?.ExtendedAuthorization.ObservedByMe ?? false) || (filter?.ExtendedAuthorization.ReportedByMe ?? false) || (filter?.ExtendedAuthorization.ProtectedObservations ?? false)) &&
                (filter?.ExtendedAuthorization.UserId ?? 0) == 0)
            {
                throw new AuthenticationRequiredException("Not authenticated");
            }

            if (!filter?.ExtendedAuthorization.ProtectedObservations ?? true)
            {
                return PublicIndexName;
            }

            if (!_httpContextAccessor?.HttpContext?.User?.HasAccessToScope(_elasticConfiguration.ProtectedScope) ?? true)
            {
                throw new AuthenticationRequiredException("Not authorized");
            }

            return ProtectedIndexName;
        }

        /// <summary>
        /// Get last modified date for provider
        /// </summary>
        /// <param name="providerId"></param>
        /// <param name="protectedIndex"></param>
        /// <returns></returns>
        private async Task<DateTime> GetLatestModifiedDateForProviderAsync(int providerId, bool protectedIndex)
        {
            try
            {
                var res = await Client.SearchAsync<Observation>(s => s
                    .Index(protectedIndex ? ProtectedIndexName : PublicIndexName)
                    .Query(q => q
                        .Term(t => t
                            .Field(f => f.DataProviderId)
                            .Value(providerId)))
                    .Aggregations(a => a
                        .Max("latestModified", m => m
                            .Field(f => f.Modified)
                        )
                    )
                );

                var epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                return epoch.AddMilliseconds(res.Aggregations?.Max("latestModified")?.Value ?? 0).ToUniversalTime();
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Failed to get last modified date for provider: { providerId }, index: { (protectedIndex ? ProtectedIndexName : PublicIndexName) }");
                return DateTime.MinValue;
            }
        }

        private async Task<ScrollResult<Observation>> ScrollObservationsWithCompleteObjectAsync(int dataProviderId, bool protectedIndex,
            string scrollId)
        {
            ISearchResponse<Observation> searchResponse;
            if (string.IsNullOrEmpty(scrollId))
            {
                searchResponse = await Client
                    .SearchAsync<Observation>(s => s
                        .Index(protectedIndex ? ProtectedIndexName : PublicIndexName)
                        .Query(query => query.Term(term => term.Field(obs => obs.DataProviderId).Value(dataProviderId)))
                        .Sort(s => s.Ascending(new Field("_doc")))
                        .Scroll(_elasticConfiguration.ScrollTimeout)
                        .Size(_elasticConfiguration.ScrollBatchSize)
                    );
            }
            else
            {
                searchResponse = await Client
                    .ScrollAsync<Observation>(_elasticConfiguration.ScrollTimeout, scrollId);
            }

            return new ScrollResult<Observation>
            {
                Records = searchResponse.Documents,
                ScrollId = searchResponse.ScrollId,
                TotalCount = searchResponse.HitsMetadata.Total.Value
            };
        }

        /// <summary>
        /// Write data to elastic search
        /// </summary>
        /// <param name="items"></param>
        /// <param name="protectedIndex"></param>
        /// <returns></returns>
        private BulkAllObserver WriteToElastic(IEnumerable<Observation> items, bool protectedIndex)
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
                    .Index(protectedIndex ? ProtectedIndexName : PublicIndexName)
                    // how long to wait between retries
                    .BackOffTime("30s")
                    // how many retries are attempted if a failure occurs                        .
                    .BackOffRetries(2)
                    // how many concurrent bulk requests to make
                    .MaxDegreeOfParallelism(Environment.ProcessorCount)
                    // number of items per bulk request
                    .Size(1000)
                    .DroppedDocumentCallback((r, o) =>
                    {
                        Logger.LogError($"OccurrenceId: {o?.Occurrence?.OccurrenceId}, Error: {r.Error.Reason}");
                    })
                )
                .Wait(TimeSpan.FromDays(1),
                    next => { Logger.LogDebug($"Indexing item for search:{count += next.Items.Count}"); });
        }

        /// <summary>
        /// Constructor used in public mode
        /// </summary>
        /// <param name="elasticClientManager"></param>
        /// <param name="client"></param>
        /// <param name="elasticConfiguration"></param>
        /// <param name="processedConfigurationCache"></param>
        /// <param name="telemetry"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="logger"></param>
        public ProcessedObservationRepository(
            IElasticClientManager elasticClientManager,
            IProcessClient client,
            ElasticSearchConfiguration elasticConfiguration,
            ICache<string, ProcessedConfiguration> processedConfigurationCache,
            TelemetryClient telemetry,
            IHttpContextAccessor httpContextAccessor,
            ILogger<ProcessedObservationRepository> logger) : base(client, true, processedConfigurationCache, logger)
        {
            LiveMode = true;

            _elasticConfiguration = elasticConfiguration ?? throw new ArgumentNullException(nameof(elasticConfiguration));
            _elasticClientManager = elasticClientManager ?? throw new ArgumentNullException(nameof(elasticClientManager));            
            _telemetry = telemetry ?? throw new ArgumentNullException(nameof(telemetry));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            WriteBatchSize = elasticConfiguration.WriteBatchSize;
        }

        /// <summary>
        /// Constructor used in admin mode
        /// </summary>
        /// <param name="elasticClientManager"></param>
        /// <param name="client"></param>
        /// <param name="elasticConfiguration"></param>
        /// <param name="processedConfigurationCache"></param>
        /// <param name="logger"></param>
        public ProcessedObservationRepository(
            IElasticClientManager elasticClientManager,
            IProcessClient client,
            ElasticSearchConfiguration elasticConfiguration,
            ICache<string, ProcessedConfiguration> processedConfigurationCache,
            ILogger<ProcessedObservationRepository> logger) : base(client, true, processedConfigurationCache, logger)
        {
            LiveMode = false;

            _elasticConfiguration = elasticConfiguration ?? throw new ArgumentNullException(nameof(elasticConfiguration));
            _elasticClientManager = elasticClientManager ?? throw new ArgumentNullException(nameof(elasticClientManager));
            WriteBatchSize = elasticConfiguration.WriteBatchSize;
        }

        /// <inheritdoc />
        public async Task<int> AddManyAsync(IEnumerable<Observation> items, bool protectedIndex)
        {
            // Save valid processed data
            Logger.LogDebug($"Start indexing batch for searching with {items.Count()} items");
            var indexResult = WriteToElastic(items, protectedIndex);
            Logger.LogDebug("Finished indexing batch for searching");
            if (indexResult == null || indexResult.TotalNumberOfFailedBuffers > 0) return 0;
            return items.Count();
        }

        /// <inheritdoc />
        public async Task<bool> ClearCollectionAsync(bool protectedIndex)
        {
            await DeleteCollectionAsync(protectedIndex);
            return await AddCollectionAsync(protectedIndex);
        }

        /// <inheritdoc />
        public async Task<bool> CopyProviderDataAsync(DataProvider dataProvider, bool protectedIndex)
        {
            var scrollResult = await ScrollObservationsWithCompleteObjectAsync(dataProvider.Id, protectedIndex, null);

            while (scrollResult?.Records?.Any() ?? false)
            {
                var processedObservations = scrollResult.Records;
                var indexResult = WriteToElastic(processedObservations, false);

                if (indexResult.TotalNumberOfFailedBuffers != 0)
                {
                    return false;
                }

                scrollResult = await ScrollObservationsWithCompleteObjectAsync(dataProvider.Id, protectedIndex, scrollResult.ScrollId);
            }

            return true;
        }

        /// <inheritdoc />
        public async Task<bool> DeleteByOccurrenceIdAsync(IEnumerable<string> occurenceIds, bool protectedIndex)
        {
            try
            {
                // Create the collection
                var res = await Client.DeleteByQueryAsync<Observation>(q => q
                    .Index(protectedIndex ? ProtectedIndexName : PublicIndexName)
                    .Query(q => q
                        .Terms(t => t
                            .Field(f => f.Occurrence.OccurrenceId)
                            .Terms(occurenceIds)
                        )
                    )
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
        public async Task<bool> DeleteProviderDataAsync(DataProvider dataProvider, bool protectedIndex)
        {
            try
            {
                // Create the collection
                var res = await Client.DeleteByQueryAsync<Observation>(q => q
                    .Index(protectedIndex ? ProtectedIndexName : PublicIndexName)
                    .Query(q => q
                        .Term(t => t
                            .Field(f => f.DataProviderId)
                            .Value(dataProvider.Id))));

                return res.IsValid;
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<bool> DisableIndexingAsync(bool protectedIndex)
        {
            var updateSettingsResponse =
                await Client.Indices.UpdateSettingsAsync(protectedIndex ? ProtectedIndexName : PublicIndexName,
                    p => p.IndexSettings(g => g.RefreshInterval(-1)));

            return updateSettingsResponse.Acknowledged && updateSettingsResponse.IsValid;
        }

        /// <inheritdoc />
        public async Task EnableIndexingAsync(bool protectedIndex)
        {
            await Client.Indices.UpdateSettingsAsync(protectedIndex ? ProtectedIndexName : PublicIndexName,
                p => p.IndexSettings(g => g.RefreshInterval(1)));
        }

        /// <inheritdoc />
        public async Task<PagedResult<dynamic>> GetAggregatedChunkAsync(SearchFilter filter, AggregationType aggregationType, int skip, int take)
        {
            var indexNames = GetCurrentIndex(filter);
            var (query, excludeQuery) = GetCoreQueries(filter);
            query.AddAggregationFilter(aggregationType);

            // Aggregation for distinct count
            static IAggregationContainer AggregationCardinality(AggregationContainerDescriptor<dynamic> agg) => agg
                .Cardinality("species_count", c => c
                    .Field("taxon.scientificName")
                );

            // Result-aggregation on taxon.id
            static IAggregationContainer Aggregation(AggregationContainerDescriptor<dynamic> agg, int size) => agg
                .Terms("species", t => t
                    .Script(s => s
                        // Build a sortable key
                        .Source("doc['taxon.attributes.sortOrder'].value + '-' + doc['taxon.scientificName'].value")
                    )
                    .Order(o => o.KeyAscending())
                    .Aggregations(thAgg => thAgg
                        .TopHits("info", info => info
                            .Size(1)
                            .Source(src => src
                                .Includes(inc => inc
                                    .Fields("taxon.id", "taxon.scientificName", "taxon.vernacularName", "taxon.scientificNameAuthorship", "taxon.attributes.redlistCategory")
                                )
                            )
                        )
                    )
                    .Order(o => o.KeyAscending())
                    .Size(size)
                );

            using var operation = _telemetry.StartOperation<DependencyTelemetry>("Observation_Search_Aggregated");
            operation.Telemetry.Properties["Filter"] = filter.ToString();

            // Get number of distinct values
            var searchResponseCount = await Client.SearchAsync<dynamic>(s => s
                .Size(0)
                .Index(indexNames)
                .Source(filter.OutputFields.ToProjection(filter is SearchFilterInternal))
                .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQuery)
                        .Filter(query)
                    )
                )
                .Aggregations(AggregationCardinality)
            );

            // Calculate size to fetch. If zero, get all
            var maxResult = (int?)searchResponseCount.Aggregations.Cardinality("species_count").Value ?? 0;
            var size = skip + take < maxResult ? skip + take : maxResult == 0 ? 1 : maxResult;
            if (skip == 0 && take == -1)
            {
                size = maxResult == 0 ? 1 : maxResult;
                take = maxResult;
            }

            if (aggregationType == AggregationType.SpeciesSightingsListTaxonCount)
            {
                return new PagedResult<dynamic>
                {
                    Records = new List<string>(),
                    Skip = 0,
                    Take = 0,
                    TotalCount = maxResult
                };
            }

            // Get the real result
            var searchResponse = await Client.SearchAsync<dynamic>(s => s
                .Size(0)
                .Index(indexNames)
                .Source(filter.OutputFields.ToProjection(filter is SearchFilterInternal))
                .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQuery)
                        .Filter(query)
                    )
                )
                .Aggregations(a => Aggregation(a, size))
            );

            if (!searchResponse.IsValid) throw new InvalidOperationException(searchResponse.DebugInformation);

            _telemetry.StopOperation(operation);

            var result = searchResponse
                .Aggregations
                .Terms("species")
                .Buckets?
                .Select(b =>
                    new AggregatedSpecies
                    {
                        TaxonId = b.TopHits("info").Documents<AggregatedSpeciesInfo>().FirstOrDefault()?.Taxon.Id ?? 0,
                        DocCount = b.DocCount,
                        VernacularName = b.TopHits("info").Documents<AggregatedSpeciesInfo>().FirstOrDefault()?.Taxon.VernacularName ?? "",
                        ScientificNameAuthorship = b.TopHits("info").Documents<AggregatedSpeciesInfo>().FirstOrDefault()?.Taxon.ScientificNameAuthorship ?? "",
                        ScientificName = b.TopHits("info").Documents<AggregatedSpeciesInfo>().FirstOrDefault()?.Taxon.ScientificName ?? "",
                        RedlistCategory = b.TopHits("info").Documents<AggregatedSpeciesInfo>().FirstOrDefault()?.Taxon.RedlistCategory ?? ""
                    })?
                .Skip(skip)
                .Take(take);

            return new PagedResult<dynamic>
            {
                Records = result,
                Skip = skip,
                Take = take,
                TotalCount = maxResult
            };

            // When operation is disposed, telemetry item is sent.
        }

        /// <inheritdoc />
        public async Task<PagedResult<dynamic>> GetAggregatedHistogramChunkAsync(SearchFilter filter, AggregationType aggregationType)
        {
            var indexNames = GetCurrentIndex(filter);
            var (query, excludeQuery) = GetCoreQueries(filter);
            query.AddAggregationFilter(aggregationType);

            var tz = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);
            IAggregationContainer Aggregation(AggregationContainerDescriptor<dynamic> agg) => agg
                .DateHistogram("aggregation", dh => dh
                    .Field("event.startDate")
                    .CalendarInterval(DateInterval.Day)
                    .TimeZone($"{(tz.TotalMinutes > 0 ? "+" : "")}{tz.Hours:00}:{tz.Minutes:00}")
                    .Format("yyyy-MM-dd")
                    .Aggregations(a => a
                        .Sum("quantity", sum => sum
                            .Field("occurrence.organismQuantityInt")
                        )
                    )
                );

            using var operation = _telemetry.StartOperation<DependencyTelemetry>("Observation_Search_Aggregated_Histogram");

            operation.Telemetry.Properties["Filter"] = filter.ToString();

            var searchResponse = await Client.SearchAsync<dynamic>(s => s
                .Size(0)
                .Index(indexNames)
                .Source(s => s.ExcludeAll())
                .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQuery)
                        .Filter(query)
                    )
                )
                .Aggregations(Aggregation)
            );

            if (!searchResponse.IsValid) throw new InvalidOperationException(searchResponse.DebugInformation);

            var totalCount = searchResponse.HitsMetadata.Total.Value;

            _telemetry.StopOperation(operation);

            var result = searchResponse
                .Aggregations
                .DateHistogram("aggregation")
                .Buckets?
                .Select(b =>
                    new
                    {
                        Date = DateTime.Parse(b.KeyAsString),
                        b.DocCount,
                        Quantity = b.Sum("quantity").Value
                    }).ToList();

            return new PagedResult<dynamic>
            {
                Records = result,
                Skip = 0,
                Take = result?.Count ?? 0,
                TotalCount = totalCount
            };

            // When operation is disposed, telemetry item is sent.
        }

        /// <inheritdoc />
        public async Task<PagedResult<dynamic>> GetChunkAsync(SearchFilter filter, int skip, int take, string sortBy,
            SearchSortOrder sortOrder)
        {
            var indexNames = GetCurrentIndex(filter);
            var (query, excludeQuery) = GetCoreQueries(filter);

            var sortDescriptor = await Client.GetSortDescriptorAsync<Observation>(indexNames, sortBy, sortOrder);
            using var operation = _telemetry.StartOperation<DependencyTelemetry>("Observation_Search");

            operation.Telemetry.Properties["Filter"] = filter.ToString();

            var searchResponse = await Client.SearchAsync<dynamic>(s => s
                .Index(indexNames)
                .Source(filter.OutputFields.ToProjection(filter is SearchFilterInternal))
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
            
            if (!searchResponse.IsValid) throw new InvalidOperationException(searchResponse.DebugInformation);

            var totalCount = searchResponse.HitsMetadata.Total.Value;

            var includeRealCount = totalCount >= ElasticSearchMaxRecords;

            if (filter is SearchFilterInternal internalFilter)
            {
                includeRealCount = internalFilter.IncludeRealCount;
            }

            if (includeRealCount)
            {
                var countResponse = await Client.CountAsync<dynamic>(s => s
                    .Index(indexNames)
                    .Query(q => q
                        .Bool(b => b
                            .MustNot(excludeQuery)
                            .Filter(query)
                        )
                    )
                );
                if (!countResponse.IsValid) throw new InvalidOperationException(countResponse.DebugInformation);
                totalCount = countResponse.Count;
            }

            operation.Telemetry.Metrics["SpeciesObservationCount"] = searchResponse.Documents.Count;

            // Optional: explicitly send telemetry item:
            _telemetry.StopOperation(operation);

            return new PagedResult<dynamic>
            {
                Records = searchResponse.Documents,
                Skip = skip,
                Take = take,
                TotalCount = totalCount
            };

            // When operation is disposed, telemetry item is sent.
        }

        /// <summary>
        /// Aggregate observations by GeoTile and Taxa. This method handles all paging and returns the complete result.
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="zoom">The precision to use in the GeoTileGrid aggregation.</param>
        /// <returns></returns>
        public async Task<Result<IEnumerable<GeoGridTileTaxaCell>>> GetCompleteGeoTileTaxaAggregationAsync(
            SearchFilter filter,
            int zoom)
        {
            var (query, excludeQuery) = GetCoreQueries(filter);

            var taxaByGeoTile = new Dictionary<string, Dictionary<int, long?>>();
            CompositeKey nextPageKey = null;

            do
            {
                var searchResponse = await PageGeoTileAndTaxaAsync(query, excludeQuery, zoom, nextPageKey);
                var compositeAgg = searchResponse.Aggregations.Composite("geoTileTaxonComposite");
                nextPageKey = compositeAgg.AfterKey;
                AddGeoTileTaxonResultToDictionary(compositeAgg, taxaByGeoTile);
            } while (nextPageKey != null);

            var georesult = taxaByGeoTile
                .Select(b => GeoGridTileTaxaCell.Create(
                    b.Key,
                    b.Value.Select(m => new GeoGridTileTaxonObservationCount()
                    {
                        ObservationCount = (int)m.Value.GetValueOrDefault(0),
                        TaxonId = m.Key
                    }).ToList()));

            return Result.Success(georesult);
        }

        /// <inheritdoc />
        public async Task<DataQualityReport> GetDataQualityReportAsync(string organismGroup)
        {
            var index = PublicIndexName;

            var searchResponse = await Client.SearchAsync<dynamic>(s => s
                .Size(0)
                .Index(index)
                .Query(q => q
                    .Bool(b => b
                        .Filter(f => f.Term(t => t
                            .Field("taxon.attributes.organismGroup")
                            .Value(organismGroup?.ToLower())))
                    )
                )
                .Aggregations(a => a
                    .Terms("uniqueKeyCount", f => f
                        .Field("dataQuality.uniqueKey")
                        .MinimumDocumentCount(2)
                        .Size(65536)
                    )
                )
            );

            if (!searchResponse.IsValid) throw new InvalidOperationException(searchResponse.DebugInformation);

            var duplicates = searchResponse
                .Aggregations
                .Terms("uniqueKeyCount")
                .Buckets?
                .Select(b =>
                    new
                    {
                        UniqueKey = b.Key,
                        b.DocCount
                    }).ToArray();

            var report = new DataQualityReport();

            if (duplicates?.Any() ?? false)
            {
                var rowCount = 0;

                foreach (var duplicate in duplicates)
                {
                    searchResponse = await Client.SearchAsync<dynamic>(s => s
                        .Index(index)
                        .Size(10000)
                        .Source(s => s.Includes(i => i
                            .Field("dataProviderId")
                            .Field("occurrence.occurrenceId")
                            .Field("location.locality")
                            .Field("event.startDate")
                            .Field("event.endDate")
                            .Field("taxon.id")
                            .Field("taxon.scientificName")
                        ))
                        .Query(q => q
                            .Bool(b => b
                                .Filter(f => f.Term(t => t
                                    .Field("dataQuality.uniqueKey")
                                    .Value(duplicate.UniqueKey)))
                            )
                        )
                        .Sort(sort => sort.Field(f => f.Field("dataProviderId")))
                    );

                    if (!searchResponse.IsValid) throw new InvalidOperationException(searchResponse.DebugInformation);
                    var docCount = searchResponse.Documents.Count;
                    if (docCount == 0)
                    {
                        continue;
                    }

                    if (rowCount + docCount > 2000)
                    {
                        break;
                    }

                    var firstDocument = searchResponse.Documents.Cast<IDictionary<string, dynamic>>().First();
                    var locality = string.Empty;
                    if (firstDocument.TryGetValue(nameof(Observation.Location).ToLower(), out var locationDictionary))
                    {
                        locality = (string)locationDictionary["locality"];
                    }

                    var record = new DataQualityReportRecord
                    {
                        EndDate = firstDocument["event"]["endDate"],
                        Locality = locality,
                        Observations = searchResponse.Documents.Select(d => new DataQualityReportObservation
                        {
                            DataProviderId = d["dataProviderId"].ToString(),
                            OccurrenceId = d["occurrence"]["occurrenceId"],
                        }),
                        StartDate = firstDocument["event"]["startDate"],
                        TaxonId = firstDocument["taxon"]["id"].ToString(),
                        TaxonScientificName = firstDocument["taxon"]["scientificName"],
                        UniqueKey = duplicate.UniqueKey
                    };

                    report.Records.Add(record);
                    rowCount += docCount;
                }
            }

            return report;
        }

        /// <inheritdoc />
        public async Task<Result<GeoGridResult>> GetGeogridAggregationAsync(
                SearchFilter filter,
                int precision)
        {
           
            var indexNames = GetCurrentIndex(filter);
            var (query, excludeQuery) = GetCoreQueries(filter);

            using var operation = _telemetry.StartOperation<DependencyTelemetry>("Observation_Search_GeoAggregated");
            operation.Telemetry.Properties["Filter"] = filter.ToString();
           
            var searchResponse = await Client.SearchAsync<dynamic>(s => s
                .Index(indexNames)
                .Size(0)
                .Aggregations(a => a.GeoHash("geohash_grid", g => g
                    .Field("location.pointLocation")
                    .Size(MaxNrElasticSearchAggregationBuckets + 1)
                    .GeoHashPrecision((GeoHashPrecision)precision)
                    .Bounds(b => b.TopLeft(filter.Location.Geometries.BoundingBox.TopLeft.ToGeoLocation()).BottomRight(filter.Location.Geometries.BoundingBox.BottomRight.ToGeoLocation()))
                    .Aggregations(b => b
                        .Cardinality("taxa_count", t => t
                            .Field("taxon.id")))
                    //.Terms("taxa_unique", t => t
                    //    .Field("taxon.id")))
                    )
                )
                .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQuery)
                        .Filter(query)
                    )
                )
            );
            if (!searchResponse.IsValid)
            {
                if (searchResponse.ServerError?.Error?.CausedBy?.Type == "too_many_buckets_exception")
                {
                    return Result.Failure<GeoGridResult>($"The number of cells that will be returned is too large. The limit is {MaxNrElasticSearchAggregationBuckets} cells. Try using lower precision or a smaller bounding box.");
                }

                throw new InvalidOperationException(searchResponse.DebugInformation);
            }

            var nrOfGridCells = (int?)searchResponse.Aggregations.GeoHash("geohash_grid").Buckets?.Count ?? 0;
            if (nrOfGridCells > MaxNrElasticSearchAggregationBuckets)
            {
                return Result.Failure<GeoGridResult>($"The number of cells that will be returned is too large. The limit is {MaxNrElasticSearchAggregationBuckets} cells. Try using lower precision or a smaller bounding box.");
            }

            _telemetry.StopOperation(operation);

            var georesult = searchResponse
                .Aggregations
                .Terms("geohash_grid")
                .Buckets?
                .Select(b =>
                    new GridCellGeohash()
                    {
                        ObservationsCount = b.DocCount,
                        TaxaCount = (long?)b.Cardinality("taxa_count")?.Value,
                        BoundingBox = LatLonGeohashBoundingBox.CreateFromGeohash(b.Key).Value
                    });

            var gridResult = new GeoGridResult()
            {
                BoundingBox = filter.Location.Geometries.BoundingBox,
                Precision = precision,
                GridCellCount = nrOfGridCells,
                GridCells = georesult
            };

            // When operation is disposed, telemetry item is sent.
            return Result.Success(gridResult);
        }

        /// <inheritdoc />
        public async Task<Result<GeoGridTileResult>> GetGeogridTileAggregationAsync(
                SearchFilter filter,
                int zoom)
        {
            var indexNames = GetCurrentIndex(filter);
            var (query, excludeQuery) = GetCoreQueries(filter);

            using var operation = _telemetry.StartOperation<DependencyTelemetry>("Observation_Search_GeoAggregated");
            operation.Telemetry.Properties["Filter"] = filter.ToString();
           
            var searchResponse = await Client.SearchAsync<dynamic>(s => s
                .Index(indexNames)
                .Size(0)
                .Aggregations(a => a.Filter("geotile_filter", g => g
                        .Filter(f => f.GeoBoundingBox(bb => bb
                        .Field("location.pointLocation")
                        .BoundingBox(b => b.TopLeft(filter.Location.Geometries.BoundingBox.TopLeft.ToGeoLocation()).BottomRight(filter.Location.Geometries.BoundingBox.BottomRight.ToGeoLocation()))
                   ))
                    .Aggregations(ab => ab.GeoTile("geotile_grid", gg => gg
                        .Field("location.pointLocation")
                        .Size(MaxNrElasticSearchAggregationBuckets + 1)
                        .Precision((GeoTilePrecision)zoom)
                        .Aggregations(b => b
                            .Cardinality("taxa_count", t => t
                                .Field("taxon.id"))
                        )))
                    )
                )
                .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQuery)
                        .Filter(query)
                    )
                )
            );


            if (!searchResponse.IsValid)
            {
                if (searchResponse.ServerError?.Error?.CausedBy?.Type == "too_many_buckets_exception")
                {
                    return Result.Failure<GeoGridTileResult>($"The number of cells that will be returned is too large. The limit is {MaxNrElasticSearchAggregationBuckets} cells. Try using lower zoom or a smaller bounding box.");
                }

                throw new InvalidOperationException(searchResponse.DebugInformation);
            }

            var nrOfGridCells = (int?)searchResponse.Aggregations?.Filter("geotile_filter")?.GeoTile("geotile_grid")?.Buckets?.Count ?? 0;
            if (nrOfGridCells > MaxNrElasticSearchAggregationBuckets)
            {
                return Result.Failure<GeoGridTileResult>($"The number of cells that will be returned is too large. The limit is {MaxNrElasticSearchAggregationBuckets} cells. Try using lower zoom or a smaller bounding box.");
            }

            _telemetry.StopOperation(operation);

            var georesult = searchResponse
                .Aggregations
                .Filter("geotile_filter")
                .GeoTile("geotile_grid")
                .Buckets?
                .Select(b => GridCellTile.Create(b.Key, b.DocCount, (long?)b.Cardinality("taxa_count").Value));

            var gridResult = new GeoGridTileResult()
            {
                BoundingBox = filter.Location.Geometries.BoundingBox,
                Zoom = zoom,
                GridCellTileCount = nrOfGridCells,
                GridCellTiles = georesult
            };

            // When operation is disposed, telemetry item is sent.
            return Result.Success(gridResult);
        }

        /// <inheritdoc />
        public async Task<Result<GeoGridMetricResult>> GetMetricGridAggregationAsync(
            SearchFilter filter,
            int gridCellSizeInMeters)
        {
            var indexNames = GetCurrentIndex(filter);
            var (query, excludeQuery) = GetCoreQueries(filter);

            using var operation =
                _telemetry.StartOperation<DependencyTelemetry>("Observation_Search_MetricGridAggregation");
            operation.Telemetry.Properties["Filter"] = filter.ToString();
           
            var searchResponse = await Client.SearchAsync<dynamic>(s => s
                .Index(indexNames)
                .Size(0)
                .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQuery)
                        .Filter(query)
                    )
                )
                .Aggregations(a => a
                    .Composite("gridCells", c => c
                        .Size(MaxNrElasticSearchAggregationBuckets + 1)
                        .Sources(s => s
                            .Terms("sweref99tm_x", t => t
                                .Script(sct => sct
                                    .Source(
                                        $"(Math.floor(doc['location.sweref99TmX'].value / {gridCellSizeInMeters}) * {gridCellSizeInMeters}).intValue()")
                                )
                            )
                            .Terms("sweref99tm_y", t => t
                                .Script(sct => sct
                                    .Source(
                                        $"(Math.floor(doc['location.sweref99TmY'].value / {gridCellSizeInMeters}) * {gridCellSizeInMeters}).intValue()")
                                )
                            )
                        )
                        .Aggregations(a => a
                            .Cardinality("taxa_count", c => c
                                .Field("taxon.id")
                            ) 
                        )
                    )
                )
            );

            if (!searchResponse.IsValid)
            {
                if (searchResponse.ServerError?.Error?.CausedBy?.Type == "too_many_buckets_exception")
                {
                    return Result.Failure<GeoGridMetricResult>($"The number of cells that will be returned is too large. The limit is {MaxNrElasticSearchAggregationBuckets} cells. Try using lower zoom or a smaller bounding box.");
                }

                throw new InvalidOperationException(searchResponse.DebugInformation);
            }

            var nrOfGridCells = (int?)searchResponse.Aggregations?.Composite("gridCells")?.Buckets?.Count ?? 0;
            if (nrOfGridCells > MaxNrElasticSearchAggregationBuckets)
            {
                return Result.Failure<GeoGridMetricResult>($"The number of cells that will be returned is too large. The limit is {MaxNrElasticSearchAggregationBuckets} cells. Try using lower zoom or a smaller bounding box.");
            }

            _telemetry.StopOperation(operation);
            
            var gridResult = new GeoGridMetricResult()
            {
                BoundingBox = filter.Location.Geometries.BoundingBox,
                GridCellSizeInMeters = gridCellSizeInMeters,
                GridCellCount = nrOfGridCells,
                GridCells = searchResponse.Aggregations.Composite("gridCells").Buckets.Select(b => 
                    new GridCell
                    {
                        Sweref99TmBoundingBox = new XYBoundingBox
                        {
                            BottomRight = new XYCoordinate(double.Parse(b.Key["sweref99tm_x"].ToString()) + gridCellSizeInMeters, double.Parse(b.Key["sweref99tm_y"].ToString())),
                            TopLeft = new XYCoordinate(double.Parse(b.Key["sweref99tm_x"].ToString()), double.Parse(b.Key["sweref99tm_y"].ToString()) + gridCellSizeInMeters)
                        },
                        ObservationsCount = b.DocCount,
                        TaxaCount = (long?)b.Cardinality("taxa_count").Value
                    }
                )
            };

            // When operation is disposed, telemetry item is sent.
            return Result.Success(gridResult);
        }

        /// <inheritdoc />
        public async Task<WaitForStatus> GetHealthStatusAsync(WaitForStatus waitForStatus)
        {
            var response = await Client.Cluster.HealthAsync(new ClusterHealthRequest() { WaitForStatus = waitForStatus });

            var healthColor = response.Status.ToString().ToLower();

            return healthColor switch
            {
                "green" => WaitForStatus.Green,
                "yellow" => WaitForStatus.Yellow,
                "red" => WaitForStatus.Red,
                _ => WaitForStatus.Red
            };
        }

        /// <inheritdoc />
        public async Task<DateTime> GetLatestModifiedDateForProviderAsync(int providerId)
        {
            var publicLatestModifiedDate = await GetLatestModifiedDateForProviderAsync(providerId, false);
            var protectedLatestModifiedDate = await GetLatestModifiedDateForProviderAsync(providerId, true);

            return protectedLatestModifiedDate > publicLatestModifiedDate
                ? protectedLatestModifiedDate
                : publicLatestModifiedDate;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Location>> GetLocationsAsync(IEnumerable<string> locationIds)
        {
            if (!locationIds?.Any() ?? true)
            {
                return null;
            }

            var searchResponse = await Client.SearchAsync<Observation>(s => s
                .Index($"{PublicIndexName}, {ProtectedIndexName}")
                .Query(q => q
                    .Bool(b => b
                        .Filter(f => f
                            .Terms(t => t
                                .Field("location.locationId.keyword")
                                .Terms(locationIds)
                            )
                        )
                    )
                )
                .Collapse(c => c.Field("location.locationId.keyword"))
               .Source(s => s
                    .Includes(i => i
                        .Field("location")
                    )
                )
            );

            if (!searchResponse.IsValid)
            {
                throw new InvalidOperationException(searchResponse.DebugInformation);
            }

            return searchResponse.Documents?.Select(d => d.Location);
        }

        /// <inheritdoc />
        public async Task<long> GetMatchCountAsync(SearchFilterBase filter)
        {
            var indexNames = GetCurrentIndex(filter);
            var (query, excludeQuery) = GetCoreQueries(filter);

            var countResponse = await Client.CountAsync<dynamic>(s => s
                .Index(indexNames)
                .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQuery)
                        .Filter(query)
                    )
                )
            );
            if (!countResponse.IsValid) throw new InvalidOperationException(countResponse.DebugInformation);

            return countResponse.Count;
        }

        /// <inheritdoc />
        public async Task<int> GetProvinceCountAsync(SearchFilterBase filter)
        {
            var indexNames = GetCurrentIndex(filter);
            var (query, excludeQuery) = GetCoreQueries(filter);

            var searchResponse = await Client.SearchAsync<dynamic>(s => s
                .Index(indexNames)
                .Size(0)
                .Aggregations(a => a.Cardinality("provinceCount", c => c
                    .Field("location.province.featureId.keyword")))
                .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQuery)
                        .Filter(query)
                    )
                ));

            if (!searchResponse.IsValid) throw new InvalidOperationException(searchResponse.DebugInformation);
            int provinceCount = Convert.ToInt32(searchResponse.Aggregations.Cardinality("provinceCount").Value);
            return provinceCount;
        }

        public async Task<dynamic> GetObservationAsync(string occurrenceId, SearchFilter filter)
        {
            var indexNames = GetCurrentIndex(filter);
            var (query, excludeQuery) = GetCoreQueries(filter);
            query.TryAddTermCriteria("occurrence.occurrenceId", occurrenceId);
            using var operation = _telemetry.StartOperation<DependencyTelemetry>("Observation_Get");            

            operation.Telemetry.Properties["OccurrenceId"] = occurrenceId;
            operation.Telemetry.Properties["Filter"] = filter.ToString();

            var searchResponse = await Client.SearchAsync<dynamic>(s => s
                .Index(indexNames)
                .Source(filter.OutputFields.ToProjection(filter is SearchFilterInternal))
                .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQuery)
                        .Filter(query)
                    )
                )
            );

            if (!searchResponse.IsValid) throw new InvalidOperationException(searchResponse.DebugInformation);

            // Optional: explicitly send telemetry item:
            _telemetry.StopOperation(operation);

            return searchResponse.Documents;

        }

        /// <inheritdoc />
        public async Task<IEnumerable<Observation>> GetObservationsAsync(IEnumerable<string> occurrenceIds, bool protectedIndex)
        {
            try
            {
                var searchResponse = await Client.SearchAsync<Observation>(s => s
                    .Index(protectedIndex ? ProtectedIndexName : PublicIndexName)
                    .Source(s => s
                        .Includes(i => i.Fields(f => f.Occurrence, f => f.Location))
                    )
                    .Query(q => q
                        .Terms(t => t
                            .Field(f => f.Occurrence.OccurrenceId)
                            .Terms(occurrenceIds)
                        )
                    )
                );

                if (!searchResponse.IsValid)
                {
                    throw new InvalidOperationException(searchResponse.DebugInformation);
                }

                return searchResponse.Documents;
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
                return null;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Observation>> GetObservationsAsync(
            IEnumerable<string> occurrenceIds,
            IEnumerable<string> outputFields,
            bool protectedIndex)
        {
            var searchResponse = await Client.SearchAsync<Observation>(s => s
                .Index(protectedIndex ? ProtectedIndexName : PublicIndexName)
                .Query(q => q
                    .Terms(t => t
                        .Field(f => f.Occurrence.OccurrenceId)
                        .Terms(occurrenceIds)
                    )
                )
                .Source(p => p
                    .Includes(i => i
                        .Fields(outputFields
                            .Select(f => new Field(f)))))
            );

            if (!searchResponse.IsValid) throw new InvalidOperationException(searchResponse.DebugInformation);

            return searchResponse.Documents;
        }

        public async Task<ScrollResult<dynamic>> GetObservationsByScrollAsync(
            SearchFilter filter,
            int take, 
            string sortBy,
            SearchSortOrder sortOrder,
            string scrollId)
        {
            var indexNames = GetCurrentIndex(filter);
            var (query, excludeQuery) = GetCoreQueries(filter);

            var sortDescriptor = await Client.GetSortDescriptorAsync<Observation>(indexNames, sortBy, sortOrder);
            using var operation = _telemetry.StartOperation<DependencyTelemetry>("Observation_Search");

            operation.Telemetry.Properties["Filter"] = filter.ToString();
            ISearchResponse<dynamic> searchResponse;

            if (string.IsNullOrEmpty(scrollId))
            {
                searchResponse = await Client.SearchAsync<dynamic>(s => s
                    .Index(indexNames)
                    .Source(filter.OutputFields.ToProjection(filter is SearchFilterInternal))
                    .Size(take)
                    .Scroll(_elasticConfiguration.ScrollTimeout)
                    .Query(q => q
                        .Bool(b => b
                            .MustNot(excludeQuery)
                            .Filter(query)
                        )
                    )
                    .Sort(sort => sortDescriptor)
                );
            }
            else
            {
                searchResponse = await Client
                    .ScrollAsync<dynamic>(_elasticConfiguration.ScrollTimeout, scrollId);
            }

            if (!searchResponse.IsValid) throw new InvalidOperationException(searchResponse.DebugInformation);
            operation.Telemetry.Metrics["SpeciesObservationCount"] = searchResponse.Documents.Count;

            // Optional: explicitly send telemetry item:
            _telemetry.StopOperation(operation);

            return new ScrollResult<dynamic>
            {
                Records = searchResponse.Documents,
                ScrollId = searchResponse.Documents.Count < take ? null : searchResponse.ScrollId,
                Take = take,
                TotalCount = searchResponse.HitsMetadata.Total.Value
            };

            // When operation is disposed, telemetry item is sent.
        }

        /// <summary>
        /// Aggregate observations by GeoTile and Taxa. This method uses paging.
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="zoom">The precision to use in the GeoTileGrid aggregation.</param>
        /// <param name="geoTilePage">The GeoTile key. Should be null in the first request.</param>
        /// <param name="taxonIdPage">The TaxonId key. Should be null in the first request.</param>
        /// <returns></returns>
        public async Task<Result<GeoGridTileTaxonPageResult>> GetPageGeoTileTaxaAggregationAsync(
                SearchFilter filter,
                int zoom,
                string geoTilePage,
                int? taxonIdPage)
        {
            int maxNrBucketsInPageResult = MaxNrElasticSearchAggregationBuckets * 3;
            var (query, excludeQuery) = GetCoreQueries(filter);

            int nrAdded = 0;
            var taxaByGeoTile = new Dictionary<string, Dictionary<int, long?>>();
            CompositeKey nextPageKey = null;
            if (!string.IsNullOrEmpty(geoTilePage) && taxonIdPage.HasValue)
            {
                nextPageKey = new CompositeKey(new Dictionary<string, object> { { "geoTile", geoTilePage }, { "taxon", taxonIdPage } });
            }

            do
            {
                var searchResponse = await PageGeoTileAndTaxaAsync(query, excludeQuery, zoom, nextPageKey);
                var compositeAgg = searchResponse.Aggregations.Composite("geoTileTaxonComposite");
                nextPageKey = compositeAgg.AfterKey;
                nrAdded += AddGeoTileTaxonResultToDictionary(compositeAgg, taxaByGeoTile);
            } while (nrAdded < maxNrBucketsInPageResult && nextPageKey != null);

            var georesult = taxaByGeoTile
                .Select(b => GeoGridTileTaxaCell.Create(
                    b.Key,
                    b.Value.Select(m => new GeoGridTileTaxonObservationCount()
                    {
                        ObservationCount = (int)m.Value.GetValueOrDefault(0),
                        TaxonId = m.Key
                    }).ToList())).ToList();

            var result = new GeoGridTileTaxonPageResult
            {
                NextGeoTilePage = nextPageKey?["geoTile"].ToString(),
                NextTaxonIdPage = nextPageKey == null ? null : (int?)Convert.ToInt32((long)nextPageKey["taxon"]),
                HasMorePages = nextPageKey != null,
                GridCells = georesult
            };

            return Result.Success(result);
        }

        /// <inheritdoc /> 
        public async Task<(DateTime? firstSpotted, DateTime? lastSpotted, GeoBounds geographicCoverage)> GetProviderMetaDataAsync(int providerId, bool protectedIndex)
        {
            var res = await Client.SearchAsync<Observation>(s => s
                .Index(protectedIndex ? ProtectedIndexName : PublicIndexName)
                .Query(q => q
                    .Term(t => t
                        .Field(f => f.DataProviderId)
                        .Value(providerId)))
                .Aggregations(a => a
                    .Min("firstSpotted", m => m
                        .Field(f => f.Event.StartDate)
                    )
                    .Max("lastSpotted", m => m
                        .Field(f => f.Event.EndDate)
                    )
                    .GeoBounds("geographicCoverage", g => g
                        .Field(f => f.Location.PointLocation)
                        .WrapLongitude()
                    )
                )
            );

            var defaultGeoBounds = new GeoBounds
                { BottomRight = new LatLon() { Lat = 0.0, Lon = 0.0 }, TopLeft = new LatLon() { Lat = 0.0, Lon = 0.0 } };
            if (!res.IsValid)
            {
                return (null, null, defaultGeoBounds);
            }

            var firstSpotted = res.Aggregations?.Min("firstSpotted")?.Value;
            var lastSpotted = res.Aggregations?.Max("lastSpotted")?.Value;
            var geographicCoverage = res.Aggregations?.GeoBounds("geographicCoverage")?.Bounds;

            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);

            return (epoch.AddMilliseconds(firstSpotted ?? 0).ToUniversalTime(), epoch.AddMilliseconds(lastSpotted ?? 0).ToUniversalTime(), geographicCoverage?.BottomRight != null ? geographicCoverage : defaultGeoBounds);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Observation>> GetRandomObservationsAsync(int take, bool protectedIndex)
        {
            try
            {
                var searchResponse = await Client.SearchAsync<Observation>(s => s
                    .Index(protectedIndex ? ProtectedIndexName : PublicIndexName)
                    .Query(q => q
                        .FunctionScore(fs => fs
                            .Functions(f => f
                                .RandomScore(rs => rs
                                    .Seed(DateTime.Now.ToBinary())
                                    .Field(p => p.Occurrence.OccurrenceId)))))
                    .Source(s => s
                        .Includes(i => i.Fields(f => f.Occurrence, f => f.Location))
                    )
                    .Size(take)
                );

                if (!searchResponse.IsValid)
                {
                    throw new InvalidOperationException(searchResponse.DebugInformation);
                }

                return searchResponse.Documents;
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
                return null;
            }
        }

        /// <inheritdoc />
        public async Task<Result<PagedResult<TaxonAggregationItem>>> GetTaxonAggregationAsync(
            SearchFilter filter,
            int? skip,
            int? take)
        {
            var indexName = GetCurrentIndex(filter);
            var (query, excludeQuery) = GetCoreQueries(filter);

            Dictionary<int,int> observationCountByTaxonId = await GetAllObservationCountByTaxonIdAsync(
                indexName,
                query, 
                excludeQuery);

            // Update skip and take
            if (skip == null)
            {
                skip = 0;
            }
            if (skip > observationCountByTaxonId.Count)
            {
                skip = observationCountByTaxonId.Count;
            }
            if (take == null)
            {
                take = observationCountByTaxonId.Count - skip;
            }
            else
            {
                take = Math.Min(observationCountByTaxonId.Count - skip.Value, take.Value);
            }

            var taxaResult = observationCountByTaxonId
                .Select(b => TaxonAggregationItem.Create(
                    b.Key,
                    b.Value))
                .OrderByDescending(m => m.ObservationCount)
                .Skip(skip.Value)
                .Take(take.Value)
                .ToList();

            var pagedResult = new PagedResult<TaxonAggregationItem>
            {
                Records = taxaResult,
                Skip = skip.Value,
                Take = take.Value,
                TotalCount = observationCountByTaxonId.Count
            };

            return Result.Success(pagedResult);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TaxonAggregationItem>> GetTaxonExistsIndicationAsync(
            SearchFilter filter)
        {
            var indexNames = GetCurrentIndex(filter);
            var (query, excludeQuery) = GetCoreQueries(filter);

            var searchResponse = await Client.SearchAsync<dynamic>(s => s
                .Size(0)
                .Index(indexNames)
                .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQuery)
                        .Filter(query)
                    )
                )
                .Aggregations(a => a
                    .Terms("taxon_group", t => t
                        .Size(filter.Taxa?.Ids?.Count()) // Size can never be grater than number of taxon id's
                        .Field("taxon.id")
                    )
                )
            );

            if (!searchResponse.IsValid) throw new InvalidOperationException(searchResponse.DebugInformation);

            return searchResponse.Aggregations
                .Terms("taxon_group")
                .Buckets
                .Select(b => new TaxonAggregationItem { TaxonId = int.Parse(b.Key), ObservationCount = (int)(b.DocCount ?? 0) });
        }

        /// <inheritdoc />
        public async Task<bool> HasIndexOccurrenceIdDuplicatesAsync(bool protectedIndex)
        {
            var searchResponse = await Client.SearchAsync<Observation>(s => s
                .Size(0)
                .Index(protectedIndex ? ProtectedIndexName : PublicIndexName)
                .Aggregations(a => a
                    .Terms("uniqueOccurrenceIdCount", t => t
                        .Field(f => f.Occurrence.OccurrenceId)
                        .MinimumDocumentCount(2)
                        .Size(1)
                    )
                )
            );

            if (!searchResponse.IsValid) throw new InvalidOperationException(searchResponse.DebugInformation);

            return searchResponse.Aggregations
                .Terms("uniqueOccurrenceIdCount")
                .Buckets.Count > 0;
        }

        /// <inheritdoc />
        public Uri HostUrl => Client.ConnectionSettings.ConnectionPool.Nodes.FirstOrDefault().Uri;

        /// <inheritdoc />
        public async Task<long> IndexCountAsync(bool protectedIndex)
        {
            try
            {
                var countResponse = await Client.CountAsync<dynamic>(s => s
                    .Index(protectedIndex ? ProtectedIndexName : PublicIndexName)
                );

                if (!countResponse.IsValid)
                {
                    throw new InvalidOperationException(countResponse.DebugInformation);
                }

                return countResponse.Count;
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
                return -1;
            }
        }

        public int MaxNrElasticSearchAggregationBuckets => _elasticConfiguration.MaxNrAggregationBuckets;

        /// <inheritdoc />
        public string PublicIndexName => IndexHelper.GetIndexName<Observation>(_elasticConfiguration.IndexPrefix, _elasticClientManager.Clients.Length == 1, LiveMode ? ActiveInstance : InActiveInstance, false);

        /// <inheritdoc />
        public string ProtectedIndexName => IndexHelper.GetIndexName<Observation>(_elasticConfiguration.IndexPrefix, _elasticClientManager.Clients.Length == 1, LiveMode ? ActiveInstance : InActiveInstance, true);

        /// <inheritdoc />
        public async Task<ScrollResult<ExtendedMeasurementOrFactRow>> ScrollMeasurementOrFactsAsync(
            SearchFilterBase filter,
            string scrollId)
        {
            ISearchResponse<dynamic> searchResponse;
            if (string.IsNullOrEmpty(scrollId))
            {
                var indexNames = GetCurrentIndex(filter);
                searchResponse = await Client.SearchAsync<dynamic>(s => s
                    .Index(indexNames)
                    .Source(source => source
                        .Includes(fieldsDescriptor => fieldsDescriptor
                            .Field("occurrence.occurrenceId")
                            .Field("measurementOrFacts")))
                    .Query(query => query
                        .Bool(boolQueryDescriptor => boolQueryDescriptor
                            .Filter(filter.ToMeasurementOrFactsQuery())
                        )
                    )
                    .Sort(s => s.Ascending(new Field("_doc")))
                    .Scroll(_elasticConfiguration.ScrollTimeout)
                    .Size(_elasticConfiguration.ScrollBatchSize)
                );
            }
            else
            {
                searchResponse = await Client
                    .ScrollAsync<Observation>(_elasticConfiguration.ScrollTimeout, scrollId);
            }

            if (!searchResponse.IsValid) throw new InvalidOperationException(searchResponse.DebugInformation);

            return new ScrollResult<ExtendedMeasurementOrFactRow>
            {
                Records = CastDynamicsToObservations(searchResponse.Documents)?.ToExtendedMeasurementOrFactRows(),
                ScrollId = searchResponse.ScrollId,
                TotalCount = searchResponse.HitsMetadata.Total.Value
            };
        }

        /// <inheritdoc />
        public async Task<ScrollResult<SimpleMultimediaRow>> ScrollMultimediaAsync(
            SearchFilterBase filter,
            string scrollId)
        {
            ISearchResponse<dynamic> searchResponse;
            if (string.IsNullOrEmpty(scrollId))
            {
                var indexNames = GetCurrentIndex(filter);
                searchResponse = await Client.SearchAsync<dynamic>(s => s
                    .Index(indexNames)
                    .Source(source => source
                        .Includes(fieldsDescriptor => fieldsDescriptor
                            .Field("occurrence.occurrenceId")
                            .Field("media")))
                    .Query(query => query
                        .Bool(boolQueryDescriptor => boolQueryDescriptor
                            .Filter(filter.ToMultimediaQuery())
                        )
                    )
                    .Sort(s => s.Ascending(new Field("_doc")))
                    .Scroll(_elasticConfiguration.ScrollTimeout)
                    .Size(_elasticConfiguration.ScrollBatchSize)
                );
            }
            else
            {
                searchResponse = await Client
                    .ScrollAsync<dynamic>(_elasticConfiguration.ScrollTimeout, scrollId);
            }

            if (!searchResponse.IsValid) throw new InvalidOperationException(searchResponse.DebugInformation);


            return new ScrollResult<SimpleMultimediaRow>
            {
                Records = CastDynamicsToObservations(searchResponse.Documents)?.ToSimpleMultimediaRows(),
                ScrollId = searchResponse.ScrollId,
                TotalCount = searchResponse.HitsMetadata.Total.Value
            };
        }

        /// <inheritdoc />
        public async Task<ScrollResult<Observation>> ScrollObservationsAsync(
            SearchFilterBase filter,
            string scrollId)
        {
            ISearchResponse<dynamic> searchResponse;

            if (string.IsNullOrEmpty(scrollId))
            {
                var query = filter.ToQuery();
                var projection = new SourceFilterDescriptor<dynamic>()
                    .Excludes(e => e
                        .Field("artportalenInternal")
                        .Field("location.point")
                        .Field("location.pointLocation")
                        .Field("location.pointWithBuffer")
                        .Field("location.pointWithDisturbanceBuffer")
                    );
                var indexNames = GetCurrentIndex(filter);
                searchResponse = await Client
                    .SearchAsync<dynamic>(s => s
                        .Index(indexNames)
                        .Source(p => projection)
                        .Query(q => q
                            .Bool(b => b
                                .Filter(query)
                            )
                        )
                        .Sort(s => s.Ascending(new Field("_doc")))
                        .Scroll(_elasticConfiguration.ScrollTimeout)
                        .Size(_elasticConfiguration.ScrollBatchSize)
                    );

            }
            else
            {
                searchResponse = await Client
                    .ScrollAsync<Observation>(_elasticConfiguration.ScrollTimeout, scrollId);
            }

            if (!searchResponse.IsValid) throw new InvalidOperationException(searchResponse.DebugInformation);

            return new ScrollResult<Observation>
            {
                Records = CastDynamicsToObservations(searchResponse.Documents),
                ScrollId = searchResponse.ScrollId,
                TotalCount = searchResponse.HitsMetadata?.Total?.Value ?? 0
            };
        }

        public async Task<ScrollResult<dynamic>> ScrollObservationsAsDynamicAsync(
            SearchFilter filter,
            string scrollId)
        {
            ISearchResponse<dynamic> searchResponse;

            if (string.IsNullOrEmpty(scrollId))
            {
                var query = filter.ToQuery();
                var projection = new SourceFilterDescriptor<dynamic>()
                    .Excludes(e => e
                        .Field("artportalenInternal")
                        .Field("location.point")
                        .Field("location.pointLocation")
                        .Field("location.pointWithBuffer")
                        .Field("location.pointWithDisturbanceBuffer")
                    );
                var indexNames = GetCurrentIndex(filter);
                searchResponse = await Client
                    .SearchAsync<dynamic>(s => s
                        .Index(indexNames)
                        .Source(filter.OutputFields.ToProjection(filter is SearchFilterInternal))                        
                        .Query(q => q
                            .Bool(b => b
                                .Filter(query)
                            )
                        )
                        .Sort(s => s.Ascending(new Field("_doc")))
                        .Scroll(_elasticConfiguration.ScrollTimeout)
                        .Size(_elasticConfiguration.ScrollBatchSize)
                    );

            }
            else
            {
                searchResponse = await Client
                    .ScrollAsync<dynamic>(_elasticConfiguration.ScrollTimeout, scrollId);
            }

            if (!searchResponse.IsValid) throw new InvalidOperationException(searchResponse.DebugInformation);

            return new ScrollResult<dynamic>
            {
                Records = searchResponse.Documents,
                ScrollId = searchResponse.ScrollId,
                TotalCount = searchResponse.HitsMetadata?.Total?.Value ?? 0
            };
        }

        /// <inheritdoc />
        public async Task<bool> SignalSearchInternalAsync(
            SearchFilter filter,
            bool onlyAboveMyClearance)
        {
            // Save user extended authorization to use later
            var extendedAuthorizations = filter.ExtendedAuthorization?.ExtendedAreas;
            // Authorization is handled different in signal search, reset some values before we get core queries
            filter.ExtendedAuthorization.ExtendedAreas = null;
            filter.ExtendedAuthorization.UserId = 0;
            filter.ExtendedAuthorization.ProtectedObservations = false;

            var (query, excludeQuery) = GetCoreQueries(filter);
            query.AddSignalSearchCriteria(extendedAuthorizations, onlyAboveMyClearance);
            
            var searchResponse = await Client.CountAsync<dynamic>(s => s
                .Index(ProtectedIndexName)
                .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQuery)
                        .Filter(query)
                    )
                )
            );

            if (!searchResponse.IsValid)
            {
                throw new InvalidOperationException(searchResponse.DebugInformation);
            }

            return searchResponse.Count > 0;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<string>> TryToGetOccurenceIdDuplicatesAsync(bool activeInstance, bool protectedIndex, int maxReturnedItems)
        {
            var searchResponse = await (activeInstance ? Client : InActiveClient).SearchAsync<dynamic>(s => s
                .Size(0)
                .Index(protectedIndex ? ProtectedIndexName : PublicIndexName)
                .Source(s => s.ExcludeAll())
                .Aggregations(a => a
                    .Terms("OccurrenceIdDuplicatesExists", f => f
                        .Field("occurrence.occurrenceId")
                        .MinimumDocumentCount(2)
                        .Size(maxReturnedItems)
                    )
                )
            );

            if (!searchResponse.IsValid)
            {
                throw new InvalidOperationException(searchResponse.DebugInformation);
            }

            return searchResponse.Aggregations.Terms("OccurrenceIdDuplicatesExists").Buckets?.Select(b => b.Key);
        }

        /// <inheritdoc />
        public string UniquePublicIndexName => IndexHelper.GetIndexName<Observation>(_elasticConfiguration.IndexPrefix, true, LiveMode ? ActiveInstance : InActiveInstance, false);

        /// <inheritdoc />
        public string UniqueProtectedIndexName => IndexHelper.GetIndexName<Observation>(_elasticConfiguration.IndexPrefix, true, LiveMode ? ActiveInstance : InActiveInstance, true);

        /// <inheritdoc />
        public async Task<bool> ValidateProtectionLevelAsync(bool protectedIndex)
        {
            try
            {
                var countResponse = protectedIndex ?
                    await Client.CountAsync<Observation>(s => s
                    .Index(ProtectedIndexName)
                    .Query(q => q
                        .Bool(b => b
                            .MustNot(mn => mn.Term(t => t
                                .Field(f => f.DiffusionStatus).Value(DiffusionStatus.NotDiffused))
                            )
                        )
                    ))
                    :
                    await Client.CountAsync<Observation>(s => s
                    .Index(PublicIndexName)
                    .Query(q => q
                        .Bool(b => b
                            .Filter(f => f
                                .Term(t => t
                                    .Field(f => f.AccessRights.Id).Value((int)AccessRightsId.NotForPublicUsage)
                                ), f => f
                                .Term(t => t
                                    .Field(f => f.DiffusionStatus).Value(DiffusionStatus.NotDiffused)
                                )
                            )
                        )
                    ));

                if (!countResponse.IsValid)
                {
                    throw new InvalidOperationException(countResponse.DebugInformation);
                }
                if (!countResponse.Count.Equals(0))
                {
                    Logger.LogError($"Failed to validate protection level for Index: {(protectedIndex ? ProtectedIndexName : PublicIndexName)}, count of observations with protection:{protectedIndex} = {countResponse.Count}, should be 0");
                }
                return countResponse.Count.Equals(0);
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<bool> VerifyCollectionAsync(bool protectedIndex)
        {
            var response = await Client.Indices.ExistsAsync(protectedIndex ? ProtectedIndexName : PublicIndexName);

            if (!response.Exists)
            {
                await AddCollectionAsync(protectedIndex);
            }

            return !response.Exists;
        }

        /// <inheritdoc />
        public int WriteBatchSize { get; set; }
    }
}
