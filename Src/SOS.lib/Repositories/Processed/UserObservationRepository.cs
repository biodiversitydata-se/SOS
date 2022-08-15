using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nest;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Processed.Configuration;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search.Result;
using SOS.Lib.Models.Statistics;
using SOS.Lib.Repositories.Processed.Interfaces;

namespace SOS.Lib.Repositories.Processed
{
    /// <summary>
    ///     User observation repository.
    /// </summary>
    public class UserObservationRepository : ProcessRepositoryBase<UserObservation, long>,
        IUserObservationRepository
    {
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
                .Map<UserObservation>(m => m
                    .AutoMap<UserObservation>()
                    .Properties(p => p
                        .Keyword(kw => kw
                            .Name(nm => nm.ProvinceFeatureId)    
                        )
                        .Keyword(kw => kw
                            .Name(nm => nm.MunicipalityFeatureId)
                        )
                        .Keyword(kw => kw
                            .Name(nm => nm.CountryRegionFeatureId)
                        )
                    )
                )
            );
            
            return createIndexResponse.Acknowledged && createIndexResponse.IsValid ? true : throw new Exception($"Failed to create user observation index. Error: {createIndexResponse.DebugInformation}");
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
        private BulkAllObserver WriteToElastic(IEnumerable<UserObservation> items)
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
                        Logger.LogError($"User observation id: {o?.Id}, Error: {r.Error.Reason}");
                    })
                )
                .Wait(TimeSpan.FromDays(1),
                    next => { Logger.LogDebug($"Indexing user observations for search:{count += next.Items.Count}"); });
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="elasticClientManager"></param>
        /// <param name="elasticConfiguration"></param>
        /// <param name="processedConfigurationCache"></param>
        /// <param name="logger"></param>
        public UserObservationRepository(
            IElasticClientManager elasticClientManager,
            ElasticSearchConfiguration elasticConfiguration,
            ICache<string, ProcessedConfiguration> processedConfigurationCache,
            ILogger<UserObservationRepository> logger) : base(true, elasticClientManager, processedConfigurationCache, elasticConfiguration, logger)
        {
            LiveMode = false;
            _id = nameof(Observation); // The active instance should be the same as the ProcessedObservationRepository which uses the Observation type.
        }

        /// <inheritdoc />
        public async Task<int> AddManyAsync(IEnumerable<UserObservation> items)
        {
            // Save valid processed data
            Logger.LogDebug($"Start indexing UserObservation batch for searching with {items.Count()} items");
            var indexResult = WriteToElastic(items);
            Logger.LogDebug("Finished indexing UserObservation batch for searching");
            if (indexResult == null || indexResult.TotalNumberOfFailedBuffers > 0) return 0;
            return items.Count();
        }

        public async Task<bool> DeleteAllDocumentsAsync()
        {
            try
            {
                var res = await Client.DeleteByQueryAsync<UserObservation>(q => q
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
                p => p.IndexSettings(g => g.RefreshInterval(1)));
        }

        /// <inheritdoc />
        public string UniqueIndexName => IndexHelper.GetIndexName<UserObservation>(IndexPrefix, true, LiveMode ? ActiveInstance : InActiveInstance, false);

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

        /// <summary>
        /// Get the number of species found for users using paging.
        /// </summary>
        /// <remarks>
        /// Known limitations: 65536 is the maximal number of users that can be handled. To increase this value, you must change
        /// some settings on the Elasticsearch servers (cluster).
        /// </remarks>
        /// <param name="filter"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<PagedResult<UserStatisticsItem>> PagedSpeciesCountSearchAsync(SpeciesCountUserStatisticsQuery filter, int? skip, int? take)
        {
            var query = filter.ToQuery<UserObservation>();
            var searchResponse = await Client.SearchAsync<UserObservation>(s => s
                .Index(IndexName)
                .Query(q => q
                    .Bool(b => b
                            .Filter(query)
                    )
                )
                .Aggregations(a => a
                    .Terms("taxaCountByUserId", t => t
                        .Size(65536)
                        .Field("userId")
                        .Aggregations(aa => aa
                            .Cardinality("taxaCount", c => c
                                .Field("taxonId")
                            )
                            .BucketSort("sortByTaxaCount", b => b
                                .Sort(s => s
                                    .Descending("taxaCount"))
                                .From(skip)
                                .Size(take)
                            )
                        )
                    )
                    .Cardinality("userCount", t => t
                        .Field("userId")
                    )
                )
                .Size(0)
                .Source(so => so.ExcludeAll())
                .TrackTotalHits(false)
            );

            if (!searchResponse.IsValid) throw new InvalidOperationException(searchResponse.DebugInformation);

            var buckets = searchResponse
                .Aggregations
                .Terms("taxaCountByUserId")
                .Buckets
                .Select(b => new UserStatisticsItem { UserId = Convert.ToInt32(b.Key), ObservationCount = Convert.ToInt32(b.DocCount ?? 0), SpeciesCount = Convert.ToInt32(b.ValueCount("taxaCount").Value) }).ToList();

            int totalCount = Convert.ToInt32(searchResponse.Aggregations.Cardinality("userCount").Value ?? 0);

            // Update skip and take
            if (skip == null)
            {
                skip = 0;
            }
            if (skip > totalCount)
            {
                skip = totalCount;
            }
            if (take == null)
            {
                take = totalCount - skip;
            }
            else
            {
                take = Math.Min(totalCount - skip.Value, take.Value);
            }

            var pagedResult = new PagedResult<UserStatisticsItem>
            {
                Records = buckets,
                Skip = skip.Value,
                Take = take.Value,
                TotalCount = Convert.ToInt32(searchResponse.Aggregations.Cardinality("userCount").Value ?? 0)
            };
            
            return pagedResult;
        }

        public async Task<List<UserStatisticsItem>> AreaSpeciesCountSearchAsync(
            SpeciesCountUserStatisticsQuery filter, List<int> userIds)
        {
            var query = filter.ToQuery<UserObservation>();
            query.TryAddTermsCriteria("userId", userIds);
            var searchResponse = await Client.SearchAsync<UserObservation>(s => s
                .Index(IndexName)
                .Query(q => q
                    .Bool(b => b
                        .Filter(query)
                    )
                )
                .Aggregations(a => a
                    .Terms("userGroup", t => t
                        .Size(65536)
                        .Field("userId")
                        .Aggregations(ag => ag
                            .Terms("provinceGroup", te => te
                                .Size(65536)
                                .Field("provinceFeatureId")
                                .Aggregations(agg => agg
                                    .Cardinality("taxaCount", c => c
                                        .Field("taxonId")
                                    )
                                )
                            )
                            .Cardinality("sumTaxaCount", c => c
                                .Field("taxonId")
                            )
                        )
                    )
                )
                .Size(0)
                .Source(so => so.ExcludeAll())
                .TrackTotalHits(false)
            );

            if (!searchResponse.IsValid) throw new InvalidOperationException(searchResponse.DebugInformation);
            
            var items = searchResponse
                .Aggregations
                .Terms("userGroup")
                .Buckets
                .Select(b => new UserStatisticsItem
                {
                    UserId = Convert.ToInt32(b.Key), 
                    ObservationCount = Convert.ToInt32(b.DocCount ?? 0),
                    SpeciesCount = Convert.ToInt32(b.Cardinality("sumTaxaCount").Value.GetValueOrDefault()),
                    SpeciesCountByFeatureId = b
                        .Terms("provinceGroup")
                        .Buckets
                        .ToDictionary(bu => bu.Key, bu => Convert.ToInt32(bu.Cardinality("taxaCount").Value.GetValueOrDefault()))
                }).ToList();
            
            return items;
        }

        /// <summary>
        /// Get species count for all users.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public async Task<List<UserStatisticsItem>> SpeciesCountSearchAsync(
            SpeciesCountUserStatisticsQuery filter)
        {
            var query = filter.ToQuery<UserObservation>();
            string indexName = IndexName;
            CompositeKey nextPageKey = null;
            var pageTaxaAsyncTake = MaxNrElasticSearchAggregationBuckets;
            var items = new List<UserStatisticsItem>();

            do
            {
                var searchResponse = await SpeciesCountSearchCompositeAggregationAsync(indexName, query, nextPageKey, pageTaxaAsyncTake);
                var compositeAgg = searchResponse.Aggregations.Composite("taxonComposite");
                foreach (var bucket in compositeAgg.Buckets)
                {
                    var userId = Convert.ToInt32((long)bucket.Key["userId"]);
                    var observationCount = Convert.ToInt32(bucket.DocCount.GetValueOrDefault(0));
                    var speciesCount = Convert.ToInt32(bucket.Cardinality("taxaCount").Value.GetValueOrDefault());
                    items.Add(new UserStatisticsItem()
                    {
                        UserId = userId,
                        SpeciesCount = speciesCount,
                        ObservationCount = observationCount
                    });
                }

                nextPageKey = compositeAgg.Buckets.Count >= pageTaxaAsyncTake ? compositeAgg.AfterKey : null;
            } while (nextPageKey != null);
            
            return items;
        }

        private async Task<ISearchResponse<dynamic>> SpeciesCountSearchCompositeAggregationAsync(
            string indexName,
            ICollection<Func<QueryContainerDescriptor<UserObservation>, QueryContainer>> query,
            CompositeKey nextPage,
            int take)
        {
            var searchResponse = await Client.SearchAsync<UserObservation>(s => s
                .Index(indexName)
                .Query(q => q
                    .Bool(b => b
                        .Filter(query)
                    )
                )
                .Aggregations(a => a.Composite("taxonComposite", g => g
                    .Size(take)
                    .After(nextPage ?? null)
                    .Sources(src => src
                        .Terms("userId", tt => tt
                            .Field("userId"))
                        )
                    .Aggregations(aa => aa
                        .Cardinality("taxaCount", c => c
                            .Field("taxonId"))
                        )
                    )
                )
                .Size(0)
                .Source(s => s.ExcludeAll())
                .TrackTotalHits(false)
            );

            if (!searchResponse.IsValid)
            {
                throw new InvalidOperationException(searchResponse.DebugInformation);
            }

            return searchResponse;
        }
    }
}