﻿using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Nest;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Extensions;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Gis;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Processed.Configuration;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Models.Search.Result;
using SOS.Lib.Models.TaxonTree;
using SOS.Lib.Repositories.Processed;
using SOS.Observations.Api.Repositories.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Result = CSharpFunctionalExtensions.Result;

namespace SOS.Observations.Api.Repositories
{
    /// <summary>
    ///     Species data service
    /// </summary>
    public class ProcessedTaxonRepository : ProcessedObservationBaseRepository,
        IProcessedTaxonRepository
    {
        private readonly ITaxonManager _taxonManager;

        private class TaxonProvinceItem
        {
            public int TaxonId { get; set; }
            public string ProvinceId { get; set; }
            public int ObservationCount { get; set; }
        }

        private class TaxonProvinceAgg
        {
            public int TaxonId { get; set; }
            public List<string> ProvinceIds { get; set; } = new List<string>();
            public Dictionary<string, int> ObservationCountByProvinceId { get; set; } = new Dictionary<string, int>();
            public int ObservationCount { get; set; }
        }

        private class TaxonAggregationTreeNodeSum
        {
            public DateTime? FirstSighting { get; set; }
            public DateTime? LastSighting { get; set; }
            public int TopologicalIndex { get; set; }
            public TaxonTreeNode<IBasicTaxon> TreeNode { get; set; }
            public int ObservationCount { get; set; }
            public int SumObservationCount { get; set; }
            public Dictionary<string, int> ObservationCountByProvinceId { get; set; } = new Dictionary<string, int>();
            public Dictionary<string, int> SumObservationCountByProvinceId { get; set; } = new Dictionary<string, int>();
            public int ProvinceCount { get; set; }
            public int SumProvinceCount => DependentProvinceIds == null ? 0 : DependentProvinceIds.Count;
            public HashSet<string> DependentProvinceIds { get; set; }
            public HashSet<int> DependentTaxonIds { get; set; }

            //public TaxonAggregationTreeNodeSum MainParent { get; set; } // Uncomment to use for debug purpose
            //public HashSet<TaxonAggregationTreeNodeSum> SecondaryParents { get; set; } = new HashSet<TaxonAggregationTreeNodeSum>(); // Uncomment to use for debug purpose
            //public HashSet<TaxonAggregationTreeNodeSum> MainChildren { get; set; } = new HashSet<TaxonAggregationTreeNodeSum>(); // Uncomment to use for debug purpose
            //public HashSet<TaxonAggregationTreeNodeSum> SecondaryChildren { get; set; } = new HashSet<TaxonAggregationTreeNodeSum>(); // Uncomment to use for debug purpose

            public override bool Equals(object obj)
            {
                return obj is TaxonAggregationTreeNodeSum sum &&
                       EqualityComparer<TaxonTreeNode<IBasicTaxon>>.Default.Equals(TreeNode, sum.TreeNode);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(TreeNode);
            }

            public override string ToString()
            {
                if (TreeNode != null) return $"TaxonId: {TreeNode.TaxonId}, Count: {ObservationCount:N0}, SumCount: {SumObservationCount:N0}";
                return base.ToString();
            }
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
        ///  Count observations per taxon
        /// </summary>
        /// <param name="indexName"></param>
        /// <param name="query"></param>
        /// <param name="excludeQuery"></param>
        /// <param name="taxonCount"></param>
        /// <returns></returns>
        private async Task<Dictionary<int, (int, DateTime?, DateTime?)>> GetAllObservationCountByTaxonIdAsync(
            string indexName,
            ICollection<Func<QueryDescriptor<dynamic>, QueryContainer>> query,
            ICollection<Func<QueryDescriptor<object>, QueryContainer>> excludeQuery,
            int taxonCount)
        {
            if (taxonCount is > 0 and <= 10000)
            {
                return await TaxaTermsAggregationAsync(indexName, query, excludeQuery, taxonCount);
            }

            var observationCountByTaxonId = new Dictionary<int, (int, DateTime?, DateTime?)>();
            CompositeKey nextPageKey = null;
            var take = taxonCount > 0 && taxonCount < MaxNrElasticSearchAggregationBuckets ? taxonCount : MaxNrElasticSearchAggregationBuckets;
            do
            {
                var searchResponse = await PageTaxaCompositeAggregationAsync(indexName, query, excludeQuery, nextPageKey, take);
                var compositeAgg = searchResponse.Aggregations.Composite("taxonComposite");
                foreach (var bucket in compositeAgg.Buckets)
                {
                    var taxonId = Convert.ToInt32((long)bucket.Key["taxonId"]);
                    var firstSighting = DateTime.Parse(bucket.Min("firstSighting").ValueAsString);
                    var lastSighting = DateTime.Parse(bucket.Max("lastSighting").ValueAsString);

                    observationCountByTaxonId.Add(taxonId,
                        (
                            Convert.ToInt32(bucket.DocCount.GetValueOrDefault(0)),
                            firstSighting,
                            lastSighting
                        )
                    );
                }

                nextPageKey = compositeAgg.Buckets.Count >= take ? compositeAgg.AfterKey : null;
            } while (nextPageKey != null);


            return observationCountByTaxonId;
        }

        private async Task<Dictionary<int, (int, DateTime?, DateTime?)>> GetTaxonAggregationAsync(SearchFilter filter)
        {
            var indexName = GetCurrentIndex(filter);
            var (query, excludeQuery) = GetCoreQueries(filter);
            var observationCountByTaxonId = await GetAllObservationCountByTaxonIdAsync(
                indexName,
                query,
                excludeQuery,
                filter?.Taxa?.Ids?.Count() ?? 0);
            return observationCountByTaxonId;
        }

        private async Task<Dictionary<int, (int, DateTime?, DateTime?)>> GetTaxonAggregationSumAsync(SearchFilter filter)
        {
            var indexName = GetCurrentIndex(filter);
            Dictionary<int, (int, DateTime?, DateTime?)> observationCountByTaxonId = null;
            var taxonTree = await _taxonManager.GetTaxonTreeAsync();

            // Save requested taxon id's. If biota is requested, id's will be empty then we add all taxon id's to make sure we got all parents for sum calculation
            var taxonIds = new HashSet<int>((filter.Taxa?.Ids?.Any() ?? false) ? filter.Taxa.Ids : taxonTree.GetUnderlyingTaxonIds(0, true));

            // If IncludeUnderlyingTaxa = true, underlying taxa are allready added. If false we need to add it here to get observations for them to use in calculation
            if ((!filter.Taxa?.IncludeUnderlyingTaxa ?? true) && (filter.Taxa?.Ids?.Any() ?? false))
            {
                filter.Taxa.Ids = taxonTree.GetUnderlyingTaxonIds(filter.Taxa?.Ids, true);
            }

            var (query, excludeQuery) = GetCoreQueries(filter);
            observationCountByTaxonId = await GetAllObservationCountByTaxonIdAsync(
                indexName,
                query,
                excludeQuery,
                filter?.Taxa?.Ids?.Count() ?? 0);

            if (taxonIds?.Any() ?? false)
            {
                // Add requested taxon with no observations
                foreach (var taxonId in taxonIds)
                {
                    observationCountByTaxonId.TryAdd(taxonId, (0, null, null));
                }
            }

            var treeNodeSumByTaxonId = new Dictionary<int, TaxonAggregationTreeNodeSum>();                       
            foreach (var item in taxonTree.TreeNodeById.Values)
            {
                var (observationCount, firstSighting, lastSighting) = observationCountByTaxonId.GetValueOrDefault(item.TaxonId);
                var sumNode = new TaxonAggregationTreeNodeSum
                {
                    FirstSighting = firstSighting,
                    LastSighting = lastSighting,
                    TopologicalIndex = taxonTree.ReverseTopologicalSortById[item.TaxonId],
                    TreeNode = item,
                    ObservationCount = observationCount,
                    SumObservationCount = observationCount,
                    DependentTaxonIds = new HashSet<int>() { item.TaxonId }
                };
                treeNodeSumByTaxonId.Add(item.TaxonId, sumNode);
            }

            foreach (var sumNode in treeNodeSumByTaxonId.Values.OrderBy(m => m.TopologicalIndex))
            {
                // Main parent
                if (sumNode.TreeNode.Parent != null)
                {
                    if (treeNodeSumByTaxonId.TryGetValue(sumNode.TreeNode.Parent.TaxonId, out var parentSumNode))
                    {
                        // sumNode.MainParent = parentSumNode; // Uncomment to use for debug purpose
                        // parentSumNode.MainChildren.Add(sumNode); // Uncomment to use for debug purpose
                        var newDependentTaxonIds = sumNode.DependentTaxonIds.Except(parentSumNode.DependentTaxonIds);
                        parentSumNode.DependentTaxonIds.UnionWith(newDependentTaxonIds);
                        //  parentSumNode.Hits = parentSumNode.Hits == null ? sumNode.Hits : parentSumNode.Hits.Union(sumNode.Hits).OrderByDescending(h => h.Item1).Take(noOfLatestHits);
                        foreach (var taxonId in newDependentTaxonIds)
                        {
                            var childSumNode = treeNodeSumByTaxonId[taxonId];
                            parentSumNode.SumObservationCount += childSumNode.ObservationCount;
                            if (parentSumNode.FirstSighting == null || parentSumNode.FirstSighting > childSumNode.FirstSighting)
                            {
                                parentSumNode.FirstSighting = childSumNode.FirstSighting;
                            }
                            if (parentSumNode.LastSighting == null || parentSumNode.LastSighting < childSumNode.LastSighting)
                            {
                                parentSumNode.LastSighting = childSumNode.LastSighting;
                            }
                        }
                    }
                }

                // Secondary parent
                if (sumNode.TreeNode.SecondaryParents?.Any() ?? false)
                {
                    foreach (var secondaryParent in sumNode.TreeNode.SecondaryParents)
                    {
                        if (treeNodeSumByTaxonId.TryGetValue(secondaryParent.TaxonId, out var secondaryParentSumNode))
                        {
                            // sumNode.SecondaryParents.Add(secondaryParentSumNode); // Uncomment to use for debug purpose
                            // secondaryParentSumNode.SecondaryChildren.Add(sumNode); // Uncomment to use for debug purpose
                            var newDependentTaxonIds = sumNode.DependentTaxonIds.Except(secondaryParentSumNode.DependentTaxonIds).ToList();
                            secondaryParentSumNode.DependentTaxonIds.UnionWith(newDependentTaxonIds);
                            foreach (var taxonId in newDependentTaxonIds)
                            {
                                var childSumNode = treeNodeSumByTaxonId[taxonId];
                                secondaryParentSumNode.SumObservationCount += childSumNode.ObservationCount;
                                if (secondaryParentSumNode.FirstSighting == null || secondaryParentSumNode.FirstSighting > childSumNode.FirstSighting)
                                {
                                    secondaryParentSumNode.FirstSighting = childSumNode.FirstSighting;
                                }
                                if (secondaryParentSumNode.LastSighting == null || secondaryParentSumNode.LastSighting < childSumNode.LastSighting)
                                {
                                    secondaryParentSumNode.LastSighting = childSumNode.LastSighting;
                                }
                            }
                        }
                    }
                }
            }

            // Replace observation count with accumulated sum. Remove nodes with 0 observations.
            foreach (var taxonId in observationCountByTaxonId.Keys)
            {
                if (treeNodeSumByTaxonId.TryGetValue(taxonId, out var sumNode))
                {
                    if (sumNode.SumObservationCount > 0 && (!taxonIds.Any() || taxonIds.Contains(taxonId)))
                    {
                        observationCountByTaxonId[taxonId] = (sumNode.SumObservationCount, sumNode.FirstSighting, sumNode.LastSighting);
                    }
                    else // Remove taxon with no observations or underlying taxa not requested by user
                    {
                        observationCountByTaxonId.Remove(taxonId);
                    }
                }
            }

            return observationCountByTaxonId;
        }

        private async Task<Dictionary<int, TaxonProvinceAgg>> GetElasticTaxonSumAggregationByTaxonIdAsync(
            string indexName,
            ICollection<Func<QueryDescriptor<dynamic>, QueryContainer>> query,
            ICollection<Func<QueryDescriptor<object>, QueryContainer>> excludeQuery)
        {
            var items = new List<TaxonProvinceItem>();
            CompositeKey nextPageKey = null;
            var pageTaxaAsyncTake = MaxNrElasticSearchAggregationBuckets;
            do
            {
                var searchResponse = await TaxonProvinceCompositeAggregationAsync(indexName, query, excludeQuery, nextPageKey, pageTaxaAsyncTake);
                var compositeAgg = searchResponse.Aggregations.Composite("taxonComposite");
                foreach (var bucket in compositeAgg.Buckets)
                {
                    var taxonId = Convert.ToInt32((long)bucket.Key["taxonId"]);
                    var provinceId = bucket.Key["provinceId"].ToString();
                    var observationCount = Convert.ToInt32(bucket.DocCount.GetValueOrDefault(0));
                    items.Add(new TaxonProvinceItem
                    {
                        TaxonId = taxonId,
                        ProvinceId = provinceId,
                        ObservationCount = observationCount
                    });
                }

                nextPageKey = compositeAgg.Buckets.Count >= pageTaxaAsyncTake ? compositeAgg.AfterKey : null;
            } while (nextPageKey != null);

            var dic = new Dictionary<int, TaxonProvinceAgg>();
            foreach (var item in items)
            {
                if (!dic.TryGetValue(item.TaxonId, out var taxonProvinceAgg))
                {
                    taxonProvinceAgg = new TaxonProvinceAgg() { TaxonId = item.TaxonId };
                    dic.Add(item.TaxonId, taxonProvinceAgg);
                }

                taxonProvinceAgg.ObservationCount += item.ObservationCount;
                taxonProvinceAgg.ProvinceIds.Add(item.ProvinceId);
                taxonProvinceAgg.ObservationCountByProvinceId.Add(item.ProvinceId, item.ObservationCount);
            }

            return dic;
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
            ICollection<Func<QueryDescriptor<dynamic>, QueryContainer>> query,
            ICollection<Func<QueryDescriptor<object>, QueryContainer>> excludeQuery,
            int zoom,
            CompositeKey nextPage)
        {
            ISearchResponse<dynamic> searchResponse;

            searchResponse = await Client.SearchAsync<dynamic>(s => s
                .Index(PublicIndexName)
                .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQuery)
                        .Filter(query)
                    )
                )
                .Aggregations(a => a.Composite("geoTileTaxonComposite", g => g
                    .Size(MaxNrElasticSearchAggregationBuckets + 1)
                    .After(nextPage ?? new CompositeKey(new Dictionary<string, object>() { { "geoTile", "0/0/0" }, { "taxon", 0 } }))
                    .Sources(src => src
                        .GeoTileGrid("geoTile", h => h
                            .Field("location.pointLocation")
                            .Precision((GeoTilePrecision)zoom).Order(SortOrder.Ascending))
                        .Terms("taxon", tt => tt
                            .Field("taxon.id").Order(SortOrder.Ascending)
                        ))))

                .Size(0)
                .Source(s => s.ExcludeAll())
                .TrackTotalHits(false)
            );

