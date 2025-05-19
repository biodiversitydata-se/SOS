using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Cluster;
using Elastic.Clients.Elasticsearch.QueryDsl;

namespace SOS.UserStatistics.Api.Repositories.Interfaces;

public class UserStatisticsObservationRepository : UserObservationRepository, IUserStatisticsObservationRepository
{
    private async Task<SearchResponse<UserObservation>> SpeciesCountSearchCompositeAggregationAsync(
        string indexName,
        ICollection<Action<QueryDescriptor<UserObservation>>> queries,
        IReadOnlyDictionary<string, FieldValue>? afterKey = null,
        int take = 0)
    {
        var searchResponse = await Client.SearchAsync<UserObservation>(s => s
            .Index(indexName)
            .Query(q => q
                .Bool(b => b
                    .Filter(queries.ToArray())
                )
            )
            .Aggregations(a => a
                .Add("taxonComposite", a => a
                    .Composite(c => c
                        .After(ak => afterKey?.ToFluentFieldDictionary())
                        .Size(take)
                        .Sources(
                            [
                                CreateCompositeTermsAggregationSource(
                                    ("userId", "userId", SortOrder.Asc)
                                )
                            ]
                        )
                    )
                    .Aggregations(a => a
                        .Add("taxaCount", a => a
                            .Cardinality(c => c
                                .Field("taxonId")
                            )
                        )
                    )
                )
            )
            .AddDefaultAggrigationSettings()
        );

        if (!searchResponse.IsValidResponse)
        {
            throw new InvalidOperationException(searchResponse.DebugInformation);
        }

        return searchResponse;
    }

    private async Task<SearchResponse<UserObservation>> AreaSpeciesCountSearchCompositeAggregationAsync(
        string indexName,
        ICollection<Action<QueryDescriptor<UserObservation>>> queries,
        IReadOnlyDictionary<string, FieldValue>? afterKey = null,
        int take = 0)
    {
        var searchResponse = await Client.SearchAsync<UserObservation>(s => s
            .Index(indexName)
            .Query(q => q
                .Bool(b => b
                    .Filter(queries.ToArray())
                )
            )
            .Aggregations(a => a
                .Add("taxonComposite", a => a
                    .Composite(c => c
                        .After(ak => afterKey?.ToFluentFieldDictionary())
                        .Size(take)
                        .Sources(
                            [
                                CreateCompositeTermsAggregationSource(
                                    ("userId", "userId", SortOrder.Asc)
                                ),
                                CreateCompositeTermsAggregationSource(
                                    ("provinceId", "provinceFeatureId", SortOrder.Asc)
                                )
                            ]
                        )
                    )
                    .Aggregations(a => a
                        .Add("taxaCount", a => a
                            .Cardinality(c => c
                                .Field("taxonId")
                            )
                        )
                    )
                )
            )
            .AddDefaultAggrigationSettings()
        );

        if (!searchResponse.IsValidResponse)
        {
            throw new InvalidOperationException(searchResponse.DebugInformation);
        }

        return searchResponse;
    }

