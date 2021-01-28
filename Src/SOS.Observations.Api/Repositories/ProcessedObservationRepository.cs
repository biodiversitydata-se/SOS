using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Logging;
using Nest;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Gis;
using SOS.Lib.Models.Processed.Configuration;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search;
using SOS.Lib.Repositories.Processed;
using SOS.Observations.Api.Exceptions;
using SOS.Observations.Api.Extensions;
using SOS.Observations.Api.Models.AggregatedResult;
using SOS.Observations.Api.Repositories.Interfaces;
using Result = CSharpFunctionalExtensions.Result;

namespace SOS.Observations.Api.Repositories
{
    /// <summary>
    ///     Species data service
    /// </summary>
    public class ProcessedObservationRepository : ProcessRepositoryBase<Observation>,
        IProcessedObservationRepository
    {
        private const int ElasticSearchMaxRecords = 10000;
        private readonly IElasticClient _elasticClient;
        private readonly ElasticSearchConfiguration _elasticConfiguration;
        private readonly TelemetryClient _telemetry;

        private Tuple<ICollection<Func<QueryContainerDescriptor<dynamic>, QueryContainer>>, 
            ICollection<Func<QueryContainerDescriptor<object>, QueryContainer>>> GetCoreQueries(FilterBase filter)
        {
            var query = filter.ToQuery();
            var excludeQuery = filter.ToExcludeQuery();

            return new Tuple<ICollection<Func<QueryContainerDescriptor<dynamic>, QueryContainer>>, 
                ICollection<Func<QueryContainerDescriptor<object>, QueryContainer>>>(query, excludeQuery);
        }

        /// <summary>
        /// Get public index name and also protected index name if user is authorized
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        private string GetIndexNames(FilterBase filter)
        {
            var publicIndex = $"{(string.IsNullOrEmpty(_elasticConfiguration.IndexPrefix) ? "" : $"{_elasticConfiguration.IndexPrefix}-")}{GetInstanceName(ActiveInstance, false)}".ToLower();

            if (!filter.ProtectedObservations)
            {
                return publicIndex;
            }           

            if(filter.ProtectedObservations && (!filter?.ExtendedAuthorizations?.Any() ?? true))
            {
                throw new AuthenticationRequiredException("Not authorized");
            }
            
            var protectedIndex = $"{(string.IsNullOrEmpty(_elasticConfiguration.IndexPrefix) ? "" : $"{_elasticConfiguration.IndexPrefix}-")}{GetInstanceName(ActiveInstance, true)}".ToLower();

            return protectedIndex;
        }

        public int MaxNrElasticSearchAggregationBuckets => _elasticConfiguration.MaxNrAggregationBuckets;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="elasticClient"></param>
        /// <param name="client"></param>
        /// <param name="elasticConfiguration"></param>
        /// <param name="telemetry"></param>
        /// <param name="logger"></param>
        /// <param name="processedConfigurationCache"></param>
        public ProcessedObservationRepository(
            IElasticClient elasticClient,
            IProcessClient client,
            ElasticSearchConfiguration elasticConfiguration,
            TelemetryClient telemetry,
            ILogger<ProcessedObservationRepository> logger,
            IClassCache<ProcessedConfiguration> processedConfigurationCache) : base(client, true, logger, processedConfigurationCache)
        {
            LiveMode = true;

            _elasticConfiguration = elasticConfiguration ?? throw new ArgumentNullException(nameof(elasticConfiguration));
            _elasticClient = elasticClient ?? throw new ArgumentNullException(nameof(elasticClient));
            _telemetry = telemetry ?? throw new ArgumentNullException(nameof(telemetry));
        }

        /// <inheritdoc />
        public async Task<PagedResult<dynamic>> GetChunkAsync(SearchFilter filter, int skip, int take, string sortBy,
            SearchSortOrder sortOrder)
        {
            var indexNames = GetIndexNames(filter);
            var (query, excludeQuery) = GetCoreQueries(filter);

            var sortDescriptor = sortBy.ToSortDescriptor<Observation>(sortOrder);
            using var operation = _telemetry.StartOperation<DependencyTelemetry>("Observation_Search");

            operation.Telemetry.Properties["Filter"] = filter.ToString();

            var searchResponse = await _elasticClient.SearchAsync<dynamic>(s => s
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

            if (filter is SearchFilterInternal)
            {
                var internalFilter = filter as SearchFilterInternal;
                includeRealCount = internalFilter.IncludeRealCount;
            }

            if (includeRealCount)
            {
                var countResponse = await _elasticClient.CountAsync<dynamic>(s => s
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

        /// <inheritdoc />
        public async Task<PagedResult<dynamic>> GetAggregatedHistogramChunkAsync(SearchFilter filter, AggregationType aggregationType)
        {
            var indexNames = GetIndexNames(filter);
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

            var searchResponse = await _elasticClient.SearchAsync<dynamic>(s => s
                .Size(0)
                .Index(indexNames)
                .Source(filter.OutputFields.ToProjection(filter is SearchFilterInternal))
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
        public async Task<PagedResult<dynamic>> GetAggregatedChunkAsync(SearchFilter filter, AggregationType aggregationType, int skip, int take)
        {
            var indexNames = GetIndexNames(filter);
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
                        .Source("doc['taxon.sortOrder'].value + '-' + doc['taxon.scientificName'].value")
                    )
                    .Order(o => o.KeyAscending())
                    .Aggregations(thAgg => thAgg
                        .TopHits("info", info => info
                            .Size(1)
                            .Source(src => src
                                .Includes(inc => inc
                                    .Fields("taxon.id", "taxon.scientificName", "taxon.vernacularName", "taxon.scientificNameAuthorship", "taxon.redlistCategory")
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
            var searchResponseCount = await _elasticClient.SearchAsync<dynamic>(s => s
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
            var size = skip + take < maxResult ? skip + take : maxResult;
            if (skip == 0 && take == 0)
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
            var searchResponse = await _elasticClient.SearchAsync<dynamic>(s => s
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
        public async Task<DateTime> GetLatestModifiedDateForProviderAsync(int providerId)
        {
            var indexNames = GetIndexNames(null);
          
            try
            {
                var res = await _elasticClient.SearchAsync<Observation>(s => s
                    .Index(indexNames)
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
                Logger.LogError(e, $"Failed to get last modified date for provider: { providerId }, index: { indexNames }");
                return DateTime.MinValue;
            }
        }

        /// <inheritdoc />
        public async Task<long> GetMatchCountAsync(FilterBase filter)
        {
            var indexNames = GetIndexNames(filter);
            var (query, excludeQuery) = GetCoreQueries(filter);

            var countResponse = await _elasticClient.CountAsync<dynamic>(s => s
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

        public async Task<Result<PagedResult<TaxonAggregationItem>>> GetTaxonAggregationAsync(
            SearchFilter filter, 
            LatLonBoundingBox bbox,
            int skip,
            int take)
        {
            var indexNames = GetIndexNames(filter);
            var (query, excludeQuery) = GetCoreQueries(filter);


            using var operation = _telemetry.StartOperation<DependencyTelemetry>("Observation_Search_TaxonAggregation");
            operation.Telemetry.Properties["Filter"] = filter.ToString();

            // Get number of taxa
            var searchResponseCount = await _elasticClient.SearchAsync<dynamic>(s => s
                .Index(indexNames)
                .Size(0)
                .Aggregations(a => a.Filter("bbox_filter", f => f
                    .Filter(fq => fq.GeoBoundingBox(b => b
                        .Field("location.pointLocation")
                        .BoundingBox(box => box.TopLeft(bbox.TopLeft.ToGeoLocation()).BottomRight(bbox.BottomRight.ToGeoLocation())
                        )))
                    .Aggregations(ac => ac.Cardinality("taxa_count", c => c
                        .Field("taxon.id")
                        .PrecisionThreshold(40000)
                        )))
                )
                .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQuery)
                        .Filter(query)
                    )
                )
            );
            int? nrTaxa = (int?)searchResponseCount.Aggregations.Filter("bbox_filter").Cardinality("taxa_count").Value;
            if (!nrTaxa.HasValue || nrTaxa == 0)
            {
                return Result.Success(new PagedResult<TaxonAggregationItem>
                {
                    Records = Enumerable.Empty<TaxonAggregationItem>(),
                    Skip = skip,
                    Take = take,
                    TotalCount = 0
                });
            }

            // Calculate size to fetch. 
            var size = skip + take < nrTaxa ? skip + take : nrTaxa;
            if (size > MaxNrElasticSearchAggregationBuckets)
            {
                return Result.Failure<PagedResult<TaxonAggregationItem>>($"Skip+Take={skip + take}. Skip+Take must be less than or equal to {MaxNrElasticSearchAggregationBuckets}.");
            }
            
            // Get observation count for each taxon
            var searchResponse = await _elasticClient.SearchAsync<dynamic>(s => s
                .Index(indexNames)
                .Size(0)
                .Aggregations(a => a.Filter("bbox_filter", f => f
                    .Filter(fq => fq.GeoBoundingBox(b => b
                        .Field("location.pointLocation")
                        .BoundingBox(box => box.TopLeft(bbox.TopLeft.ToGeoLocation()).BottomRight(bbox.BottomRight.ToGeoLocation())
                        )))
                    .Aggregations(ac => ac.Terms("taxa_count", t => t
                        .Field("taxon.id")
                        .Size(size)
                        .Order(o => o.CountDescending().KeyAscending())
                    )))
                )
                .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQuery)
                        .Filter(query)
                    )
                )
            );

            if (!searchResponse.IsValid) throw new InvalidOperationException(searchResponse.DebugInformation);

            _telemetry.StopOperation(operation);

            IEnumerable<TaxonAggregationItem> observationCountByTaxon = searchResponse
                .Aggregations
                .Filter("bbox_filter")
                .Terms("taxa_count")
                .Buckets
                .Select(b => TaxonAggregationItem.Create(int.Parse(b.Key), Convert.ToInt32(b.DocCount)))
                .Skip(skip)
                .Take(take);

            var pagedResult = new PagedResult<TaxonAggregationItem>
            {
                Records = observationCountByTaxon,
                Skip = skip,
                Take = take,
                TotalCount = nrTaxa.Value
            };

            return Result.Success(pagedResult);
        }


        public async Task<Result<GeoGridResult>> GetGeogridAggregationAsync(
                SearchFilter filter,
                int precision,
                LatLonBoundingBox bbox)
        {
            const int maxNrReturnedBuckets = 10000;
            var indexNames = GetIndexNames(filter);
            var (query, excludeQuery) = GetCoreQueries(filter);

            using var operation = _telemetry.StartOperation<DependencyTelemetry>("Observation_Search_GeoAggregated");
            operation.Telemetry.Properties["Filter"] = filter.ToString();

            var searchResponse = await _elasticClient.SearchAsync<dynamic>(s => s
                .Index(indexNames)
                .Size(0)
                .Aggregations(a => a.GeoHash("geohash_grid", g => g
                    .Field("location.pointLocation")
                    .Size(maxNrReturnedBuckets + 1)
                    .GeoHashPrecision((GeoHashPrecision)precision)
                    .Bounds(b => b.TopLeft(bbox.TopLeft.ToGeoLocation()).BottomRight(bbox.BottomRight.ToGeoLocation()))
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
                    return Result.Failure<GeoGridResult>($"The number of cells that will be returned is too large. The limit is {maxNrReturnedBuckets} cells. Try using lower precision or a smaller bounding box.");
                }

                throw new InvalidOperationException(searchResponse.DebugInformation);
            }

            var nrOfGridCells = (int?)searchResponse.Aggregations.GeoHash("geohash_grid").Buckets?.Count ?? 0;
            if (nrOfGridCells > maxNrReturnedBuckets)
            {
                return Result.Failure<GeoGridResult>($"The number of cells that will be returned is too large. The limit is {maxNrReturnedBuckets} cells. Try using lower precision or a smaller bounding box.");
            }

            _telemetry.StopOperation(operation);

            var georesult = searchResponse
                .Aggregations
                .Terms("geohash_grid")
                .Buckets?
                .Select(b =>
                    new GridCell()
                    {
                        ObservationsCount = b.DocCount,
                        TaxaCount = (long?)b.Cardinality("taxa_count")?.Value,
                        BoundingBox = LatLonGeohashBoundingBox.CreateFromGeohash(b.Key).Value
                    });

            var gridResult = new GeoGridResult()
            {
                BoundingBox = bbox,
                Precision = precision,
                GridCellCount = nrOfGridCells,
                GridCells = georesult
            };

            // When operation is disposed, telemetry item is sent.
            return Result.Success(gridResult);
        }

        public async Task<Result<GeoGridTileResult>> GetGeogridTileAggregationAsync(
                SearchFilter filter,
                int zoom,
                LatLonBoundingBox bbox)
        {
            const int maxNrReturnedBuckets = 10000;
            var indexNames = GetIndexNames(filter);
            var (query, excludeQuery) = GetCoreQueries(filter);

            using var operation = _telemetry.StartOperation<DependencyTelemetry>("Observation_Search_GeoAggregated");
            operation.Telemetry.Properties["Filter"] = filter.ToString();

            var searchResponse = await _elasticClient.SearchAsync<dynamic>(s => s
                .Index(indexNames)
                .Size(0)
                .Aggregations(a => a.Filter("geotile_filter", g => g
                    .Filter(f => f.GeoBoundingBox(bb => bb
                        .Field("location.pointLocation")
                        .BoundingBox(b => b.TopLeft(bbox.TopLeft.ToGeoLocation()).BottomRight(bbox.BottomRight.ToGeoLocation()))
                   ))
                    .Aggregations(ab => ab.GeoTile("geotile_grid", gg => gg
                        .Field("location.pointLocation")
                        .Size(maxNrReturnedBuckets + 1)
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
                    return Result.Failure<GeoGridTileResult>($"The number of cells that will be returned is too large. The limit is {maxNrReturnedBuckets} cells. Try using lower zoom or a smaller bounding box.");
                }

                throw new InvalidOperationException(searchResponse.DebugInformation);
            }

            var nrOfGridCells = (int?)searchResponse.Aggregations?.Filter("geotile_filter")?.GeoTile("geotile_grid")?.Buckets?.Count ?? 0;
            if (nrOfGridCells > maxNrReturnedBuckets)
            {
                return Result.Failure<GeoGridTileResult>($"The number of cells that will be returned is too large. The limit is {maxNrReturnedBuckets} cells. Try using lower zoom or a smaller bounding box.");
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
                BoundingBox = bbox,
                Zoom = zoom,
                GridCellTileCount = nrOfGridCells,
                GridCellTiles = georesult
            };

            // When operation is disposed, telemetry item is sent.
            return Result.Success(gridResult);
        }
    }
}