            searchResponse.ThrowIfInvalid();

            return searchResponse;
        }

        private async Task<ISearchResponse<dynamic>> PageTaxaCompositeAggregationAsync(
            string indexName,
            ICollection<Func<QueryDescriptor<dynamic>, QueryContainer>> query,
            ICollection<Func<QueryDescriptor<object>, QueryContainer>> excludeQuery,
            CompositeKey nextPage,
            int take)
        {
            ISearchResponse<dynamic> searchResponse;

            searchResponse = await Client.SearchAsync<dynamic>(s => s
                .Index(indexName)
                .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQuery)
                        .Filter(query)
                    )
                )
                .Aggregations(a => a
                    .Composite("taxonComposite", g => g
                        .After(nextPage)
                        .Size(take)
                        .Sources(src => src
                            .Terms("taxonId", tt => tt
                                .Field("taxon.id")
                            )
                        )
                        .Aggregations(a => a
                            .Min("firstSighting", m => m
                                .Field("event.startDate")
                            )
                            .Max("lastSighting", m => m
                                .Field("event.startDate")
                            )
                        /*.TopHits("latestRecordedObservations", th => th
                             .Size(noOfLatestHits)
                             .Source(src => src
                                 .Includes(inc => inc
                                     .Fields("event.startDate", "occurrence.occurrenceId")
                                 )
                             )
                             .Sort(s => s.Descending("event.startDate"))
                         )*/
                        )
                    )
                )
                .Size(0)
                .Source(s => s.ExcludeAll())
                .TrackTotalHits(false)
            );

            searchResponse.ThrowIfInvalid();

            return searchResponse;
        }

        private async Task<ISearchResponse<dynamic>> TaxonProvinceCompositeAggregationAsync(
            string indexName,
            ICollection<Func<QueryDescriptor<dynamic>, QueryContainer>> query,
            ICollection<Func<QueryDescriptor<object>, QueryContainer>> excludeQuery,
            CompositeKey nextPage,
            int take)
        {
            var searchResponse = await Client.SearchAsync<dynamic>(s => s
                .Index(indexName)
                .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQuery)
                        .Filter(query)
                    )
                )
                .Aggregations(a => a.Composite("taxonComposite", g => g
                    .Size(take)
                    .After(nextPage ?? new CompositeKey(new Dictionary<string, object>() { { "taxonId", 0 }, { "provinceId", "" } }))
                    .Sources(src => src
                        .Terms("taxonId", tt => tt
                            .Field("taxon.id"))
                        .Terms("provinceId", p => p
                            .Field("location.province.featureId"))
                        )))

                .Size(0)
                .Source(s => s.ExcludeAll())
                .TrackTotalHits(false)
                .RequestConfiguration(r => r
                    .RequestTimeout(TimeSpan.FromMinutes(5)) // 5 minutes timeout
                )
            );

            searchResponse.ThrowIfInvalid();

            return searchResponse;
        }

        private async Task<Dictionary<int, (int, DateTime?, DateTime?)>> TaxaTermsAggregationAsync(
            string indexName,
            ICollection<Func<QueryDescriptor<dynamic>, QueryContainer>> query,
            ICollection<Func<QueryDescriptor<object>, QueryContainer>> excludeQuery,
            int size)
        {
            var searchResponse = await Client.SearchAsync<dynamic>(s => s
                .Index(indexName)
                .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQuery)
                        .Filter(query)
                    )
                )
                .Aggregations(a => a
                    .Terms("taxa", t => t
                        .Field("taxon.id")
                        .Size(size)
                        .Aggregations(a => a
                            .Min("firstSighting", m => m
                                .Field("event.startDate")
                            )
                            .Max("lastSighting", m => m
                                .Field("event.startDate")
                            )
                        )
                    )
                )
                .Size(0)
                .Source(s => s.ExcludeAll())
                .TrackTotalHits(false)
            );

            searchResponse.ThrowIfInvalid();

            var observationCountByTaxonId = new Dictionary<int, (int, DateTime?, DateTime?)>();
            foreach (var bucket in searchResponse.Aggregations.Terms("taxa").Buckets)
            {
                observationCountByTaxonId.Add(int.Parse(bucket.Key),
                    (
                        Convert.ToInt32(bucket.DocCount.GetValueOrDefault(0)),
                        DateTime.Parse(bucket.Min("firstSighting").ValueAsString),
                        DateTime.Parse(bucket.Max("lastSighting").ValueAsString)
                    )
                );
            }

            return observationCountByTaxonId;
        }

        /// <summary>
        /// Constructor used in public mode
        /// </summary>
        /// <param name="elasticClientManager"></param>
        /// <param name="elasticConfiguration"></param>
        /// <param name="processedConfigurationCache"></param>
        /// <param name="taxonManager"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public ProcessedTaxonRepository(
            IElasticClientManager elasticClientManager,
            ElasticSearchConfiguration elasticConfiguration,
            ICache<string, ProcessedConfiguration> processedConfigurationCache,
            ITaxonManager taxonManager,
            IClassCache<ConcurrentDictionary<string, ClusterHealthResponse>> clusterHealthCache,
            ILogger<ProcessedTaxonRepository> logger) : base(true, elasticClientManager, processedConfigurationCache, elasticConfiguration, clusterHealthCache, logger)
        {
            _taxonManager = taxonManager ?? throw new ArgumentNullException(nameof(taxonManager));
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
        public async Task<Result<PagedResult<TaxonAggregationItem>>> GetTaxonAggregationAsync(
            SearchFilter filter,
            int? skip,
            int? take,
            bool sumUnderlyingTaxa = false)
        {
            Dictionary<int, (int, DateTime?, DateTime?)> observationCountByTaxonId = null;
            if (sumUnderlyingTaxa)
            {
                observationCountByTaxonId = await GetTaxonAggregationSumAsync(filter);
            }
            else
            {
                observationCountByTaxonId = await GetTaxonAggregationAsync(filter);
            }

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
                        b.Value.Item1,
                        b.Value.Item2,
                        b.Value.Item3
                    )
                //  b.Value.Item4?.Select(h => new TaxonAggregationHit { EventStartDate = h.Item1, OccurrenceId = h.Item2 }))
                )
                .OrderByDescending(m => m.ObservationCount)
                .ThenBy(m => m.TaxonId)
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

        /// <summary>
        /// Get taxon sum aggregation. Including underlying taxa and province count.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public async Task<Dictionary<int, TaxonSumAggregationItem>> GetTaxonSumAggregationAsync(SearchFilter filter)
        {
            var indexName = GetCurrentIndex(filter);
            Dictionary<int, TaxonProvinceAgg> observationCountByTaxonId = null;

            var (queryWithoutTaxaFilter, excludeQueryWithoutTaxaFilter) = GetCoreQueries(filter);
            observationCountByTaxonId = await GetElasticTaxonSumAggregationByTaxonIdAsync(
                indexName,
                queryWithoutTaxaFilter,
                excludeQueryWithoutTaxaFilter);

            var treeNodeSumByTaxonId = new Dictionary<int, TaxonAggregationTreeNodeSum>();
            var taxonTree = await _taxonManager.GetTaxonTreeAsync();            
            foreach (var item in taxonTree.TreeNodeById.Values)
            {
                var taxonProvinceAgg = observationCountByTaxonId.GetValueOrDefault(item.TaxonId);
                var sumNode = new TaxonAggregationTreeNodeSum
                {
                    TopologicalIndex = taxonTree.ReverseTopologicalSortById[item.TaxonId],
                    TreeNode = item,
                    ObservationCount = taxonProvinceAgg?.ObservationCount ?? 0,
                    SumObservationCount = taxonProvinceAgg?.ObservationCount ?? 0,
                    ProvinceCount = taxonProvinceAgg?.ProvinceIds?.Count ?? 0,
                    DependentProvinceIds = new HashSet<string>() { },
                    DependentTaxonIds = new HashSet<int>() { item.TaxonId }
                };
                if (taxonProvinceAgg != null)
                {
                    sumNode.DependentProvinceIds.UnionWith(taxonProvinceAgg.ProvinceIds);
                    foreach (var pair in taxonProvinceAgg.ObservationCountByProvinceId)
                    {
                        sumNode.ObservationCountByProvinceId.Add(pair.Key, pair.Value);
                        sumNode.SumObservationCountByProvinceId.Add(pair.Key, pair.Value);
                    }
                }

                treeNodeSumByTaxonId.Add(item.TaxonId, sumNode);
            }
            treeNodeSumByTaxonId = treeNodeSumByTaxonId.OrderBy(m => m.Value.TopologicalIndex).ToDictionary(tn => tn.Key, tn => tn.Value);

            foreach (var sumNode in treeNodeSumByTaxonId.Values)
            {
                // Main parent
                if (sumNode.TreeNode.Parent != null)
                {
                    if (treeNodeSumByTaxonId.TryGetValue(sumNode.TreeNode.Parent.TaxonId, out var parentSumNode))
                    {
                        //sumNode.MainParent = parentSumNode; // Uncomment to use for debug purpose
                        //parentSumNode.MainChildren.Add(sumNode); // Uncomment to use for debug purpose
                        var newDependentTaxonIds = sumNode.DependentTaxonIds.Except(parentSumNode.DependentTaxonIds).ToList();
                        parentSumNode.DependentTaxonIds.UnionWith(newDependentTaxonIds);
                        parentSumNode.DependentProvinceIds.UnionWith(sumNode.DependentProvinceIds);
                        foreach (var taxonId in newDependentTaxonIds)
                        {
                            parentSumNode.SumObservationCount += treeNodeSumByTaxonId[taxonId].ObservationCount;

                            foreach (var pair in treeNodeSumByTaxonId[taxonId].ObservationCountByProvinceId)
                            {
                                if (!parentSumNode.SumObservationCountByProvinceId.TryAdd(pair.Key, pair.Value))
                                {
                                    parentSumNode.SumObservationCountByProvinceId[pair.Key] += pair.Value;
                                }
                            }
                        }
                    }
                }

                // Secondary parent
                if (sumNode.TreeNode.SecondaryParents != null && sumNode.TreeNode.SecondaryParents.Count > 0)
                {
                    foreach (var secondaryParent in sumNode.TreeNode.SecondaryParents)
                    {
                        if (treeNodeSumByTaxonId.TryGetValue(secondaryParent.TaxonId, out var secondaryParentSumNode))
                        {
                            //sumNode.SecondaryParents.Add(secondaryParentSumNode); // Uncomment to use for debug purpose
                            //secondaryParentSumNode.SecondaryChildren.Add(sumNode); // Uncomment to use for debug purpose
                            var newDependentTaxonIds = sumNode.DependentTaxonIds.Except(secondaryParentSumNode.DependentTaxonIds).ToList();
                            secondaryParentSumNode.DependentTaxonIds.UnionWith(newDependentTaxonIds);
                            secondaryParentSumNode.DependentProvinceIds.UnionWith(sumNode.DependentProvinceIds);
                            foreach (var taxonId in newDependentTaxonIds)
                            {
                                secondaryParentSumNode.SumObservationCount += treeNodeSumByTaxonId[taxonId].ObservationCount;
                                foreach (var pair in treeNodeSumByTaxonId[taxonId].ObservationCountByProvinceId)
                                {
                                    if (!secondaryParentSumNode.SumObservationCountByProvinceId.TryAdd(pair.Key, pair.Value))
                                    {
                                        secondaryParentSumNode.SumObservationCountByProvinceId[pair.Key] += pair.Value;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            var result = new Dictionary<int, TaxonSumAggregationItem>();
            foreach (var node in treeNodeSumByTaxonId.Values)
            {
                var agg = new TaxonSumAggregationItem()
                {
                    TaxonId = node.TreeNode.TaxonId,
                    ObservationCount = node.ObservationCount,
                    SumObservationCount = node.SumObservationCount,
                    ProvinceCount = node.ProvinceCount,
                    SumProvinceCount = node.SumProvinceCount,
                    SumObservationCountByProvinceId = node.SumObservationCountByProvinceId
                };

                result.Add(node.TreeNode.TaxonId, agg);
            }

            return result;
        }


        /// <inheritdoc />
        public async Task<IEnumerable<TaxonAggregationItem>> GetTaxonExistsIndicationAsync(
            SearchFilter filter)
        {
            var indexNames = GetCurrentIndex(filter);
            var (query, excludeQuery) = GetCoreQueries(filter);

            var searchResponse = await Client.SearchAsync<dynamic>(s => s
                .Index(indexNames)
                .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQuery)
                        .Filter(query)
                    )
                )
                .Aggregations(a => a
                    .Terms("taxon_group", t => t
                        .Size(filter.Taxa?.Ids?.Count() ?? 0) // Size can never be grater than number of taxon id's
                        .Field("taxon.id")
                    )
                )
                .Size(0)
                .Source(s => s.ExcludeAll())
                .TrackTotalHits(false)
            );

            searchResponse.ThrowIfInvalid();

            return searchResponse.Aggregations
                .Terms("taxon_group")
                .Buckets
                .Select(b => new TaxonAggregationItem { TaxonId = int.Parse(b.Key), ObservationCount = (int)(b.DocCount ?? 0) });
        }
    }
}