    public UserStatisticsObservationRepository(
        IElasticClientManager elasticClientManager,
        ElasticSearchConfiguration elasticConfiguration,
        ICache<string, ProcessedConfiguration> processedConfigurationCache,
        IClassCache<ConcurrentDictionary<string, HealthResponse>> clusterHealthCache,
        ILogger<UserObservationRepository> logger) 
        : base(elasticClientManager, elasticConfiguration, processedConfigurationCache, clusterHealthCache, logger)
    {
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
        var queries = filter.ToQuery<UserObservation>();
        var searchResponse = await Client.SearchAsync<UserObservation>(s => s
            .Index(IndexName)
            .Query(q => q
                .Bool(b => b
                    .Filter(queries.ToArray())
                )
            )
            .Aggregations(a => a
                .Add("taxaCountByUserId", a => a
                    .Terms(t => t
                        .Size(65536)
                        .Field("userId")
                    )
                    .Aggregations(a => a
                        .Add("taxaCount", a => a
                            .Cardinality(c => c
                                .Field("taxonId")
                            )
                            .BucketSort(bs => bs
                                .Sort(s => s
                                    .Field("taxaCount".ToField(), c => c.Order(Elastic.Clients.Elasticsearch.SortOrder.Desc))
                                )
                                .From(skip)
                                .Size(take)
                            )
                        )
                    )
                )
                .Add("userCount", a => a
                    .Cardinality(c => c
                        .Field("userId")
                    )
                )
            )
            .AddDefaultAggrigationSettings()
        );

        if (!searchResponse.IsValidResponse) throw new InvalidOperationException(searchResponse.DebugInformation);

        var buckets = searchResponse
            .Aggregations
            .GetLongTerms("taxaCountByUserId")
            .Buckets
            .Select(b => new UserStatisticsItem { 
                UserId = Convert.ToInt32(b.Key), 
                ObservationCount = Convert.ToInt32(b.DocCount), 
                SpeciesCount = Convert.ToInt32(b.Aggregations.GetCardinality("taxaCount").Value) 
            }).ToList();

        int totalCount = Convert.ToInt32(searchResponse.Aggregations.GetCardinality("userCount").Value);

        // Update skip and take
        skip ??= 0;
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
            TotalCount = totalCount
        };

