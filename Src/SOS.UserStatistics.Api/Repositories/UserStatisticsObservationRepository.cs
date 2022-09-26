namespace SOS.UserStatistics.Api.Repositories.Interfaces;

public class UserStatisticsObservationRepository : UserObservationRepository, IUserStatisticsObservationRepository
{
    public UserStatisticsObservationRepository(
        IElasticClientManager elasticClientManager,
        ElasticSearchConfiguration elasticConfiguration,
        ICache<string, ProcessedConfiguration> processedConfigurationCache,
        ILogger<UserObservationRepository> logger) 
        : base(elasticClientManager, elasticConfiguration, processedConfigurationCache, logger)
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

    public async Task<List<UserStatisticsItem>> SpeciesCountSearchAsync(
        SpeciesCountUserStatisticsQuery filter, List<int> userIds = null)
    {
        var query = filter.ToQuery<UserObservation>();
        query.TryAddTermsCriteria("userId", userIds);
        string indexName = IndexName;
        CompositeKey nextPageKey = null;
        var pageTaxaAsyncTake = MaxNrElasticSearchAggregationBuckets;
        var items = new List<UserStatisticsItem>();
        CompositeBucketAggregate compositeAgg = null;

        while (compositeAgg == null || compositeAgg.Buckets.Count >= pageTaxaAsyncTake)
        {
            var searchResponse = await SpeciesCountSearchCompositeAggregationAsync(indexName, query, nextPageKey, pageTaxaAsyncTake);
            compositeAgg = searchResponse.Aggregations.Composite("taxonComposite");
            foreach (var bucket in compositeAgg.Buckets)
            {
                items.Add(new UserStatisticsItem()
                {
                    UserId = Convert.ToInt32((long)bucket.Key["userId"]),
                    SpeciesCount = Convert.ToInt32(bucket.Cardinality("taxaCount").Value.GetValueOrDefault()),
                    ObservationCount = Convert.ToInt32(bucket.DocCount.GetValueOrDefault(0))
                });
            }

            nextPageKey = compositeAgg.AfterKey;
        }

        var sortedItems = items
            .OrderByDescending(m => m.SpeciesCount)
            .ThenBy(m => m.UserId)
            .ToList();
        return sortedItems;
    }

    private async Task<ISearchResponse<UserObservation>> SpeciesCountSearchCompositeAggregationAsync(
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

    public async Task<List<UserStatisticsItem>> AreaSpeciesCountSearchCompositeAsync(
        SpeciesCountUserStatisticsQuery filter, List<int> userIds)
    {
        var query = filter.ToQuery<UserObservation>();
        query.TryAddTermsCriteria("userId", userIds);
        string indexName = IndexName;
        CompositeKey nextPageKey = null;
        var pageTaxaAsyncTake = MaxNrElasticSearchAggregationBuckets;
        var userStatisticsByUserId = new Dictionary<int, UserStatisticsItem>();
        CompositeBucketAggregate compositeAgg = null;

        while (compositeAgg == null || compositeAgg.Buckets.Count >= pageTaxaAsyncTake)
        {
            var searchResponse = await AreaSpeciesCountSearchCompositeAggregationAsync(indexName, query, nextPageKey, pageTaxaAsyncTake);
            compositeAgg = searchResponse.Aggregations.Composite("taxonComposite");
            foreach (var bucket in compositeAgg.Buckets)
            {
                var userId = Convert.ToInt32((long)bucket.Key["userId"]);
                var provinceId = (string)bucket.Key["provinceId"];
                var observationCount = Convert.ToInt32(bucket.DocCount.GetValueOrDefault(0));
                var speciesCount = Convert.ToInt32(bucket.Cardinality("taxaCount").Value.GetValueOrDefault());

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

            nextPageKey = compositeAgg.AfterKey;
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

    private async Task<ISearchResponse<UserObservation>> AreaSpeciesCountSearchCompositeAggregationAsync(
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
                        .Terms("provinceId", p => p
                            .Field("provinceFeatureId"))
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
