using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Nest;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Gis;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Processed.Configuration;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Models.Search.Result;
using SOS.Lib.Models.TaxonTree;
using SOS.Lib.Repositories.Processed.Interfaces;
using DateTime = System.DateTime;
using Result = CSharpFunctionalExtensions.Result;

namespace SOS.Lib.Repositories.Processed
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
        /// 
        /// </summary>
        /// <param name="indexName"></param>
        /// <param name="query"></param>
        /// <param name="excludeQuery"></param>
        /// <returns></returns>
        private async Task<Dictionary<int, (int, DateTime?, DateTime?)>> GetAllObservationCountByTaxonIdAsync(
            string indexName,
            ICollection<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> query,
            ICollection<Func<QueryContainerDescriptor<object>, QueryContainer>> excludeQuery)
        {
            var observationCountByTaxonId = new Dictionary<int, (int, DateTime?, DateTime?)>();
            CompositeKey nextPageKey = null;
            var pageTaxaAsyncTake = MaxNrElasticSearchAggregationBuckets;
            do
            {
                var searchResponse = await PageTaxaCompositeAggregationAsync(indexName, query, excludeQuery, nextPageKey, pageTaxaAsyncTake);
                var compositeAgg = searchResponse.Aggregations.Composite("taxonComposite");
                foreach (var bucket in compositeAgg.Buckets)
                {
                    var taxonId = Convert.ToInt32((long)bucket.Key["taxonId"]);
                    var firstSighting = DateTime.Parse(bucket.Min("firstSighting").ValueAsString);
                    var lastSighting = DateTime.Parse(bucket.Max("lastSighting").ValueAsString);

                    observationCountByTaxonId.Add(taxonId, (Convert.ToInt32(bucket.DocCount.GetValueOrDefault(0)), firstSighting, lastSighting));
                }

                nextPageKey = compositeAgg.Buckets.Count >= pageTaxaAsyncTake ? compositeAgg.AfterKey : null;
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
                excludeQuery);
            return observationCountByTaxonId;
        }

        private async Task<Dictionary<int, (int, DateTime?, DateTime?)>> GetTaxonAggregationSumAsync(SearchFilter filter)
        {
            var indexName = GetCurrentIndex(filter);
            Dictionary<int, (int, DateTime?, DateTime?)> observationCountByTaxonId = null;
            Dictionary<int, (int, DateTime?, DateTime?)> outputCountByTaxonId = null;
            if (filter.HasTaxonFilter())
            {
                var filterWithoutTaxaFilter = filter.Clone();
                filterWithoutTaxaFilter.Taxa = null;
                var (queryWithoutTaxaFilter, excludeQueryWithoutTaxaFilter) = GetCoreQueries(filterWithoutTaxaFilter);
                observationCountByTaxonId = await GetAllObservationCountByTaxonIdAsync(
                indexName,
                queryWithoutTaxaFilter,
                excludeQueryWithoutTaxaFilter);

                var (query, excludeQuery) = GetCoreQueries(filter);
                outputCountByTaxonId = await GetAllObservationCountByTaxonIdAsync(
                    indexName,
                    query,
                    excludeQuery);

                if (filter.Taxa.IncludeUnderlyingTaxa && (filter.Taxa.Ids == null || !filter.Taxa.Ids.Any()))
                {
                    filter.Taxa.Ids = new int[] { 0 }; // Add Biota if IncludeUnderlyingTaxa and there are no Taxon Ids.
                }

                if (filter.Taxa.Ids != null && filter.Taxa.Ids.Any())
                {
                    IEnumerable<int> taxonIds = filter.Taxa.IncludeUnderlyingTaxa ?
                        _taxonManager.TaxonTree.GetUnderlyingTaxonIds(filter.Taxa.Ids, true) : filter.Taxa.Ids;

                    foreach (var taxonId in taxonIds)
                    {
                        outputCountByTaxonId.TryAdd(taxonId, (0, null, null));
                    }
                }
            }
            else
            {
                var (query, excludeQuery) = GetCoreQueries(filter);
                outputCountByTaxonId = await GetAllObservationCountByTaxonIdAsync(
                    indexName,
                    query,
                    excludeQuery);
                observationCountByTaxonId = outputCountByTaxonId;
            }

            var treeNodeSumByTaxonId = new Dictionary<int, TaxonAggregationTreeNodeSum>();
            var tree = _taxonManager.TaxonTree;
            foreach (var item in tree.TreeNodeById.Values)
            {
                var (observationCount, firstSighting, lastSighting) = observationCountByTaxonId.GetValueOrDefault(item.TaxonId);
                var sumNode = new TaxonAggregationTreeNodeSum
                {
                    FirstSighting = firstSighting,
                    LastSighting = lastSighting,
                    TopologicalIndex = tree.ReverseTopologicalSortById[item.TaxonId],
                    TreeNode = item,
                    ObservationCount = observationCount,
                    SumObservationCount = observationCount,
                    DependentTaxonIds = new HashSet<int>() { item.TaxonId }
                };
                treeNodeSumByTaxonId.Add(item.TaxonId, sumNode);
            }

            var orderedTreeNodeSum = treeNodeSumByTaxonId.Values.OrderBy(m => m.TopologicalIndex).ToList();
            foreach (var sumNode in orderedTreeNodeSum)
            {
                // Main parent
                if (sumNode.TreeNode.Parent != null)
                {
                    if (treeNodeSumByTaxonId.TryGetValue(sumNode.TreeNode.Parent.TaxonId, out var parentSumNode))
                    {
                        // sumNode.MainParent = parentSumNode; // Uncomment to use for debug purpose
                        // parentSumNode.MainChildren.Add(sumNode); // Uncomment to use for debug purpose
                        var newDependedntTaxonIds = sumNode.DependentTaxonIds.Except(parentSumNode.DependentTaxonIds).ToList();
                        parentSumNode.DependentTaxonIds.UnionWith(newDependedntTaxonIds);
                        foreach (var taxonId in newDependedntTaxonIds)
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
                if (sumNode.TreeNode.SecondaryParents != null && sumNode.TreeNode.SecondaryParents.Count > 0)
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
                                if (secondaryParentSumNode.FirstSighting > childSumNode.FirstSighting)
                                {
                                    secondaryParentSumNode.FirstSighting = childSumNode.FirstSighting;
                                }
                                if (secondaryParentSumNode.LastSighting < childSumNode.LastSighting)
                                {
                                    secondaryParentSumNode.LastSighting = childSumNode.LastSighting;
                                }
                            }
                        }
                    }
                }
            }

            // Replace observation count with accumulated sum. Remove nodes with 0 observations.
            foreach (var taxonId in outputCountByTaxonId.Keys)
            {
                if (treeNodeSumByTaxonId.TryGetValue(taxonId, out var sumNode))
                {
                    if (sumNode.SumObservationCount > 0)
                    {
                        outputCountByTaxonId[taxonId] = (sumNode.SumObservationCount, sumNode.FirstSighting, sumNode.LastSighting);
                    }
                    else
                    {
                        outputCountByTaxonId.Remove(taxonId);
                    }
                }
            }

            return outputCountByTaxonId;
        }

        private async Task<Dictionary<int, TaxonProvinceAgg>> GetElasticTaxonSumAggregationByTaxonIdAsync(
            string indexName,
            ICollection<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> query,
            ICollection<Func<QueryContainerDescriptor<object>, QueryContainer>> excludeQuery)
        {
            List<TaxonProvinceItem> items = new List<TaxonProvinceItem>();
            CompositeKey nextPageKey = null;
            var pageTaxaAsyncTake = MaxNrElasticSearchAggregationBuckets;
            do
            {
                var searchResponse = await PageElasticTaxonSumAggregationAsync(indexName, query, excludeQuery, nextPageKey, pageTaxaAsyncTake);
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
            ICollection<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> query,
            ICollection<Func<QueryContainerDescriptor<object>, QueryContainer>> excludeQuery,
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

            if (!searchResponse.IsValid)
            {
                throw new InvalidOperationException(searchResponse.DebugInformation);
            }

            return searchResponse;
        }

        private async Task<ISearchResponse<dynamic>> PageTaxaCompositeAggregationAsync(
            string indexName,
            ICollection<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> query,
            ICollection<Func<QueryContainerDescriptor<object>, QueryContainer>> excludeQuery,
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
                        .After(nextPage ?? new CompositeKey(new Dictionary<string, object>() { { "taxonId", 0 } }))
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
                           /* .TopHits("latestRecordedOccurrenceIds", th => th
                                .Size(10)
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

            if (!searchResponse.IsValid)
            {
                throw new InvalidOperationException(searchResponse.DebugInformation);
            }

            return searchResponse;
        }

        private async Task<ISearchResponse<dynamic>> PageElasticTaxonSumAggregationAsync(
            string indexName,
            ICollection<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> query,
            ICollection<Func<QueryContainerDescriptor<object>, QueryContainer>> excludeQuery,
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
            );
 
            if (!searchResponse.IsValid)
            {
                throw new InvalidOperationException(searchResponse.DebugInformation);
            }

            return searchResponse;
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
            ILogger<ProcessedTaxonRepository> logger) : base(true, elasticClientManager, processedConfigurationCache, elasticConfiguration, logger)
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
                    b.Value.Item3)
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

            var filterWithoutTaxaFilter = filter.Clone();
            filterWithoutTaxaFilter.Taxa = null;
            var (queryWithoutTaxaFilter, excludeQueryWithoutTaxaFilter) = GetCoreQueries(filterWithoutTaxaFilter);
            observationCountByTaxonId = await GetElasticTaxonSumAggregationByTaxonIdAsync(
                indexName,
                queryWithoutTaxaFilter,
                excludeQueryWithoutTaxaFilter);
            var treeNodeSumByTaxonId = new Dictionary<int, TaxonAggregationTreeNodeSum>();
            var tree = _taxonManager.TaxonTree;
            foreach (var item in tree.TreeNodeById.Values)
            {
                var taxonProvinceAgg = observationCountByTaxonId.GetValueOrDefault(item.TaxonId);
                var sumNode = new TaxonAggregationTreeNodeSum
                {
                    TopologicalIndex = tree.ReverseTopologicalSortById[item.TaxonId],
                    TreeNode = item,
                    ObservationCount = taxonProvinceAgg == null ? 0 : taxonProvinceAgg.ObservationCount,
                    SumObservationCount = taxonProvinceAgg == null ? 0 : taxonProvinceAgg.ObservationCount,
                    ProvinceCount = taxonProvinceAgg == null ? 0 : taxonProvinceAgg.ProvinceIds.Count,
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

            var orderedTreeNodeSum = treeNodeSumByTaxonId.Values.OrderBy(m => m.TopologicalIndex).ToList();
            foreach (var sumNode in orderedTreeNodeSum)
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
            foreach (var node in orderedTreeNodeSum)
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

            if (!searchResponse.IsValid) throw new InvalidOperationException(searchResponse.DebugInformation);

            return searchResponse.Aggregations
                .Terms("taxon_group")
                .Buckets
                .Select(b => new TaxonAggregationItem { TaxonId = int.Parse(b.Key), ObservationCount = (int)(b.DocCount ?? 0) });
        }
    }
}