        return pagedResult;
    }

    public async Task<List<UserStatisticsItem>> AreaSpeciesCountSearchAsync(
        SpeciesCountUserStatisticsQuery filter, List<int> userIds)
    {
        var queries = filter.ToQuery<UserObservation>();
        queries.TryAddTermsCriteria("userId", userIds);
        var searchResponse = await Client.SearchAsync<UserObservation>(s => s
            .Index(IndexName)
            .Query(q => q
                .Bool(b => b
                    .Filter(queries.ToArray())
                )
            )
            .Aggregations(a => a
                .Add("userGroup", a => a
                    .Terms(t => t
                        .Size(65536)
                        .Field("userId")
                    )
                    .Aggregations(a => a
                        .Add("provinceGroup", a => a
                            .Terms(t => t
                                .Size(65536)
                                .Field("provinceFeatureId")
                            )
                            .Aggregations(a => a
                                .Add("taxaCount", a => a
                                    .Cardinality(c => c
                                         .Field("taxonId")
                                    )
                                )
                            ) 
                        )
                        .Add("sumTaxaCount", a => a
                             .Cardinality(c => c
                                .Field("taxonId")
                            )
                        )
                    )
                )
            )
            .AddDefaultAggrigationSettings()
        );

        if (!searchResponse.IsValidResponse) throw new InvalidOperationException(searchResponse.DebugInformation);

        var items = searchResponse
            .Aggregations
            .GetLongTerms("userGroup")
            .Buckets
            .Select(b => new UserStatisticsItem
            {
                UserId = Convert.ToInt32(b.Key),
                ObservationCount = Convert.ToInt32(b.DocCount),
                SpeciesCount = Convert.ToInt32(b.Aggregations.GetCardinality("sumTaxaCount").Value),
                SpeciesCountByFeatureId = b.Aggregations.GetStringTerms("provinceGroup")
                    .Buckets
                    .ToDictionary(bu => bu.Key.Value.ToString(), bu => Convert.ToInt32(bu.Aggregations.GetCardinality("taxaCount").Value))
            }).ToList();

        return items;
    }

    public async Task<List<UserStatisticsItem>> SpeciesCountSearchAsync(
        SpeciesCountUserStatisticsQuery filter, List<int> userIds = null)
    {
        var queries = filter.ToQuery<UserObservation>();
        queries.TryAddTermsCriteria("userId", userIds);
        string indexName = IndexName;
        IReadOnlyDictionary<string, FieldValue>? afterKey = null;
        var pageTaxaAsyncTake = MaxNrElasticSearchAggregationBuckets;
        var items = new List<UserStatisticsItem>();
        var searchResponse = await SpeciesCountSearchCompositeAggregationAsync(indexName, queries, afterKey, pageTaxaAsyncTake);
        var compositeAggregation = searchResponse.Aggregations.GetComposite("taxonComposite");

        while (compositeAggregation.Buckets.Count > 0)
        {
            foreach (var bucket in compositeAggregation.Buckets)
            {
                items.Add(new UserStatisticsItem()
                {
                    UserId = Convert.ToInt32((long)bucket.Key["userId"].Value),
                    SpeciesCount = Convert.ToInt32(bucket.Aggregations.GetCardinality("taxaCount").Value),
                    ObservationCount = Convert.ToInt32(bucket.DocCount)
                });
            }

            searchResponse = await SpeciesCountSearchCompositeAggregationAsync(indexName, queries, compositeAggregation.AfterKey, pageTaxaAsyncTake);
            compositeAggregation = searchResponse.Aggregations.GetComposite("taxonComposite");
        }

        var sortedItems = items
            .OrderByDescending(m => m.SpeciesCount)
            .ThenBy(m => m.UserId)
            .ToList();
        return sortedItems;
    }

    

    public async Task<List<UserStatisticsItem>> AreaSpeciesCountSearchCompositeAsync(
        SpeciesCountUserStatisticsQuery filter, List<int> userIds)
    {
        var queries = filter.ToQuery<UserObservation>();
        queries.TryAddTermsCriteria("userId", userIds);
        string indexName = IndexName;
        IReadOnlyDictionary<string, FieldValue>? afterKey = null;
        var pageTaxaAsyncTake = MaxNrElasticSearchAggregationBuckets;
        var userStatisticsByUserId = new Dictionary<int, UserStatisticsItem>();
        var searchResponse = await AreaSpeciesCountSearchCompositeAggregationAsync(indexName, queries, afterKey, pageTaxaAsyncTake);
        var compositeAggregation = searchResponse.Aggregations.GetComposite("taxonComposite");

        while (compositeAggregation.Buckets.Count > 0)
        {
            foreach (var bucket in compositeAggregation.Buckets)
            {
                var userId = Convert.ToInt32((long)bucket.Key["userId"].Value);
                var provinceId = (string)bucket.Key["provinceId"].Value;
                var observationCount = Convert.ToInt32(bucket.DocCount);
                var speciesCount = Convert.ToInt32(bucket.Aggregations.GetCardinality("taxaCount").Value);

                UserStatisticsItem item;
                if (!userStatisticsByUserId.ContainsKey(userId))
                {
                    item = new UserStatisticsItem
                    {
                        UserId = userId, 
                        SpeciesCountByFeatureId = new Dictionary<string, int>() 
                    };
                    userStatisticsByUserId.Add(userId, item);
                }
                else
                {
                    item = userStatisticsByUserId[userId];
                }

                item.SpeciesCountByFeatureId.Add(provinceId, speciesCount);
            }

            searchResponse = await AreaSpeciesCountSearchCompositeAggregationAsync(indexName, queries, compositeAggregation.AfterKey, pageTaxaAsyncTake);
            compositeAggregation = searchResponse.Aggregations.GetComposite("taxonComposite");
        }

        // Calculate sum
        List<UserStatisticsItem> sumItems = await SpeciesCountSearchAsync(filter, userIds);
        Dictionary<int, UserStatisticsItem> sumItemsByUserId = sumItems.ToDictionary(m => m.UserId, m => m);
        foreach (var pair in userStatisticsByUserId)
        {
            var sumItem = sumItemsByUserId[pair.Key];
            pair.Value.ObservationCount = sumItem.ObservationCount;
            pair.Value.SpeciesCount = sumItem.SpeciesCount;
        }

        return userStatisticsByUserId.Values.ToList();
    }

    
}
