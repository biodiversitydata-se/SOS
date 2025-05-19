using Elastic.Clients.Elasticsearch.Cluster;

namespace SOS.UserStatistics.Api.Repositories.Interfaces;

public class UserStatisticsProcessedObservationRepository : ProcessedObservationCoreRepository, IUserStatisticsProcessedObservationRepository
{
    public UserStatisticsProcessedObservationRepository(
        IElasticClientManager elasticClientManager,
        ElasticSearchConfiguration elasticConfiguration,
        ICache<string, ProcessedConfiguration> processedConfigurationCache,
        ITaxonManager taxonManager,
        IClassCache<ConcurrentDictionary<string, HealthResponse>> clusterHealthCache,
        ILogger<ProcessedObservationCoreRepository> logger
    ) : base(elasticClientManager, elasticConfiguration, processedConfigurationCache, taxonManager, clusterHealthCache, logger)
    {

    }

    public async Task<PagedResult<UserStatisticsItem>> PagedSpeciesCountSearchAsync(SpeciesCountUserStatisticsQuery filter, int? skip, int? take)
    {
        var queries = filter.ToProcessedObservationQuery<Observation>();
        var searchResponse = await Client.SearchAsync<Observation>(s => s
            .Index(IndexName)
            .Query(q => q
                .Bool(b => b
                    .Filter(queries.ToArray())
                )
            )
            .Aggregations(a => a
                .Add("league", a => a
                    .Nested(n => n
                        .Path("artportalenInternal.occurrenceRecordedByInternal")
                    )
                    .Aggregations(a => a
                        .Add("taxaCountByUserId", a => a
                            .Terms(t => t
                                .Field("artportalenInternal.occurrenceRecordedByInternal.id")
                                .Size(65536)
                            )
                            .Aggregations(a => a
                                .Add("recorder", a => a
                                    .ReverseNested(rn => rn
                                        .Path(null)
                                    )
                                    .Aggregations(a => a
                                        .Add("taxaCount", a => a
                                            .Cardinality(c => c
                                                .Field("taxon.id")
                                            )
                                        )
                                    )
                                )
                                 .Add("sortByTaxaCount", a => a
                                    .BucketSort(bs => bs
                                        .Sort(s => s
                                            .Field("recorder.taxaCount".ToField(), c => c.Order(Elastic.Clients.Elasticsearch.SortOrder.Desc))
                                        )
                                        .From(skip)
                                        .Size(take)
                                    )
                                )
                            )
                        )
                        .Add("userCount", a => a
                            .Cardinality(c => c
                                .Field("artportalenInternal.occurrenceRecordedByInternal.id")
                            )
                        )
                    )
                )
            )
            .AddDefaultAggrigationSettings()
        );

        if (!searchResponse.IsValidResponse) throw new InvalidOperationException(searchResponse.DebugInformation);

        var buckets = searchResponse
            .Aggregations.GetNested("league")
                    .Aggregations.GetLongTerms("taxaCountByUserId").Buckets
                .Select(b => new UserStatisticsItem { UserId = Convert.ToInt32(b.Key), ObservationCount = Convert.ToInt32(b.DocCount), SpeciesCount = Convert.ToInt32(b.Aggregations.GetReverseNested("recorder").Aggregations.GetCardinality("taxaCount").Value) }).ToList();

        int totalCount = Convert.ToInt32(searchResponse.Aggregations.GetNested("league").Aggregations.GetCardinality("userCount").Value);

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
            TotalCount = totalCount
        };

