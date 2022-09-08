namespace SOS.UserStatistics.Api.Repositories.Interfaces;

public class UserStatisticsProcessedObservationRepository : ProcessedObservationRepository, IUserStatisticsProcessedObservationRepository
{
    public UserStatisticsProcessedObservationRepository(
        IElasticClientManager elasticClientManager,
        ElasticSearchConfiguration elasticConfiguration,
        ICache<string, ProcessedConfiguration> processedConfigurationCache,
        ILogger<ProcessedObservationRepository> logger
    ) : base(elasticClientManager, elasticConfiguration, processedConfigurationCache, logger)
    {

    }

    public async Task<PagedResult<UserStatisticsItem>> PagedSpeciesCountSearchAsync(SpeciesCountUserStatisticsQuery filter, int? skip, int? take)
    {
        //var testres1 = await TopListGetProvinceCrossLeagueAsync(filter, skip.GetValueOrDefault(), take.GetValueOrDefault());
        var query = filter.ToProcessedObservationQuery<Observation>();
        var searchResponse = await Client.SearchAsync<Observation>(s => s
            .Index(IndexName)
            .Query(q => q
                .Bool(b => b
                    .Filter(query)
                )
            )
            .Aggregations(a => a
                .Nested("league", n => n
                    .Path("artportalenInternal.occurrenceRecordedByInternal")
                    .Aggregations(ag => ag
                        .Terms("taxaCountByUserId", t => t
                            .Field("artportalenInternal.occurrenceRecordedByInternal.id")
                            .Size(65536)
                            .Aggregations(a => a
                                .ReverseNested("recorder", rn => rn
                                    .Aggregations(aa => aa
                                        .Cardinality("taxaCount", c => c
                                            .Field("taxon.id")
                                        )
                                    )
                                )
                                .BucketSort("sortByTaxaCount", b => b
                                    .Sort(s => s
                                        .Descending("recorder.taxaCount"))
                                    .From(skip)
                                    .Size(take)
                                )
                            )
                        )
                        .Cardinality("userCount", t => t
                            .Field("artportalenInternal.occurrenceRecordedByInternal.id")
                        )
                    )
                )
            )
            .Size(0)
            .Source(so => so.ExcludeAll())
            .TrackTotalHits(false)
        );

        if (!searchResponse.IsValid) throw new InvalidOperationException(searchResponse.DebugInformation);

        var buckets = searchResponse
            .Aggregations
            .Nested("league")
            .Terms("taxaCountByUserId")
            .Buckets
            .Select(b => new UserStatisticsItem { UserId = Convert.ToInt32(b.Key), ObservationCount = Convert.ToInt32(b.DocCount ?? 0), SpeciesCount = Convert.ToInt32(b.ReverseNested("recorder").Cardinality("taxaCount").Value) }).ToList();

        int totalCount = Convert.ToInt32(searchResponse.Aggregations.Nested("league").Cardinality("userCount").Value ?? 0);

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
        var query = filter.ToProcessedObservationQuery<Observation>();
        query.TryAddNestedTermsCriteria("artportalenInternal.occurrenceRecordedByInternal", "id", userIds);
        var searchResponse = await Client.SearchAsync<Observation>(s => s
            .Index(IndexName)
            .Query(q => q
                .Bool(b => b
                    .Filter(query)
                )
            )
            .Aggregations(a => a
                .Nested("league", n => n
                    .Path("artportalenInternal.occurrenceRecordedByInternal")
                    .Aggregations(ag => ag
                        .Terms("taxaCountByUserId", t => t
                            .Field("artportalenInternal.occurrenceRecordedByInternal.id")
                            .Size(65536) // userIds.Count?
                            .Aggregations(a => a
                                .ReverseNested("recorder", rn => rn
                                    .Aggregations(a => a
                                        .Terms("province", t => t
                                            .Field("location.province.featureId")
                                            .Size(33) // max number of provinces
                                            .Aggregations(a => a
                                                .Cardinality("taxonCount", c => c
                                                    .Field("taxon.id")
                                                )
                                            )
                                        )
                                        .Cardinality("sumTaxaCount", c => c
                                            .Field("taxon.id")
                                        )
                                    )
                                )
                            )
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
            .Nested("league")
            .Terms("taxaCountByUserId")
            .Buckets
            .Select(b => new UserStatisticsItem
            {
                UserId = Convert.ToInt32(b.Key),
                ObservationCount = Convert.ToInt32(b.DocCount ?? 0),
                SpeciesCount = Convert.ToInt32(b.ReverseNested("recorder").Cardinality("sumTaxaCount").Value),
                SpeciesCountByFeatureId = b
                    .ReverseNested("recorder")
                    .Terms("province")
                    .Buckets
                    .ToDictionary(bu => bu.Key, bu => Convert.ToInt32(bu.Cardinality("taxonCount").Value.GetValueOrDefault()))
            }).ToList();

        return items;
    }

    #region Top Lists (experimental code)

    //public async Task<PagedResult<dynamic>> TopListGetProvinceCrossLeagueAsync(SearchFilter filter, int skip, int take)
    public async Task<PagedResult<dynamic>> TopListGetProvinceCrossLeagueAsync(SpeciesCountUserStatisticsQuery filter, int skip, int take)
    {
        var query = filter.ToQuery<Observation>();
        //var (query, excludeQuery) = GetCoreQueries(filter);

        var searchResponse = await Client.SearchAsync<Observation>(s => s
           .Index(new[] { PublicIndexName, ProtectedIndexName })
           .Query(q => q
                .Bool(b => b
                    //.MustNot(excludeQuery)
                    .Filter(query)
                )
            )
            .Aggregations(a => a
                .Nested("league", n => n
                    .Path("artportalenInternal.occurrenceRecordedByInternal")
                    .Aggregations(a => a
                        .Terms("recordedBy", t => t
                            .Field("artportalenInternal.occurrenceRecordedByInternal.id")
                            .Size(skip + take)
                            .Aggregations(a => a
                                .ReverseNested("recorder", rn => rn
                                    .Aggregations(a => a
                                        .Terms("province", t => t
                                            .Field("location.province.name")
                                            .Size(33)
                                            .Aggregations(a => a
                                                .Cardinality("taxonCount", c => c
                                                    .Field("taxon.id")
                                                )
                                            )
                                        )
                                        .SumBucket("taxonCount", sb => sb
                                            .BucketsPath("province.taxonCount")
                                        )
                                    )
                                )
                                .BucketSort("leagueSort", bs => bs
                                    .Sort(s => s
                                        .Descending("recorder.taxonCount")
                                    )
                                )
                            )
                        )
                        .Cardinality("recordedByCount", c => c
                            .Field("artportalenInternal.occurrenceRecordedByInternal.id")
                        )
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

        return new PagedResult<dynamic>
        {
            Skip = skip,
            Take = take,
            TotalCount = (long)searchResponse.Aggregations.Nested("league").Cardinality("recordedByCount").Value,
            Records = searchResponse.Aggregations.Nested("league").Terms("recordedBy").Buckets.Skip(skip).Take(take).Select(b =>
                new
                {
                    Id = b.Key,
                    Provinces = b.ReverseNested("recorder").Terms("province").Buckets.Select(b => new
                    {
                        Id = b.Key,
                        TaxonCount = (int)b.Cardinality("taxonCount").Value
                    })
                }
            )
        };
    }

    public async Task<PagedResult<dynamic>> TopListGetLocationLeagueAsync(SearchFilter filter, int skip, int take)
    {
        var (query, excludeQuery) = GetCoreQueries(filter);

        var searchResponse = await Client.SearchAsync<dynamic>(s => s
           .Index(new[] { PublicIndexName, ProtectedIndexName })
           .Query(q => q
                .Bool(b => b
                    .MustNot(excludeQuery)
                    .Filter(query)
                )
            )
            .Aggregations(a => a
                .Terms("localities", c => c
                    .Field("location.locationId")
                    .Size(skip + take)
                    .Aggregations(a => a
                        .Cardinality("taxonCount", c => c
                            .Field("taxon.id")
                        )
                        .BucketSort("sort", bs => bs
                            .Sort(s => s
                                .Descending("taxonCount.value")
                            )
                        )
                    )
                )
                .Cardinality("localityCount", c => c
                    .Field("location.locationId")
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

        return new PagedResult<dynamic>
        {
            Skip = skip,
            Take = take,
            TotalCount = (long)searchResponse.Aggregations.Cardinality("localityCount").Value,
            Records = searchResponse.Aggregations.Terms("localities").Buckets.Skip(skip).Take(take).Select(b =>
                new
                {
                    Id = b.Key,
                    TaxonCount = (int)b.Cardinality("taxonCount").Value
                }
            )
        };
    }

    /// <inheritdoc/>
    public async Task<PagedResult<dynamic>> TopListGetMonthCrossLeagueAsync(SearchFilter filter, int skip, int take)
    {
        var (query, excludeQuery) = GetCoreQueries(filter);

        var searchResponse = await Client.SearchAsync<dynamic>(s => s
           .Index(new[] { PublicIndexName, ProtectedIndexName })
           .Query(q => q
                .Bool(b => b
                    .MustNot(excludeQuery)
                    .Filter(query)
                )
            )
            .Aggregations(a => a
                .Nested("league", n => n
                    .Path("artportalenInternal.occurrenceRecordedByInternal")
                    .Aggregations(a => a
                        .Terms("recordedBy", t => t
                            .Field("artportalenInternal.occurrenceRecordedByInternal.id")
                            .Size(skip + take)
                            .Aggregations(a => a
                                .ReverseNested("recorder", rn => rn
                                    .Aggregations(a => a
                                        .Terms("months", t => t
                                            .Field("event.startMonth")
                                            .Size(12)
                                            .Aggregations(a => a
                                                .Cardinality("taxonCount", c => c
                                                    .Field("taxon.id")
                                                )
                                            )
                                        )
                                        .SumBucket("taxonCount", sb => sb
                                            .BucketsPath("months.taxonCount")
                                        )
                                    )
                                )
                                .BucketSort("leagueSort", bs => bs
                                    .Sort(s => s
                                        .Descending("recorder.taxonCount")
                                    )
                                )
                            )
                        )
                        .Cardinality("recordedByCount", c => c
                            .Field("artportalenInternal.occurrenceRecordedByInternal.id")
                        )
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

        return new PagedResult<dynamic>
        {
            Skip = skip,
            Take = take,
            Records = searchResponse.Aggregations.Nested("league").Terms("recordedBy").Buckets.Skip(skip).Take(take).Select(b =>
                new
                {
                    Id = b.Key,
                    Provinces = b.ReverseNested("recorder").Terms("months").Buckets.Select(b => new
                    {
                        Id = b.Key,
                        TaxonCount = (int)b.Cardinality("taxonCount").Value
                    })
                }
            ),
            TotalCount = (long)searchResponse.Aggregations.Nested("league").Cardinality("recordedByCount").Value
        };
    }

    public async Task<PagedResult<dynamic>> TopListGetMunicipalityLeagueAsync(SearchFilter filter, int skip, int take)
    {
        var (query, excludeQuery) = GetCoreQueries(filter);

        var searchResponse = await Client.SearchAsync<dynamic>(s => s
           .Index(new[] { PublicIndexName, ProtectedIndexName })
           .Query(q => q
                .Bool(b => b
                    .MustNot(excludeQuery)
                    .Filter(query)
                )
            )
            .Aggregations(a => a
                .Terms("municipalities", c => c
                    .Field("location.municipality.featureId")
                    .Size(skip + take)
                    .Aggregations(a => a
                        .Cardinality("taxonCount", c => c
                            .Field("taxon.id")
                        )
                        .BucketSort("sort", bs => bs
                            .Sort(s => s
                                .Descending("taxonCount.value")
                            )
                        )
                    )
                )
                .Cardinality("municipalityCount", c => c
                    .Field("location.municipality.featureId")
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

        return new PagedResult<dynamic>
        {
            Skip = skip,
            Take = take,
            TotalCount = (long)searchResponse.Aggregations.Cardinality("municipalityCount").Value,
            Records = searchResponse.Aggregations.Terms("municipalities").Buckets.Skip(skip).Take(take).Select(b =>
                new
                {
                    Id = b.Key,
                    TaxonCount = (int)b.Cardinality("taxonCount").Value
                }
            )
        };
    }

    /// <inheritdoc/>
    public async Task<PagedResult<dynamic>> TopListGetProvinceLeagueAsync(SearchFilter filter, int skip, int take)
    {
        var (query, excludeQuery) = GetCoreQueries(filter);

        var searchResponse = await Client.SearchAsync<dynamic>(s => s
           .Index(new[] { PublicIndexName, ProtectedIndexName })
           .Query(q => q
                .Bool(b => b
                    .MustNot(excludeQuery)
                    .Filter(query)
                )
            )
            .Aggregations(a => a
                .Terms("provinces", c => c
                    .Field("location.province.featureId")
                    .Size(skip + take)
                    .Aggregations(a => a
                        .Cardinality("taxonCount", c => c
                            .Field("taxon.id")
                        )
                        .BucketSort("sort", bs => bs
                            .Sort(s => s
                                .Descending("taxonCount.value")
                            )
                        )
                    )
                )
                .Cardinality("provinceCount", c => c
                    .Field("location.province.featureId")
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

        return new PagedResult<dynamic>
        {
            Skip = skip,
            Take = take,
            TotalCount = (long)searchResponse.Aggregations.Cardinality("provinceCount").Value,
            Records = searchResponse.Aggregations.Terms("provinces").Buckets.Skip(skip).Take(take).Select(b =>
                new
                {
                    Id = b.Key,
                    TaxonCount = (int)b.Cardinality("taxonCount").Value
                }
            )
        };
    }

    /// <inheritdoc/>
    public async Task<PagedResult<dynamic>> TopListGetReportingVolumeLeagueAsync(SearchFilter filter, int skip, int take)
    {
        var (query, excludeQuery) = GetCoreQueries(filter);

        var searchResponse = await Client.SearchAsync<dynamic>(s => s
           .Index(new[] { PublicIndexName, ProtectedIndexName })
           .Query(q => q
                .Bool(b => b
                    .MustNot(excludeQuery)
                    .Filter(query)
                )
            )
            .Aggregations(a => a
                .Terms("reportedBy", c => c
                    .Field("artportalenInternal.reportedByUserId")
                    .Size(skip + take)
                )
                .Cardinality("reportedByCount", c => c
                    .Field("artportalenInternal.reportedByUserId")
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

        return new PagedResult<dynamic>
        {
            Skip = skip,
            Take = take,
            TotalCount = (long)searchResponse.Aggregations.Cardinality("reportedByCount").Value,
            Records = searchResponse.Aggregations.Terms("reportedBy").Buckets.Skip(skip).Take(take).Select(b =>
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
