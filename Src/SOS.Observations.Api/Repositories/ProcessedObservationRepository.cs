using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Logging;
using Nest;
using NGeoHash;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Gis;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search;
using SOS.Observations.Api.Extensions;
using SOS.Observations.Api.Models.AggregatedResult;
using SOS.Observations.Api.Repositories.Interfaces;
using Result = CSharpFunctionalExtensions.Result;

namespace SOS.Observations.Api.Repositories
{
    /// <summary>
    ///     Species data service
    /// </summary>
    public class ProcessedObservationRepository : ProcessBaseRepository<ProcessedObservation, string>,
        IProcessedObservationRepository
    {
        private readonly IElasticClient _elasticClient;
        private readonly TelemetryClient _telemetry;
        private readonly string _indexName;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="elasticClient"></param>
        /// <param name="client"></param>
        /// <param name="elasticConfiguration"></param>
        /// <param name="telemetry"></param>
        /// <param name="logger"></param>
        public ProcessedObservationRepository(
            IElasticClient elasticClient,
            IProcessClient client,
            ElasticSearchConfiguration elasticConfiguration,
            TelemetryClient telemetry,
            ILogger<ProcessedObservationRepository> logger) : base(client, true, logger)
        {
            _elasticClient = elasticClient ?? throw new ArgumentNullException(nameof(elasticClient));

            _indexName = string.IsNullOrEmpty(elasticConfiguration.IndexPrefix)
                ? $"{CollectionName.ToLower()}"
                : $"{elasticConfiguration.IndexPrefix.ToLower()}-{CollectionName.ToLower()}";

            _telemetry = telemetry ?? throw new ArgumentNullException(nameof(telemetry)); ;
        }

        /// <inheritdoc />
        public async Task<PagedResult<dynamic>> GetChunkAsync(SearchFilter filter, int skip, int take, string sortBy,
            SearchSortOrder sortOrder)
        {
            if (!filter?.IsFilterActive ?? true)
            {
                return null;
            }

            var query = filter.ToQuery();
            query = AddSightingTypeFilters(filter, query);
            query = InternalFilterBuilder.AddFilters(filter, query);

            var excludeQuery = CreateExcludeQuery(filter);
            excludeQuery = InternalFilterBuilder.AddExcludeFilters(filter, excludeQuery);

            var sortDescriptor = sortBy.ToSortDescriptor<ProcessedObservation>(sortOrder);

            using var operation = _telemetry.StartOperation<DependencyTelemetry>("Observation_Search");

            operation.Telemetry.Properties["Filter"] = filter.ToString();

            var searchResponse = await _elasticClient.SearchAsync<dynamic>(s => s
                .Index(_indexName)
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

            if (filter is SearchFilterInternal)
            {
                var internalFilter = filter as SearchFilterInternal;
                if (internalFilter.IncludeRealCount)
                {
                    var countResponse = await _elasticClient.CountAsync<dynamic>(s => s
                        .Index(_indexName)
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
            if (!filter?.IsFilterActive ?? true)
            {
                return null;
            }

            var query = filter.ToQuery();
            query = AddSightingTypeFilters(filter, query);
            query = InternalFilterBuilder.AddFilters(filter, query);

            var excludeQuery = CreateExcludeQuery(filter);
            excludeQuery = InternalFilterBuilder.AddExcludeFilters(filter, excludeQuery);

            query = InternalFilterBuilder.AddAggregationFilter(aggregationType, query);
            
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
                .Index(_indexName)
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
        public async Task<PagedResult<dynamic>> GetAggregatedChunkAsync(SearchFilter filter, AggregationType aggregationType, int skip, int take, string sortBy, SearchSortOrder sortOrder)
        {
            if (!filter?.IsFilterActive ?? true)
            {
                return null;
            }

            var query = filter.ToQuery();
            query = AddSightingTypeFilters(filter, query);
            query = InternalFilterBuilder.AddFilters(filter, query);

            var excludeQuery = CreateExcludeQuery(filter);
            excludeQuery = InternalFilterBuilder.AddExcludeFilters(filter, excludeQuery);

            query = InternalFilterBuilder.AddAggregationFilter(aggregationType, query);

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
                .Index(_indexName)
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
                .Index(_indexName)
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

        private static IEnumerable<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> AddSightingTypeFilters(SearchFilter filter, IEnumerable<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> query)
        {
            var queryList = query.ToList();

            if (filter is SearchFilterInternal)
            {
                var internalFilter = filter as SearchFilterInternal;
                int[] sightingTypeSearchGroupFilter = null;
                if (internalFilter.TypeFilter == SearchFilterInternal.SightingTypeFilter.DoNotShowMerged)
                {
                    sightingTypeSearchGroupFilter = new int[] { 0, 1, 4, 16, 32, 128 };
                }
                else if (internalFilter.TypeFilter == SearchFilterInternal.SightingTypeFilter.ShowBoth)
                {
                    sightingTypeSearchGroupFilter = new int[] { 0, 1, 2, 4, 16, 32, 128 };
                }
                else if (internalFilter.TypeFilter == SearchFilterInternal.SightingTypeFilter.ShowOnlyMerged)
                {
                    sightingTypeSearchGroupFilter = new int[] { 0, 2 };
                }
                else if (internalFilter.TypeFilter == SearchFilterInternal.SightingTypeFilter.DoNotShowSightingsInMerged)
                {
                    sightingTypeSearchGroupFilter = new int[] { 0, 1, 2, 4, 32, 128 };
                }

                queryList.Add(q => q
                    .Terms(t => t
                        .Field("artportalenInternal.sightingTypeSearchGroupId")
                        .Terms(sightingTypeSearchGroupFilter)
                    )
                );
            }
            else
            {
                queryList.Add(q => q
                    .Terms(t => t
                        .Field("artportalenInternal.sightingTypeId")
                        .Terms(new int[] { 0, 3 })
                    )
                );
                queryList.Add(q => q
                    .Terms(t => t
                        .Field("artportalenInternal.sightingTypeSearchGroupId")
                        .Terms(new int[] { 0, 1, 32 })
                    )
                );

            }

            query = queryList;
            return query;
        }

        private static List<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> CreateExcludeQuery(
            FilterBase filter)
        {
            var queryContainers = new List<Func<QueryContainerDescriptor<dynamic>, QueryContainer>>();

            if (filter.GeometryFilter?.IsValid ?? false)
            {
                foreach (var geom in filter.GeometryFilter.Geometries)
                {
                    switch (geom.Type.ToLower())
                    {
                        case "holepolygon":
                            if (filter.GeometryFilter.UsePointAccuracy)
                            {
                                queryContainers.Add(q => q
                                    .GeoShape(gd => gd
                                        .Field("location.pointWithBuffer")
                                        .Shape(s => geom)
                                        .Relation(GeoShapeRelation.Intersects)
                                    )
                                );
                            }
                            else
                            {
                                queryContainers.Add(q => q
                                    .GeoShape(gd => gd
                                        .Field("location.point")
                                        .Shape(s => geom)
                                        .Relation(GeoShapeRelation.Within)
                                    )
                                );
                            }

                            break;
                    }
                }
            }

            return queryContainers;
        }

        public async Task<Result<GeoGridResult>> GetGeogridAggregationAsync(
                SearchFilter filter,
                int precision,
                LatLonBoundingBox bbox)
        {
            const int maxNrBucketsInElactic = 65535;
            const int maxNrReturnedBuckets = 10000;
            if (!filter?.IsFilterActive ?? true)
            {
                return null;
            }

            var query = filter.ToQuery();
            query = AddSightingTypeFilters(filter, query);
            query = InternalFilterBuilder.AddFilters(filter, query);
            var excludeQuery = CreateExcludeQuery(filter);
            excludeQuery = InternalFilterBuilder.AddExcludeFilters(filter, excludeQuery);

            using var operation = _telemetry.StartOperation<DependencyTelemetry>("Observation_Search_GeoAggregated");
            operation.Telemetry.Properties["Filter"] = filter.ToString();

            var searchResponseZoomedInGrid = await _elasticClient.SearchAsync<dynamic>(s => s
                .Index(_indexName)
                .Size(0)
                .Aggregations(a => a.GeoHash("geohash_grid", g => g
                    .Field("location.pointLocation")
                    .Size(maxNrBucketsInElactic + 1)
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
            if (!searchResponseZoomedInGrid.IsValid)
            {
                if (searchResponseZoomedInGrid.ServerError.Error.CausedBy.Type == "too_many_buckets_exception")
                {
                    return Result.Failure<GeoGridResult>($"The number of cells that will be returned is too large. The limit is {maxNrReturnedBuckets} cells. Try using lower precision or a smaller bounding box.");
                }

                throw new InvalidOperationException(searchResponseZoomedInGrid.DebugInformation);
            }

            var nrOfGridCells = (int?)searchResponseZoomedInGrid.Aggregations.GeoHash("geohash_grid").Buckets?.Count ?? 0;
            if (nrOfGridCells >= maxNrReturnedBuckets)
            {
                return Result.Failure<GeoGridResult>($"The number of cells that will be returned is too large. The limit is {maxNrReturnedBuckets} cells. Try using lower precision or a smaller bounding box.");
            }

            _telemetry.StopOperation(operation);

            var georesult = searchResponseZoomedInGrid
                .Aggregations
                .Terms("geohash_grid")
                .Buckets?
                .Select(b =>
                    new GridCell()
                    {
                        ObservationsCount = b.DocCount,
                        TaxaCount = (long?)b.Cardinality("taxa_count")?.Value,
                        BoundingBox = GetBoundingBoxFromGeoHash(b.Key)
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

        private LatLonBoundingBox GetBoundingBoxFromGeoHash(string geoHash)
        {
            var geoHashBbox = GeoHash.DecodeBbox(geoHash);
            var bbox = new LatLonBoundingBox()
            {
                GeoHash = geoHash,
                TopLeft = new LatLonCoordinate(geoHashBbox.Maximum.Lat, geoHashBbox.Minimum.Lon),
                BottomRight = new LatLonCoordinate(geoHashBbox.Minimum.Lat, geoHashBbox.Maximum.Lon)
            };
            return bbox;
        }
    }
}