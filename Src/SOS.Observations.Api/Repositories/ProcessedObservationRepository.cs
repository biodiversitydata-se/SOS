using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Logging;
using Nest;
using SOS.Lib;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Gis;
using SOS.Lib.Models.Processed.AggregatedResult;
using SOS.Lib.Models.Processed.Configuration;
using SOS.Lib.Repositories.Processed;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Models.Search.Result;
using SOS.Observations.Api.Repositories.Interfaces;

namespace SOS.Observations.Api.Repositories
{
    public class ProcessedObservationRepository : ProcessedObservationCoreRepository, IProcessedObservationRepository
    {
        private const int ElasticSearchMaxRecords = 10000;

        public ProcessedObservationRepository(
            IElasticClientManager elasticClientManager,
            ICache<string, ProcessedConfiguration> processedConfigurationCache,
            TelemetryClient telemetry,
            ElasticSearchConfiguration elasticConfiguration,
            ILogger<ProcessedObservationRepository> logger) : base(elasticClientManager, elasticConfiguration, processedConfigurationCache, telemetry, logger)
        {
        }

        /// <inheritdoc />
        public async Task<PagedResult<dynamic>> GetAggregatedChunkAsync(SearchFilter filter, AggregationType aggregationType, int skip, int take)
        {
            var indexNames = GetCurrentIndex(filter);
            var (query, excludeQuery) = GetCoreQueries(filter);
            query.AddAggregationFilter(aggregationType);

            using var operation = _telemetry.StartOperation<DependencyTelemetry>("Observation_Search_Aggregated");
            operation.Telemetry.Properties["Filter"] = filter.ToString();

            // Get number of distinct values
            var searchResponseCount = await Client.SearchAsync<dynamic>(s => s
                .Index(indexNames)
                .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQuery)
                        .Filter(query)
                    )
                )
                .Aggregations(a => a
                    .Cardinality("species_count", c => c
                        .Field("taxon.scientificName")
                    )
                )
                .Size(0)
                .Source(filter.Output?.Fields.ToProjection(filter is SearchFilterInternal))
                .TrackTotalHits(false)
            );

            searchResponseCount.ThrowIfInvalid();

            var maxResult = (int?)searchResponseCount.Aggregations.Cardinality("species_count").Value ?? 0;
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
            if (skip == 0 && take == -1)
            {
                size = maxResult == 0 ? 1 : maxResult;
                take = maxResult;
            }

            // Get the real result
            var searchResponse = await Client.SearchAsync<dynamic>(s => s
                .Index(indexNames)
                .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQuery)
                        .Filter(query)
                    )
                )
                .Aggregations(a => a
                    .Composite("species", c => c
                        .Size(size)
                        .Sources(s => s
                            .Terms("sortOrder", t => t
                                .Field("taxon.attributes.sortOrder")
                             )
                            .Terms("scientificName", t => t
                                .Field("taxon.scientificName")
                             )
                            .Terms("id", t => t
                                .Field("taxon.id")
                            )
                            .Terms("vernacularName", t => t
                                .Field("taxon.vernacularName")
                                .MissingBucket(true)
                            )
                            .Terms("scientificNameAuthorship", t => t
                                .Field("taxon.scientificNameAuthorship")
                                .MissingBucket(true)
                            )
                            .Terms("redlistCategory", t => t
                                .Field("taxon.attributes.redlistCategory")
                                .MissingBucket(true)
                            )
                        )
                )
            )
                .Size(0)
                .Source(s => s.ExcludeAll())
                .TrackTotalHits(false)
            );

            searchResponse.ThrowIfInvalid();

            _telemetry.StopOperation(operation);

            var result = searchResponse
                .Aggregations
                .Composite("species")
                .Buckets?
                .Select(b =>
                        new AggregatedSpecies
                        {
                            DocCount = b.DocCount,
                            RedlistCategory = b.Key["redlistCategory"]?.ToString() ?? "",
                            ScientificNameAuthorship = b.Key["scientificNameAuthorship"]?.ToString() ?? "",
                            ScientificName = b.Key["scientificName"]?.ToString() ?? "",
                            TaxonId = int.Parse(b.Key["id"].ToString()),
                            VernacularName = b.Key["vernacularName"]?.ToString() ?? ""
                        }
                    )?
                .Skip(skip)
                .Take(take);

            return new PagedResult<dynamic>
            {
                Records = result,
                Skip = skip,
                Take = take,
                TotalCount = maxResult
            };
        }

        /// <inheritdoc />
        public async Task<PagedResult<dynamic>> GetAggregatedHistogramChunkAsync(SearchFilter filter, AggregationType aggregationType)
        {
            var indexNames = GetCurrentIndex(filter);
            var (query, excludeQuery) = GetCoreQueries(filter);
            query.AddAggregationFilter(aggregationType);

            using var operation = _telemetry.StartOperation<DependencyTelemetry>("Observation_Search_Aggregated_Histogram");

            operation.Telemetry.Properties["Filter"] = filter.ToString();
            var tz = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);
            var searchResponse = await Client.SearchAsync<dynamic>(s => s
                .Index(indexNames)
                .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQuery)
                        .Filter(query)
                    )
                )
                .Aggregations(a => a
                    
                    .DateHistogram("aggregation", dh => dh
                        .Field("event.startDate")
                        .CalendarInterval(aggregationType switch
                        {
                            AggregationType.QuantityPerYear => DateInterval.Year,
                            AggregationType.SightingsPerYear => DateInterval.Year,
                            AggregationType.QuantityPerWeek => DateInterval.Week,
                            AggregationType.SightingsPerWeek => DateInterval.Week,
                            _ => DateInterval.Day
                        }
                        )
                        .TimeZone($"{(tz.TotalMinutes > 0 ? "+" : "")}{tz.Hours:00}:{tz.Minutes:00}")
                        .Format("yyyy-MM-dd")
                        .Aggregations(a => a
                            .Sum("quantity", sum => sum
                                .Field("occurrence.organismQuantityInt")
                            )
                        )
                    )
                )
            .Size(0)
                .Source(s => s.ExcludeAll())
            );

            searchResponse.ThrowIfInvalid();
           
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
        public async Task<PagedResult<dynamic>> GetAggregated48WeekHistogramAsync(SearchFilter filter)
        {
            var indexNames = GetCurrentIndex(filter);
            var (query, excludeQuery) = GetCoreQueries(filter);
            query.AddAggregationFilter(AggregationType.SightingsPerWeek48);

            using var operation = _telemetry.StartOperation<DependencyTelemetry>("Observation_Search_Aggregated_HistogramWeek48");

            operation.Telemetry.Properties["Filter"] = filter.ToString();
            var tz = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);
            var searchResponse = await Client.SearchAsync<dynamic>(s => s
                .Index(indexNames)
                .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQuery)
                        .Filter(query)
                    )
                )
                .RuntimeFields(rf => rf
                    .RuntimeField("yearWeek", FieldType.Keyword, s => s
                        .Script("emit(doc['event.startYear'].value + '-' + doc['event.startHistogramWeek'].value);")
                    )
                )
                .Aggregations(a => a
                    .Terms("yearWeekAggregation", t => t
                        .Field("yearWeek")
                        .Size(1000)
                        .Order(o => o
                            .Ascending("_key")
                        )
                        .Aggregations(a => a
                            .Sum("quantity", sum => sum
                                .Field("occurrence.organismQuantityInt")
                            )
                        )
                    )
                )
                .Size(0)
                .Source(s => s.ExcludeAll())
            );

            searchResponse.ThrowIfInvalid();

            _telemetry.StopOperation(operation);

            var yearWeekAggregations = searchResponse
                .Aggregations
                .Terms("yearWeekAggregation")
                .Buckets?
                .Select(b =>
                    new
                    {
                        Year = int.Parse(b.Key.Substring(0, 4)),
                        Week = int.Parse(b.Key.Substring(5)),
                        DocCount = b.DocCount,
                        Quantity = b.Sum("quantity").Value
                    }).ToDictionary(b => $"{b.Year}-{b.Week}", b => b);

            //Fill in gaps in returned result
            var yearWeeks = new List<dynamic>();
            if (yearWeekAggregations?.Any() ?? false)
            {
                var currentYear = yearWeekAggregations.First().Value.Year;
                var currentWeek = yearWeekAggregations.First().Value.Week;
                var lastYear = yearWeekAggregations.Last().Value.Year;
                var lastWeek = yearWeekAggregations.Last().Value.Week;
                
                while(currentYear < lastYear || (currentYear == lastYear && currentWeek <= lastWeek))
                {
                    if (!yearWeekAggregations.TryGetValue($"{currentYear}-{currentWeek}", out var yearWeekAggregation))
                    {
                        yearWeekAggregation = new
                        {
                            Year = currentYear,
                            Week = currentWeek,
                            DocCount = (long?)0,
                            Quantity = (double?)0.0
                        };
                    }
                    yearWeeks.Add(yearWeekAggregation);

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
        public async Task<GeoGridTileResult> GetGeogridTileAggregationAsync(
                SearchFilter filter,
                int zoom)
        {
            var indexNames = GetCurrentIndex(filter);
            var (query, excludeQuery) = GetCoreQueries(filter);

            using var operation = _telemetry.StartOperation<DependencyTelemetry>("Observation_Search_GeoAggregated");
            operation.Telemetry.Properties["Filter"] = filter.ToString();

            var searchResponse = await Client.SearchAsync<dynamic>(s => s
                .Index(indexNames)
                .Aggregations(a => a
                    .Filter("geotile_filter", g => g
                        .Filter(f => f
                            .GeoBoundingBox(bb => bb
                                .Field("location.pointLocation")
                                .BoundingBox(b => b
                                    .TopLeft(filter.Location.Geometries.BoundingBox.TopLeft.ToGeoLocation())
                                    .BottomRight(filter.Location.Geometries.BoundingBox.BottomRight.ToGeoLocation())
                               )
                            )
                        )
                        .Aggregations(ab => ab
                            .GeoTile("geotile_grid", gg => gg
                                .Field("location.pointLocation")
                                .Size(MaxNrElasticSearchAggregationBuckets + 1)
                                .Precision((GeoTilePrecision)(zoom))
                                .Aggregations(b => b
                                    .Cardinality("taxa_count", t => t
                                    .Field("taxon.id"))
                                )
                            )
                        )
                    )
                )
                .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQuery)
                        .Filter(query)
                    )
                )
                .Size(0)
                .Source(s => s.ExcludeAll())
                .TrackTotalHits(false)
            );

            if (!searchResponse.IsValid)
            {
                if (searchResponse.ServerError?.Error?.CausedBy?.Type == "too_many_buckets_exception")
                {
                    throw new ArgumentOutOfRangeException($"The number of cells that will be returned is too large. The limit is {MaxNrElasticSearchAggregationBuckets} cells. Try using lower zoom or a smaller bounding box.");
                }
                searchResponse.ThrowIfInvalid();
            }

            var nrOfGridCells = (int?)searchResponse.Aggregations?.Filter("geotile_filter")?.GeoTile("geotile_grid")?.Buckets?.Count ?? 0;
            if (nrOfGridCells > MaxNrElasticSearchAggregationBuckets)
            {
                throw new ArgumentOutOfRangeException($"The number of cells that will be returned is too large. The limit is {MaxNrElasticSearchAggregationBuckets} cells. Try using lower zoom or a smaller bounding box.");
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
            return gridResult;
        }        

        /// <inheritdoc />
        public async Task<int> GetProvinceCountAsync(SearchFilterBase filter)
        {
            var indexNames = GetCurrentIndex(filter);
            var (query, excludeQuery) = GetCoreQueries(filter);

            var searchResponse = await Client.SearchAsync<dynamic>(s => s
                .Index(indexNames)
                .Aggregations(a => a.Cardinality("provinceCount", c => c
                    .Field("location.province.featureId")))
                .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQuery)
                        .Filter(query)
                    )
                )
                .Size(0)
                .Source(s => s.ExcludeAll())
                .TrackTotalHits(false)
            );

            searchResponse.ThrowIfInvalid();
            int provinceCount = Convert.ToInt32(searchResponse.Aggregations.Cardinality("provinceCount").Value);
            return provinceCount;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<YearCountResult>> GetUserYearCountAsync(SearchFilter filter)
        {
            try
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
                        .Composite("observationByYear", c => c
                            .Size(100) // 100 years
                            .Sources(s => s
                                .Terms("startYear", t => t
                                    .Field("event.startYear")
                                    .Order(SortOrder.Descending)
                                )
                            )
                            .Aggregations(a => a
                                .Cardinality("unique_taxonids", c => c
                                    .Field("taxon.id")
                                )
                            )
                        )
                    )
                    .Size(0)
                    .Source(s => s.ExcludeAll())
                    .TrackTotalHits(false)
                );

                searchResponse.ThrowIfInvalid();

                var result = new HashSet<YearCountResult>();
                foreach (var bucket in searchResponse.Aggregations.Composite("observationByYear").Buckets)
                {
                    var key = bucket.Key;

                    key.TryGetValue("startYear", out int startYear);
                    var count = bucket.DocCount;
                    var taxonCount = (long)bucket.Cardinality("unique_taxonids").Value;

                    result.Add(new YearCountResult
                    {
                        Count = count ?? 0,
                        TaxonCount = taxonCount,
                        Year = startYear
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
                        .Composite("observationByYearMonth", c => c
                            .Size(1200) // 12 months * 100 year
                            .Sources(s => s
                                .Terms("startYear", t => t
                                    .Field("event.startYear")
                                    .Order(SortOrder.Descending)
                                )
                                .Terms("startMonth", t => t
                                    .Field("event.startMonth")
                                    .Order(SortOrder.Descending)
                                )
                            )
                            .Aggregations(a => a
                                .Cardinality("unique_taxonids", c => c
                                    .Field("taxon.id")
                                )
                            )
                        )
                    )
                    .Size(0)
                    .Source(s => s.ExcludeAll())
                    .TrackTotalHits(false)
                );

                searchResponse.ThrowIfInvalid();

                var result = new HashSet<YearMonthCountResult>();
                foreach (var bucket in searchResponse.Aggregations.Composite("observationByYearMonth").Buckets)
                {
                    var key = bucket.Key;

                    key.TryGetValue("startYear", out int startYear);
                    key.TryGetValue("startMonth", out int startMonth);
                    var count = bucket.DocCount;
                    var taxonCount = (long)bucket.Cardinality("unique_taxonids").Value;

                    result.Add(new YearMonthCountResult
                    {
                        Count = count ?? 0,
                        Month = startMonth,
                        TaxonCount = taxonCount,
                        Year = startYear
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
                var (query, excludeQuery) = GetCoreQueries(filter);

                // First get observations count and taxon count group by day
                var searchResponse = await Client.SearchAsync<dynamic>(s => s
                   .Index(new[] { PublicIndexName, ProtectedIndexName })
                   .Query(q => q
                        .Bool(b => b
                            .MustNot(excludeQuery)
                            .Filter(query)
                        )
                    )
                    .Aggregations(a => a
                        .Composite("observationByYearMonth", c => c
                            .Size(skip + take) // Take as few as possible
                            .Sources(s => s
                                .Terms("startYear", t => t
                                    .Field("event.startYear")
                                    .Order(SortOrder.Descending)
                                )
                                .Terms("startMonth", t => t
                                    .Field("event.startMonth")
                                    .Order(SortOrder.Descending)
                                )
                                .Terms("startDay", t => t
                                    .Field("event.startDay")
                                    .Order(SortOrder.Descending)
                                )
                            )
                            .Aggregations(a => a
                                .Cardinality("unique_taxonids", c => c
                                    .Field("taxon.id")
                                )
                            )
                        )
                    )
                    .Size(0)
                    .Source(s => s.ExcludeAll())
                    .TrackTotalHits(false)
                );

                searchResponse.ThrowIfInvalid();

                var result = new Dictionary<string, YearMonthDayCountResult>();
                foreach (var bucket in searchResponse.Aggregations.Composite("observationByYearMonth").Buckets.Skip(skip))
                {
                    var key = bucket.Key;

                    key.TryGetValue("startYear", out int startYear);
                    key.TryGetValue("startMonth", out int startMonth);
                    key.TryGetValue("startDay", out int startDay);
                    var count = bucket.DocCount;
                    var taxonCount = (long)bucket.Cardinality("unique_taxonids").Value;

                    result.Add($"{startYear}-{startMonth}-{startDay}", new YearMonthDayCountResult
                    {
                        Count = count ?? 0,
                        Day = startDay,
                        Localities = new HashSet<IdName<string>>(),
                        Month = startMonth,
                        TaxonCount = taxonCount,
                        Year = startYear
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
                                .MustNot(excludeQuery)
                                .Filter(query)
                            )
                        )
                        .Aggregations(a => a
                            .Composite("localityByYearMonth", c => c
                                .Size((skip + take) * 10) // 10 locations for one day must be enought
                                .Sources(s => s
                                    .Terms("startYear", t => t
                                        .Field("event.startYear")
                                        .Order(SortOrder.Descending)
                                    )
                                    .Terms("startMonth", t => t
                                        .Field("event.startMonth")
                                        .Order(SortOrder.Descending)
                                    )
                                    .Terms("startDay", t => t
                                        .Field("event.startDay")
                                        .Order(SortOrder.Descending)
                                    )
                                     .Terms("locationId", t => t
                                        .Field("location.locationId")
                                        .Order(SortOrder.Descending)
                                    )
                                      .Terms("locality", t => t
                                        .Field("location.locality.raw")
                                        .Order(SortOrder.Descending)
                                    )
                                )

                            )
                        )
                        .Size(0)
                        .Source(s => s.ExcludeAll())
                        .TrackTotalHits(false)
                    );

                    searchResponse.ThrowIfInvalid();

                    // Add locations to result
                    foreach (var bucket in searchResponseLocality.Aggregations.Composite("localityByYearMonth").Buckets)
                    {
                        var key = bucket.Key;

                        key.TryGetValue("startYear", out int startYear);
                        key.TryGetValue("startMonth", out int startMonth);
                        key.TryGetValue("startDay", out int startDay);
                        key.TryGetValue("locationId", out string locationId);
                        key.TryGetValue("locality", out string locality);
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
                    .Terms("uniqueOccurrenceIdCount", t => t
                        .Field(f => f.Occurrence.OccurrenceId)
                        .MinimumDocumentCount(2)
                        .Size(1)
                    )
                )
                .Size(0)
                .Source(s => s.ExcludeAll())
                .TrackTotalHits(false)
            );

            searchResponse.ThrowIfInvalid();

            return searchResponse.Aggregations
                .Terms("uniqueOccurrenceIdCount")
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

            var (query, excludeQuery) = GetCoreQueries(filter, true);
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

            searchResponse.ThrowIfInvalid();

            return searchResponse.Count > 0;
        }
    }
}
