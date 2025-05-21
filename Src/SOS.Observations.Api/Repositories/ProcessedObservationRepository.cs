using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Aggregations;
using Elastic.Clients.Elasticsearch.Cluster;
using Elastic.Clients.Elasticsearch.Mapping;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using SOS.Lib;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Gis;
using SOS.Lib.Models.Processed.AggregatedResult;
using SOS.Lib.Models.Processed.Configuration;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Models.Search.Result;
using SOS.Lib.Repositories.Processed;
using SOS.Observations.Api.Repositories.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SOS.Observations.Api.Repositories
{
    public class ProcessedObservationRepository : ProcessedObservationCoreRepository, IProcessedObservationRepository
    {
        public ProcessedObservationRepository(
            IElasticClientManager elasticClientManager,
            ICache<string, ProcessedConfiguration> processedConfigurationCache,
            ElasticSearchConfiguration elasticConfiguration,
            ITaxonManager taxonManager,
            IClassCache<ConcurrentDictionary<string, HealthResponse>> clusterHealthCache,
            IMemoryCache memoryCache,
            ILogger<ProcessedObservationRepository> logger) : base(elasticClientManager, elasticConfiguration, processedConfigurationCache, taxonManager, clusterHealthCache, memoryCache, logger)
        {
        }

        /// <inheritdoc />
        public async Task<PagedResult<dynamic>> GetAggregatedChunkAsync(SearchFilter filter, AggregationType aggregationType, int skip, int take)
        {
            var indexNames = GetCurrentIndex(filter);
            var (queries, excludeQueries) = GetCoreQueries<dynamic>(filter);
            queries.AddAggregationFilter(aggregationType);
            
            // Get number of distinct values
            var searchResponseCount = await Client.SearchAsync<dynamic>(s => s
                .Indices(indexNames)
                .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQueries.ToArray())
                        .Filter(queries.ToArray())
                    )
                )
                .Aggregations(a => a
                    .Add("species_count", a => a
                        .Cardinality(c => c
                            .Field("taxon.id")
                        )
                    )
                )
                .AddDefaultAggrigationSettings()
            );

            searchResponseCount.ThrowIfInvalid();

            var maxResult = (int?)searchResponseCount.Aggregations.GetCardinality("species_count").Value ?? 0;
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

            // Calculate size to fetch. If zero, get all
            var size = skip + take < maxResult ? skip + take : maxResult == 0 ? 1 : maxResult;
            size = size * 2; // There can be duplicates due to sort order differences between full harvest and incremental harvest observations
            if (skip == 0 && take == -1)
            {
                size = maxResult == 0 ? 1 : maxResult;
                take = maxResult;
            }

            // Get the real result
            var searchResponse = await Client.SearchAsync<dynamic>(indexNames, s => s
                .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQueries.ToArray())
                        .Filter(queries.ToArray())
                    )
                )
                .Aggregations(a => a
                    .Add("species", a => a
                        .Composite(c => c
                            .Size(size)
                            .Sources(
                                [
                                    CreateCompositeTermsAggregationSource(
                                        ("sortOrder", "taxon.attributes.sortOrder", SortOrder.Asc),
                                        ("id", "taxon.id", SortOrder.Asc)
                                    )
                                ]
                            )
                        )
                    )
                    
                )
                .AddDefaultAggrigationSettings()
            );

            searchResponse.ThrowIfInvalid();
            var buckets = searchResponse
                .Aggregations
                .GetComposite("species")
                    .Buckets?
                    .Select(b =>
                            new AggregatedSpecies
                            {
                                DocCount = b.DocCount,
                                TaxonId = int.Parse(b.Key["id"].ToString())
                            }
                        ).ToList();

            var dic = new Dictionary<int, AggregatedSpecies>();
            for (int i = 0; i < buckets.Count; i++)
            {
                if (!dic.ContainsKey(buckets[i].TaxonId))
                {
                    dic.Add(buckets[i].TaxonId, buckets[i]);
                }
                else
                {
                    dic[buckets[i].TaxonId].DocCount += buckets[i].DocCount;
                }
            }

            var result = dic.Values
                .Skip(skip)
                .Take(take)
                .ToList();

            var taxonTree = await _taxonManager.GetTaxonTreeAsync();
            foreach (var item in result)
            {
                var treeNode = taxonTree.GetTreeNode(item.TaxonId);
                if (treeNode != null)
                {
                    var sightingScientificName = treeNode.Data.Attributes.ScientificNames?.FirstOrDefault(vn => vn.ValidForSighting);
                    var sightingVernacularName = treeNode.Data.Attributes.VernacularNames?.FirstOrDefault(vn => vn.ValidForSighting);
                    item.RedlistCategory = treeNode.Data.Attributes.RedlistCategory ?? "";
                    item.ScientificName = (string.IsNullOrEmpty(sightingScientificName?.Name) ? treeNode.Data.ScientificName : sightingScientificName.Name) ?? "";
                    item.VernacularName = (string.IsNullOrEmpty(sightingVernacularName?.Name) ? treeNode.Data.VernacularName : sightingVernacularName.Name) ?? "";
                    item.ScientificNameAuthorship = (string.IsNullOrEmpty(sightingScientificName?.Name) ? treeNode.Data.ScientificNameAuthorship : sightingScientificName.Author) ?? "";
                }
            }

            return new PagedResult<dynamic>
            {
                Records = result,
                Skip = skip,
                Take = take,
                TotalCount = maxResult
            };
        }

        private class TaxInfo
        {
            public int TaxonId { get; set; }
            public int Index { get; set; }
            public int DocCount { get; set; }
        }

        /// <inheritdoc />
        public async Task<PagedResult<dynamic>> GetAggregatedHistogramChunkAsync(SearchFilter filter, AggregationType aggregationType)
        {
            var indexNames = GetCurrentIndex(filter);
            var (queries, excludeQueries) = GetCoreQueries<dynamic>(filter);
            queries.AddAggregationFilter(aggregationType);
            
            var tz = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);
            var searchResponse = await Client.SearchAsync<dynamic>(s => s
                .Indices(indexNames)
                .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQueries.ToArray())
                        .Filter(queries.ToArray())
                    )
                )
                .Aggregations(a => a
                    .Add("aggregation", a => a
                        .DateHistogram(dh => dh
                            .Field("event.startDate")
                            .CalendarInterval(aggregationType switch
                            {
                                AggregationType.QuantityPerYear => CalendarInterval.Year,
                                AggregationType.SightingsPerYear => CalendarInterval.Year,
                                AggregationType.QuantityPerWeek => CalendarInterval.Week,
                                AggregationType.SightingsPerWeek => CalendarInterval.Week,
                                _ => CalendarInterval.Day
                            }
                        )
                        .TimeZone($"{(tz.TotalMinutes > 0 ? "+" : "")}{tz.Hours:00}:{tz.Minutes:00}")
                        .Format("yyyy-MM-dd")
                        
                        )
                        .Aggregations(a => a
                            .Add("quantity", a => a
                                .Sum(sum => sum
                                    .Field("occurrence.organismQuantityAggregation")
                                )
                            )   
                        )
                    )     
                )
                .AddDefaultAggrigationSettings(trackHits: true)
            );

            searchResponse.ThrowIfInvalid();

            var totalCount = searchResponse.Total;

            var result = searchResponse
                .Aggregations
                .GetDateHistogram("aggregation")
                .Buckets?
                .Select(b =>
                    new
                    {
                        Date = DateTime.Parse(b.KeyAsString),
                        b.DocCount,
                        Quantity = b.Aggregations.GetSum("quantity").Value
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
        public async Task<PagedResult<dynamic>> GetAggregated48WeekHistogramAsync(SearchFilter filter)
        {
            var indexNames = GetCurrentIndex(filter);
            var (queries, excludeQueries) = GetCoreQueries<dynamic>(filter);
            queries.AddAggregationFilter(AggregationType.SightingsPerWeek48);

            var tz = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);
            var searchResponse = await Client.SearchAsync<dynamic>(s => s
                .Indices(indexNames)
                .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQueries.ToArray())
                        .Filter(queries.ToArray())
                    )
                )
                .RuntimeMappings(rtm => rtm
                    .Add("yearWeek", a => a
                            .Script(s => s
                                .Source("emit(doc['event.startYear'].value + '-' + doc['event.startHistogramWeek'].value);")                           
                            )
                           .Type(RuntimeFieldType.Keyword)
                    )
                )
                .Aggregations(a => a
                    .Add("yearWeekAggregation", a => a
                        .Terms(t => t
                            .Field("yearWeek")
                            .Size(1000)
                            .Order(new[] { new KeyValuePair<Field, SortOrder>(Field.KeyField, SortOrder.Asc) })
                        )
                        .Aggregations(a => a
                            .Add("quantity", a => a
                                .Sum(sum => sum
                                    .Field("occurrence.organismQuantityAggregation")
                                )
                            )
                        )
                    )
                )
                .AddDefaultAggrigationSettings()
            );

            searchResponse.ThrowIfInvalid();

            var yearWeekAggregations = searchResponse
                .Aggregations
                .GetStringTerms("yearWeekAggregation")
                .Buckets?
                .Select(b =>
                    new
                    {
                        Year = int.Parse(b.Key.Value?.ToString().Substring(0, 4)),
                        Week = int.Parse(b.Key.Value?.ToString().Substring(5)),
                        DocCount = b.DocCount,
                        Quantity = b.Aggregations.GetSum("quantity").Value ?? 0.0
                    })
                    .OrderBy(ob => ob.Year).ThenBy(tb => tb.Week).ToDictionary(b => $"{b.Year}-{b.Week}", b => b);

            //Fill in gaps in returned result
            var yearWeeks = new List<dynamic>();
            if (yearWeekAggregations?.Any() ?? false)
            {
                var firstValue = yearWeekAggregations.First().Value;
                var currentYear = firstValue.Year;
                var currentWeek = firstValue.Week;
                var lastYear = firstValue.Year;
                var lastWeek = firstValue.Week;

                while (currentYear < lastYear || (currentYear == lastYear && currentWeek <= lastWeek))
                {
                    if (yearWeekAggregations.TryGetValue($"{currentYear}-{currentWeek}", out var yearWeekAggregation))
                    {
                        yearWeeks.Add(yearWeekAggregation);
                    }
                    else
                    {
                        yearWeeks.Add(new
                        {
                            Year = currentYear,
                            Week = currentWeek,
                            DocCount = (long?)0,
                            Quantity = (double?)0.0
                        });
                    }

                    if (currentWeek == 48)
                    {
                        currentYear++;
                        currentWeek = 0;
                    }
                    currentWeek++;
                }
            }

            return new PagedResult<dynamic>
            {
                Records = yearWeeks,
                Skip = 0,
                Take = yearWeeks.Count(),
                TotalCount = yearWeeks.Count()
            };

            // When operation is disposed, telemetry item is sent.
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TimeSeriesHistogramResult>> GetTimeSeriesHistogramAsync(SearchFilter filter, TimeSeriesType timeSeriesType)
        {
            var indexNames = GetCurrentIndex(filter);
            var (queries, excludeQueries) = GetCoreQueries<dynamic>(filter);
   
            var field = timeSeriesType switch
            {
                TimeSeriesType.Month => "event.startMonth",
                TimeSeriesType.Week48 => "event.startHistogramWeek",
                TimeSeriesType.Day => "event.startDay",
                _ => "event.startDay"
            };

            var extendedBoundsMax = timeSeriesType switch
            {
                TimeSeriesType.Month => 12,
                TimeSeriesType.Week48 => 48,
                TimeSeriesType.Day => 365,
                _ => 365
            };

            var searchResponse = await Client.SearchAsync<dynamic>(s => s
                .Indices(indexNames)
                .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQueries.ToArray())
                        .Filter(queries.ToArray())
                    )
                )
                .Aggregations(a => a
                    .Add("aggregation", a => a
                        .Histogram(h => h
                             .Field(field)
                             .Interval(1)
                            .ExtendedBounds(eb => eb
                                .Min(1)
                                .Max(extendedBoundsMax)
                            )
                        )
                        .Aggregations(a => a
                            .Add("quantity", a => a
                                .Sum(s => s
                                    .Field("occurrence.organismQuantityAggregation")
                                )
                            )
                            .Add("unique_taxonids", a => a
                                .Cardinality(c => c
                                     .Field("taxon.id")
                                )
                            )
                        )
                    )
                )
                .AddDefaultAggrigationSettings()
            );

            searchResponse.ThrowIfInvalid();

            var totalCount = searchResponse.Total;
            return searchResponse
                .Aggregations.GetHistogram("aggregation")
                    .Buckets?
                        .Select(b =>
                            new TimeSeriesHistogramResult
                            {
                                Type = timeSeriesType,
                                Period = (int)b.Key,
                                Observations = (int)b.DocCount,
                                Quantity = (int)b.Aggregations.GetSum("quantity").Value,
                                Taxa = (int)b.Aggregations.GetCardinality("unique_taxonids").Value
                        });

            // When operation is disposed, telemetry item is sent.
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TimeSeriesHistogramResult>> GetYearHistogramAsync(SearchFilter filter, TimeSeriesType timeSeriesType)
        {
            var indexNames = GetCurrentIndex(filter);
            var (queries, excludeQueries) = GetCoreQueries<dynamic>(filter);

            var tz = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);
            ExtendedBoundsDate extendedBounds = null;
            if (filter.Date != null)
            {        
                extendedBounds = new ExtendedBoundsDate
                {
                    Max = filter.Date.EndDate.HasValue
                        ? new FieldDateMath(filter.Date.EndDate.Value.ToString("yyyy-MM-ddTHH:mm:ss"))
                        : null,
                    Min = filter.Date.StartDate.HasValue
                        ? new FieldDateMath(filter.Date.StartDate.Value.ToString("yyyy-MM-ddTHH:mm:ss"))
                        : null
                };
            }

            var searchResponse = await Client.SearchAsync<dynamic>(s => s
                .Indices(indexNames)
                .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQueries.ToArray())
                        .Filter(queries.ToArray())
                    )
                )
                .Aggregations(a => a
                    .Add("aggregation", a => a
                        .DateHistogram(dh => dh
                           
                            .Field("event.startDate")
                            .CalendarInterval(CalendarInterval.Year)                            
                            .TimeZone($"{(tz.TotalMinutes > 0 ? "+" : "")}{tz.Hours:00}:{tz.Minutes:00}")
                            .Format("yyyy-MM-dd||date_optional_time")
                            .ExtendedBounds(extendedBounds)
                        )
                        .Aggregations(a => a
                            .Add("quantity", a => a
                                .Sum(s => s
                                    .Field("occurrence.organismQuantityAggregation")
                                )
                            )
                            .Add("unique_taxonids", a => a
                                .Cardinality(s => s
                                    .Field("taxon.id")
                                )
                            )
                        )
                    )
                )
                .AddDefaultAggrigationSettings()
            );

            searchResponse.ThrowIfInvalid();

            var totalCount = searchResponse.Total;
            return searchResponse
                .Aggregations
                    .GetDateHistogram("aggregation")
                .Buckets?
                .OrderByDescending(b => b.KeyAsString)
                .Select(b =>
                    new TimeSeriesHistogramResult
                    {
                        Type = timeSeriesType,
                        Period = DateTime.Parse(b.KeyAsString).Year,
                        Observations = (int)b.DocCount,
                        Quantity = (int)b.Aggregations.GetSum("quantity").Value,
                        Taxa = (int)b.Aggregations.GetCardinality("unique_taxonids").Value
                    });

            // When operation is disposed, telemetry item is sent.
        }

        /// <inheritdoc />
        public async Task<GeoGridTileResult> GetGeogridTileAggregationAsync(
                SearchFilter filter,
                int zoom)
        {
            var indexNames = GetCurrentIndex(filter);
            var (queries, excludeQueries) = GetCoreQueries<dynamic>(filter);

            var searchResponse = await Client.SearchAsync<dynamic>(s => s
                .Indices(indexNames)
                .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQueries.ToArray())
                        .Filter(queries.ToArray())
                    )
                )
                .Aggregations(a => a
                    .Add("geotile_filter", a => a
                        .Filter(g => g
                            .GeoBoundingBox(gbb => gbb
                               .Field("location.pointLocation")
                               .BoundingBox(filter.Location.Geometries.BoundingBox.ToGeoBounds())
                            )
                        )
                        .Aggregations(ab => ab
                            .Add("geotile_grid", a => a
                                .GeotileGrid(gtg => gtg
                                    .Field("location.pointLocation")
                                    .Size(MaxNrElasticSearchAggregationBuckets + 1)
                                    .Precision(zoom)
                                )
                                .Aggregations(a => a
                                    .Add("taxa_count", a => a
                                        .Cardinality(t => t
                                            .Field("taxon.id")
                                        )
                                    )
                                )
                            )
                        )
                    )
                    
                )
                .AddDefaultAggrigationSettings()
            );

            if (!searchResponse.IsValidResponse)
            {
                if (searchResponse.ElasticsearchServerError?.Error?.CausedBy?.Type == "too_many_buckets_exception")
                {
                    throw new ArgumentOutOfRangeException($"The number of cells that will be returned is too large. The limit is {MaxNrElasticSearchAggregationBuckets} cells. Try using lower zoom or a smaller bounding box.");
                }
                searchResponse.ThrowIfInvalid();
            }

            var nrOfGridCells = (int?)searchResponse.Aggregations?.GetFilter("geotile_filter")?.Aggregations.GetGeotileGrid("geotile_grid")?.Buckets?.Count ?? 0;
            if (nrOfGridCells > MaxNrElasticSearchAggregationBuckets)
            {
                throw new ArgumentOutOfRangeException($"The number of cells that will be returned is too large. The limit is {MaxNrElasticSearchAggregationBuckets} cells. Try using lower zoom or a smaller bounding box.");
            }

            var georesult = searchResponse
                .Aggregations
                .GetFilter("geotile_filter")
                .Aggregations.GetGeotileGrid("geotile_grid")
                .Buckets?
                .Select(b => GridCellTile.Create(b.Key, b.DocCount, b.Aggregations.GetCardinality("taxa_count").Value));

            var gridResult = new GeoGridTileResult()
            {
                BoundingBox = filter.Location.Geometries.BoundingBox,
                Zoom = zoom,
                GridCellTileCount = nrOfGridCells,
                GridCellTiles = georesult
            };

            // When operation is disposed, telemetry item is sent.
            return gridResult;
        }

        /// <inheritdoc />
        public async Task<int> GetProvinceCountAsync(SearchFilterBase filter)
        {
            var indexNames = GetCurrentIndex(filter);
            var (queries, excludeQueries) = GetCoreQueries<dynamic>(filter);

            var searchResponse = await Client.SearchAsync<dynamic>(s => s
                .Indices(indexNames)
                .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQueries.ToArray())
                        .Filter(queries.ToArray())
                    )
                )
                .Aggregations(a => a
                    .Add("provinceCount", a => a
                        .Cardinality(c => c
                            .Field("location.province.featureId")
                        )
                    )
                ) 
                .AddDefaultAggrigationSettings()
            );

            searchResponse.ThrowIfInvalid();
            int provinceCount = Convert.ToInt32(searchResponse.Aggregations.GetCardinality("provinceCount").Value);
            return provinceCount;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<YearCountResult>> GetUserYearCountAsync(SearchFilter filter)
        {
            try
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
                        .Add("observationByYear", a => a
                            .Composite(c => c
                                .Size(100) // 100 years
                                .Sources(
                                    [
                                        CreateCompositeTermsAggregationSource(("startYear", "event.startYear", SortOrder.Desc)),
                                    ]
                                )
                            )
                            .Aggregations(a => a
                                .Add("unique_taxonids", a => a
                                    .Cardinality(c => c
                                        .Field("taxon.id")
                                    )
                                
                                )
                            )
                        )
                    )
                    .AddDefaultAggrigationSettings()
                );

                searchResponse.ThrowIfInvalid();

                var result = new HashSet<YearCountResult>();
                foreach (var bucket in searchResponse.Aggregations.GetComposite("observationByYear").Buckets)
                {
                    var key = bucket.Key;

                    key.TryGetValue("startYear", out var startYearField);
                    startYearField.TryGetLong(out var startYear);
                    var count = bucket.DocCount;
                    var taxonCount = bucket.Aggregations.GetCardinality("unique_taxonids").Value;

                    result.Add(new YearCountResult
                    {
                        Count = count,
                        TaxonCount = taxonCount,
                        Year = (int)startYear
                    });
                }

                return result;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to get user year count");
                return null!;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<YearMonthCountResult>> GetUserYearMonthCountAsync(SearchFilter filter)
        {
            try
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
                        .Add("observationByYearMonth", a => a
                            .Composite(c => c
                                .Size(1200) // 12 months * 100 year
                                .Sources(
                                    [
                                        CreateCompositeTermsAggregationSource(
                                            ("startYear", "event.startYear", SortOrder.Desc)
                                        ),
                                        CreateCompositeTermsAggregationSource(
                                            ("startMonth", "event.startMonth", SortOrder.Desc)
                                        )
                                    ]
                                )
                            )
                            .Aggregations(a => a
                                .Add("unique_taxonids", a => a
                                    .Cardinality(c => c
                                        .Field("taxon.id")
                                    )
                                ) 
                            )
                        )
                    )
                    .AddDefaultAggrigationSettings()
                );

                searchResponse.ThrowIfInvalid();

                var result = new HashSet<YearMonthCountResult>();
                foreach (var bucket in searchResponse.Aggregations.GetComposite("observationByYearMonth").Buckets)
                {
                    var key = bucket.Key;

                    key.TryGetValue("startYear", out var startYearField);
                    startYearField.TryGetLong(out var startYear);
                    key.TryGetValue("startMonth", out var startMonthField);
                    startMonthField.TryGetLong(out var startMonth);
                    var count = bucket.DocCount;
                    var taxonCount = (long)bucket.Aggregations.GetCardinality("unique_taxonids").Value;

                    result.Add(new YearMonthCountResult
                    {
                        Count = count,
                        Month = (int)startMonth,
                        TaxonCount = taxonCount,
                        Year = (int)startYear
                    });
                }

                return result;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to get user year month count");
                return null!;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<YearMonthDayCountResult>> GetUserYearMonthDayCountAsync(SearchFilter filter, int skip, int take)
        {
            try
            {
                var (queries, excludeQueries) = GetCoreQueries<dynamic>(filter);

                // First get observations count and taxon count group by day
                var searchResponse = await Client.SearchAsync<dynamic>(s => s
                   .Index(new[] { PublicIndexName, ProtectedIndexName })
                   .Query(q => q
                        .Bool(b => b
                            .MustNot(excludeQueries.ToArray())
                            .Filter(queries.ToArray())
                        )
                    )
                    .Aggregations(a => a
                        .Add("observationByYearMonth", a => a
                            .Composite(c => c
                                .Size(skip + take) // Take as few as possible
                                .Sources(
                                    [
                                         CreateCompositeTermsAggregationSource(
                                            ("startYear", "event.startYear", SortOrder.Desc)
                                        ),
                                        CreateCompositeTermsAggregationSource(
                                            ("startMonth", "event.startMonth", SortOrder.Desc)
                                        ),
                                        CreateCompositeTermsAggregationSource(
                                            ("startDay", "event.startDay", SortOrder.Desc)
                                        )
                                    ]
                                )
                            )
                            .Aggregations(a => a
                                .Add("unique_taxonids", a => a
                                    .Cardinality(c => c
                                        .Field("taxon.id")
                                    )
                                )   
                            )
                        )
                    )
                    .AddDefaultAggrigationSettings()
                );

                searchResponse.ThrowIfInvalid();

                var result = new Dictionary<string, YearMonthDayCountResult>();
                foreach (var bucket in searchResponse.Aggregations.GetComposite("observationByYearMonth").Buckets.Skip(skip))
                {
                    var key = bucket.Key;

                    key.TryGetValue("startYear", out var startYearField);
                    startYearField.TryGetLong(out var startYear);
                    key.TryGetValue("startMonth", out var startMonthField);
                    startMonthField.TryGetLong(out var startMonth);
                    key.TryGetValue("startDay", out var startDayField);
                    startDayField.TryGetLong(out var startDay);

                    var count = bucket.DocCount;
                    var taxonCount = (long)bucket.Aggregations.GetCardinality("unique_taxonids").Value;

                    result.Add($"{startYear}-{startMonth}-{startDay}", new YearMonthDayCountResult
                    {
                        Count = count,
                        Day = (int)startDay,
                        Localities = new HashSet<IdName<string>>(),
                        Month = (int)startMonth,
                        TaxonCount = taxonCount,
                        Year = (int)startYear
                    });
                }

                if (result.Any())
                {
                    var firstItem = result.First().Value;
                    var maxDate = new DateTime(firstItem.Year, firstItem.Month, firstItem.Day);
                    var lastItem = result.Last().Value;
                    var minDate = new DateTime(lastItem.Year, lastItem.Month, lastItem.Day);

                    // Limit search to only include time span we are interested in
                    filter.Date = filter.Date ?? new DateFilter();
                    filter.Date.StartDate = minDate;
                    filter.Date.EndDate = maxDate;
                    filter.Date.DateFilterType = DateFilter.DateRangeFilterType.BetweenStartDateAndEndDate;

                    // Second, get all locations group by day
                    var searchResponseLocality = await Client.SearchAsync<dynamic>(s => s
                       .Index(new[] { PublicIndexName, ProtectedIndexName })
                       .Query(q => q
                            .Bool(b => b
                                .MustNot(excludeQueries.ToArray())
                                .Filter(queries.ToArray())
                            )
                        )
                        .Aggregations(a => a
                            .Add("localityByYearMonth", a => a
                                .Composite(c => c
                                    .Size((skip + take) * 10) // 10 locations for one day must be enought
                                    .Sources(
                                        [
                                            CreateCompositeTermsAggregationSource(
                                                ("startYear", "event.startYear", SortOrder.Desc)
                                            ),
                                            CreateCompositeTermsAggregationSource(
                                                ("startMonth", "event.startMonth", SortOrder.Desc)
                                            ),
                                            CreateCompositeTermsAggregationSource(
                                                ("startDay", "event.startDay", SortOrder.Desc)
                                            ),
                                            CreateCompositeTermsAggregationSource(
                                                ("locationId", "location.locationId", SortOrder.Desc)
                                            ),
                                            CreateCompositeTermsAggregationSource(
                                                ("locality", "location.locality.raw", SortOrder.Desc)
                                            )
                                        ]
                                    )
                                )
                            )
                        )
                        .AddDefaultAggrigationSettings()
                    );

                    searchResponse.ThrowIfInvalid();

                    // Add locations to result
                    foreach (var bucket in searchResponseLocality.Aggregations.GetComposite("localityByYearMonth").Buckets)
                    {
                        var key = bucket.Key;

                        key.TryGetValue("startYear", out var startYearField);
                        startYearField.TryGetLong(out var startYear);
                        key.TryGetValue("startMonth", out var startMonthField);
                        startMonthField.TryGetLong(out var startMonth);
                        key.TryGetValue("startDay", out var startDayField);
                        startDayField.TryGetLong(out var startDay);
                        key.TryGetValue("locationId", out var locationIdField);
                        locationIdField.TryGetString(out var locationId);
                        key.TryGetValue("locality", out var localityField);
                        localityField.TryGetString(out var locality);
                        var itemKey = $"{startYear}-{startMonth}-{startDay}";

                        if (result.TryGetValue(itemKey, out var item))
                        {
                            item.Localities.Add(new IdName<string> { Id = locationId, Name = locality });
                        }
                    }
                }

                return result.Values;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to get user year month day count");
                return null!;
            }
        }

        /// <inheritdoc />
        public async Task<bool> HasIndexOccurrenceIdDuplicatesAsync(bool protectedIndex)
        {
            var searchResponse = await Client.SearchAsync<Observation>(s => s
                .Index(protectedIndex ? ProtectedIndexName : PublicIndexName)
                .Aggregations(a => a
                    .Add("uniqueOccurrenceIdCount", a => a
                        .Terms(t => t
                            .Field(f => f.Occurrence.OccurrenceId)
                            .MinDocCount(2)
                            .Size(1)
                        )
                    )
                )
                .AddDefaultAggrigationSettings()
            );
            searchResponse.ThrowIfInvalid();

            return searchResponse.Aggregations
                .GetStringTerms("uniqueOccurrenceIdCount")
                .Buckets.Count > 0;
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
            filter.ExtendedAuthorization.ProtectionFilter = ProtectionFilter.Sensitive;

            var (queries, excludeQueries) = GetCoreQueries<dynamic>(filter, true);
            queries.AddSignalSearchCriteria<dynamic>(extendedAuthorizations, onlyAboveMyClearance);
        
            var searchResponse = await Client.CountAsync<dynamic>(ProtectedIndexName, s => s
                .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQueries.ToArray())
                        .Filter(queries.ToArray())
                    )
                )
            );
            searchResponse.ThrowIfInvalid();

            return searchResponse.Count > 0;
        }
    }
}