        return pagedResult;
    }

    public async Task<List<UserStatisticsItem>> AreaSpeciesCountSearchAsync(SpeciesCountUserStatisticsQuery filter, List<int> userIds)
    {
        var queries = filter.ToProcessedObservationQuery<Observation>();
        queries.TryAddNestedTermsCriteria("artportalenInternal.occurrenceRecordedByInternal", "id", userIds);
        var searchResponse = await Client.SearchAsync<Observation>(s => s
            .Index(IndexName)
            .Query(q => q
                .Bool(b => b
                    .Filter(queries.ToArray())
                )
            )
            .Aggregations(a => a
                .Add("league", a => a
                    .Nested(n => n
                        .Path("artportalenInternal.occurrenceRecordedByInternal")
                    )
                    .Aggregations(a => a
                        .Add("taxaCountByUserId", a => a
                            .Terms(t => t
                                .Field("artportalenInternal.occurrenceRecordedByInternal.id")
                                .Size(65536) // userIds.Count?
                            )
                            .Aggregations(a => a 
                                .Add("recorder", a => a
                                    .ReverseNested(rn => rn
                                        .Path(null)
                                    )
                                    .Aggregations(a => a
                                        .Add("province", a => a
                                            .Terms(t => t
                                                .Field("location.province.featureId")
                                                .Size(33) // max number of provinces
                                            )
                                            .Aggregations(a => a
                                                .Add("taxonCount", a => a
                                                    .Cardinality(c => c
                                                        .Field("taxon.id")
                                                    )
                                                )
                                            )
                                        )
                                        .Add("sumTaxaCount", a => a
                                            .Cardinality(c => c
                                                .Field("taxon.id")
                                            )
                                        )
                                    )
                                )
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
                .GetNested("league")
                    .Aggregations
                        .GetLongTerms("taxaCountByUserId")
                            .Buckets
                                .Select(b => new UserStatisticsItem
                                {
                                    UserId = Convert.ToInt32(b.Key),
                                    ObservationCount = Convert.ToInt32(b.DocCount),
                                    SpeciesCount = Convert.ToInt32(b.Aggregations.GetReverseNested("recorder").Aggregations.GetCardinality("sumTaxaCount").Value),
                                    SpeciesCountByFeatureId = b.Aggregations.GetReverseNested("recorder").Aggregations.GetStringTerms("province").Buckets
                                        .ToDictionary(bu => bu.Key.Value.ToString(), bu => Convert.ToInt32(bu.Aggregations.GetCardinality("taxonCount").Value))
                                }
                                ).ToList();

        return items;
    }

    #region Top Lists (experimental code)

    //public async Task<PagedResult<dynamic>> TopListGetProvinceCrossLeagueAsync(SearchFilter filter, int skip, int take)
    public async Task<PagedResult<dynamic>> TopListGetProvinceCrossLeagueAsync(SpeciesCountUserStatisticsQuery filter, int skip, int take)
    {
        var queries = filter.ToProcessedObservationQuery<Observation>();
        //var (query, excludeQuery) = GetCoreQueries(filter);

        var searchResponse = await Client.SearchAsync<Observation>(s => s
           .Index(new[] { PublicIndexName, ProtectedIndexName })
           .Query(q => q
                .Bool(b => b
                    //.MustNot(excludeQuery)
                    .Filter(queries.ToArray())
                )
            )
            .Aggregations(a => a
                .Add("league", a => a
                    .Nested(n => n
                        .Path("artportalenInternal.occurrenceRecordedByInternal")
                    )
                    .Aggregations(a => a
                        .Add("recordedBy", a => a
                            .Terms(t => t
                                .Field("artportalenInternal.occurrenceRecordedByInternal.id")
                                .Size(skip + take)
                            )
                            .Aggregations(a => a
                                .Add("recorder", a => a
                                    .ReverseNested(rn => rn
                                        .Path(null)
                                    )
                                    .Aggregations(a => a
                                        .Add("province", a => a
                                            .Terms(t => t
                                                .Field("location.province.name")
                                                .Size(33)
                                            )
                                            .Aggregations(a => a
                                                .Add("taxonCount", a => a
                                                    .Cardinality(c => c
                                                        .Field("taxon.id")
                                                    )
                                                )
                                            )
                                        )
                                        .Add("taxonCount", a => a
                                            .SumBucket(sb => sb
                                                .BucketsPath("province.taxonCount")
                                            )
                                        )
                                    )
                                    .BucketSort(bs => bs
                                        .Sort(s => s
                                            .Field("recorder.taxonCount", c => c.Order(Elastic.Clients.Elasticsearch.SortOrder.Desc))
                                        )
                                    )
                                )
                            )
                        )
                        .Add("recordedByCount", a => a
                            .Cardinality(c => c
                                .Field("artportalenInternal.occurrenceRecordedByInternal.id")
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

        return new PagedResult<dynamic>
        {
            Skip = skip,
            Take = take,
            TotalCount = (long)searchResponse.Aggregations.GetNested("league").Aggregations.GetCardinality("recordedByCount").Value,
            Records = searchResponse.Aggregations.GetNested("league").Aggregations.GetLongTerms("recordedBy").Buckets.Skip(skip).Take(take).Select(b =>
                new
                {
                    Id = b.Key,
                    Provinces = b.Aggregations.GetReverseNested("recorder").Aggregations.GetStringTerms("province").Buckets.Select(b => new
                    {
                        Id = b.Key,
                        TaxonCount = (int)b.Aggregations.GetCardinality("taxonCount").Value
                    })
                }
            )
        };
    }

    public async Task<PagedResult<dynamic>> TopListGetLocationLeagueAsync(SearchFilter filter, int skip, int take)
    {
        var (queries, excludeQueries) = GetCoreQueries<dynamic>(filter);

        var searchResponse = await Client.SearchAsync<dynamic>(s => s
           .Index(new[] { PublicIndexName, ProtectedIndexName })
           .Query(q => q
                .Bool(b => b
                    .MustNot(excludeQueries.ToArray())
                    .Filter(queries.ToArray())
                )
            )
            .Aggregations(a => a
                .Add("localities", a => a
                    .Terms(t => t
                        .Field("location.locationId")
                        .Size(skip + take)
                    )
                    .Aggregations(a => a
                        .Add("taxonCount", a => a
                            .Cardinality(c => c
                                .Field("taxon.id")
                            )
                            .BucketSort(bs => bs
                                .Sort(s => s
                                    .Field("taxonCount.value", c => c.Order(Elastic.Clients.Elasticsearch.SortOrder.Desc))
                                )
                            )
                        )
                    )
                )
                .Add("localityCount", a => a
                    .Cardinality(c => c
                        .Field("location.locationId")
                    )
                )
            )
             .AddDefaultAggrigationSettings()
        );

        if (!searchResponse.IsValidResponse)
        {
            throw new InvalidOperationException(searchResponse.DebugInformation);
        }

        return new PagedResult<dynamic>
        {
            Skip = skip,
            Take = take,
            TotalCount = (long)searchResponse.Aggregations.GetCardinality("localityCount").Value,
            Records = searchResponse.Aggregations.GetStringTerms("localities").Buckets.Skip(skip).Take(take).Select(b =>
                new
                {
                    Id = b.Key,
                    TaxonCount = (int)b.Aggregations.GetCardinality("taxonCount").Value
                }
            )
        };
    }

    /// <inheritdoc/>
    public async Task<PagedResult<dynamic>> TopListGetMonthCrossLeagueAsync(SearchFilter filter, int skip, int take)
    {
        var (queries, excludeQueries) = GetCoreQueries<dynamic>(filter);

        var searchResponse = await Client.SearchAsync<dynamic>(s => s
           .Index(new[] { PublicIndexName, ProtectedIndexName })
           .Query(q => q
                .Bool(b => b
                    .MustNot(excludeQueries.ToArray())
                    .Filter(queries.ToArray())
                )
            )
            .Aggregations(a => a
                .Add("league", a => a
                    .Nested(n => n
                        .Path("artportalenInternal.occurrenceRecordedByInternal")
                    )
                    .Aggregations(a => a
                        .Add("recordedBy", a => a
                            .Terms(t => t
                                .Field("artportalenInternal.occurrenceRecordedByInternal.id")
                                .Size(skip + take)
                            )
                            .Aggregations(a => a
                                .Add("recorder", a => a
                                    .ReverseNested(rn => rn
                                        .Path(null)
                                    )
                                    .Aggregations(a => a
                                        .Add("months", a => a
                                            .Terms(t => t
                                                .Field("event.startMonth")
                                                .Size(12)
                                            )
                                            .Aggregations(a => a
                                                .Add("taxonCount", a => a
                                                    .Cardinality(c => c
                                                        .Field("taxon.id")
                                                    )
                                                )
                                            )
                                            .SumBucket(sb => sb
                                                .BucketsPath("months.taxonCount")
                                            )
                                        )
                                    )
                                    .BucketSort(bs => bs
                                        .Sort(s => s
                                            .Field("recorder.taxonCount", c => c.Order(Elastic.Clients.Elasticsearch.SortOrder.Desc))
                                        )
                                    )
                                )
                            ) 
                        )
                        .Add("recordedByCount", a => a
                            .Cardinality(c => c
                                .Field("artportalenInternal.occurrenceRecordedByInternal.id")
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

        return new PagedResult<dynamic>
        {
            Skip = skip,
            Take = take,
            Records = searchResponse.Aggregations.GetNested("league").Aggregations.GetLongTerms("recordedBy").Buckets.Skip(skip).Take(take).Select(b =>
                new
                {
                    Id = b.Key,
                    Provinces = b.Aggregations.GetReverseNested("recorder").Aggregations.GetLongTerms("months").Buckets.Select(b => new
                    {
                        Id = b.Key,
                        TaxonCount = (int)b.Aggregations.GetCardinality("taxonCount").Value
                    })
                }
            ),
            TotalCount = (long)searchResponse.Aggregations.GetNested("league").Aggregations.GetCardinality("recordedByCount").Value
        };
    }

    public async Task<PagedResult<dynamic>> TopListGetMunicipalityLeagueAsync(SearchFilter filter, int skip, int take)
    {
        var (queries, excludeQueries) = GetCoreQueries<dynamic>(filter);

        var searchResponse = await Client.SearchAsync<dynamic>(s => s
           .Index(new[] { PublicIndexName, ProtectedIndexName })
           .Query(q => q
                .Bool(b => b
                    .MustNot(excludeQueries.ToArray())
                    .Filter(queries.ToArray())
                )
            )
            .Aggregations(a => a
                .Add("municipalities", a => a
                    .Terms(t => t
                        .Field("location.municipality.featureId")
                        .Size(skip + take)
                    )
                    .Aggregations(a => a
                        .Add("taxonCount", a => a
                            .Cardinality(c => c
                                .Field("taxon.id")
                            )
                            .BucketSort(bs => bs
                                .Sort(s => s
                                    .Field("taxonCount.value", c => c.Order(Elastic.Clients.Elasticsearch.SortOrder.Desc))
                                )
                            )
                        )
                    )
                )
                .Add("municipalityCount", a => a
                    .Cardinality(c => c
                        .Field("location.municipality.featureId")
                    )
                )
            )
           .AddDefaultAggrigationSettings()
        );

        if (!searchResponse.IsValidResponse)
        {
            throw new InvalidOperationException(searchResponse.DebugInformation);
        }

        return new PagedResult<dynamic>
        {
            Skip = skip,
            Take = take,
            TotalCount = (long)searchResponse.Aggregations.GetCardinality("municipalityCount").Value,
            Records = searchResponse.Aggregations.GetStringTerms("municipalities").Buckets.Skip(skip).Take(take).Select(b =>
                new
                {
                    Id = b.Key,
                    TaxonCount = (int)b.Aggregations.GetCardinality("taxonCount").Value
                }
            )
        };
    }

    /// <inheritdoc/>
    public async Task<PagedResult<dynamic>> TopListGetProvinceLeagueAsync(SearchFilter filter, int skip, int take)
    {
        var (queries, excludeQueries) = GetCoreQueries<dynamic>(filter);

        var searchResponse = await Client.SearchAsync<dynamic>(s => s
           .Index(new[] { PublicIndexName, ProtectedIndexName })
           .Query(q => q
                .Bool(b => b
                    .MustNot(excludeQueries.ToArray())
                    .Filter(queries.ToArray())
                )
            )
            .Aggregations(a => a
                .Add("provinces", a => a
                    .Terms(t => t
                        .Field("location.province.featureId")
                        .Size(skip + take)
                    )
                    .Aggregations(a => a
                        .Add("taxonCount", a => a
                            .Cardinality(c => c
                                .Field("taxon.id")
                            )
                            .BucketSort(bs => bs
                                .Sort(s => s
                                    .Field("taxonCount.value", c => c.Order(Elastic.Clients.Elasticsearch.SortOrder.Desc))
                                )
                            )
                        )
                    )
                )
                .Add("provinceCount", a => a
                    .Cardinality(c => c
                        .Field("location.province.featureId")
                    )
                )
            )
            .AddDefaultAggrigationSettings()
        );

        if (!searchResponse.IsValidResponse)
        {
            throw new InvalidOperationException(searchResponse.DebugInformation);
        }

        return new PagedResult<dynamic>
        {
            Skip = skip,
            Take = take,
            TotalCount = (long)searchResponse.Aggregations.GetCardinality("provinceCount").Value,
            Records = searchResponse.Aggregations.GetStringTerms("provinces").Buckets.Skip(skip).Take(take).Select(b =>
                new
                {
                    Id = b.Key,
                    TaxonCount = (int)b.Aggregations.GetCardinality("taxonCount").Value
                }
            )
        };
    }

    /// <inheritdoc/>
    public async Task<PagedResult<dynamic>> TopListGetReportingVolumeLeagueAsync(SearchFilter filter, int skip, int take)
    {
        var (queries, excludeQueries) = GetCoreQueries<dynamic>(filter);

        var searchResponse = await Client.SearchAsync<dynamic>(s => s
           .Index(new[] { PublicIndexName, ProtectedIndexName })
           .Query(q => q
                .Bool(b => b
                    .MustNot(excludeQueries.ToArray())
                    .Filter(queries.ToArray())
                )
            )
            .Aggregations(a => a
                .Add("reportedBy", a => a
                    .Terms(t => t
                        .Field("artportalenInternal.reportedByUserId")
                        .Size(skip + take)
                    )
                )
                .Add("reportedByCount", a => a
                    .Cardinality(c => c
                         .Field("artportalenInternal.reportedByUserId")
                    )
                )
            )
            .AddDefaultAggrigationSettings()
        );

        if (!searchResponse.IsValidResponse)
        {
            throw new InvalidOperationException(searchResponse.DebugInformation);
        }

        return new PagedResult<dynamic>
        {
            Skip = skip,
            Take = take,
            TotalCount = (long)searchResponse.Aggregations.GetCardinality("reportedByCount").Value,
            Records = searchResponse.Aggregations.GetLongTerms("reportedBy").Buckets.Skip(skip).Take(take).Select(b =>
                new
                {
                    Id = b.Key,
                    TaxonCount = b.DocCount
                }
            )
        };
    }

    #endregion
}
