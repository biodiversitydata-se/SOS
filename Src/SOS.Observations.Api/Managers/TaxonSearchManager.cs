using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Exceptions;
using SOS.Lib.Extensions;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Gis;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Models.Search.Result;
using SOS.Observations.Api.Managers.Interfaces;
using SOS.Observations.Api.Repositories.Interfaces;
using SOS.Shared.Api.Dtos;
using SOS.Shared.Api.Extensions.Dto;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SOS.Observations.Api.Managers
{
    /// <summary>
    ///     Observation manager class
    /// </summary>
    public class TaxonSearchManager : ITaxonSearchManager
    {
        private readonly IProcessedTaxonRepository _processedTaxonRepository;
        private readonly IFilterManager _filterManager;
        private readonly IClassCache<ConcurrentDictionary<int, TaxonSumAggregationItem>> _taxonSumAggregationCache;
        private readonly ITaxonManager _taxonManager;
        private readonly ILogger<TaxonSearchManager> _logger;
        private static readonly SemaphoreSlim _taxonSumAggregationSemaphore = new SemaphoreSlim(1, 1);

        private async Task<ConcurrentDictionary<int, TaxonSumAggregationItem>> GetCachedTaxonSumAggregation(int? userId)
        {
            var taxonAggregation = _taxonSumAggregationCache.Get();
            if (taxonAggregation == null)
            {
                await _taxonSumAggregationSemaphore.WaitAsync();
                try
                {
                    taxonAggregation = _taxonSumAggregationCache.Get();
                    if (taxonAggregation == null)
                    {
                        _logger.LogInformation("Start create taxonSumAggregationCache");
                        var searchFilter = new SearchFilterInternal(userId ?? 0, ProtectionFilter.Public);
                        searchFilter.PositiveSightings = true;
                        searchFilter.NotPresentFilter = SightingNotPresentFilter.DontIncludeNotPresent;
                        searchFilter.DeterminationFilter = SightingDeterminationFilter.NotUnsureDetermination;

                        await _filterManager.PrepareFilterAsync(null, null, searchFilter);
                        Stopwatch sp = Stopwatch.StartNew();
                        taxonAggregation = await _processedTaxonRepository.GetTaxonSumAggregationAsync(searchFilter);
                        sp.Stop();
                        _taxonSumAggregationCache.Set(taxonAggregation);
                        _logger.LogInformation($"Finish create taxonSumAggregationCache. Elapsed time: {sp.Elapsed.Seconds} seconds");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "GetCachedTaxonSumAggregation() error");
                }
                finally
                {
                    _taxonSumAggregationSemaphore.Release();
                }
            }

            return taxonAggregation;
        }

        private static IEnumerable<TaxonSumAggregationItem> OrderSumTaxonAggregation(IEnumerable<TaxonSumAggregationItem> items, string sortBy, SearchSortOrder sortOrder)
        {
            IEnumerable<TaxonSumAggregationItem> orderedResult = null;
            string sort = sortBy == null ? "" : sortBy.ToLower();
            if (sort == nameof(TaxonSumAggregationItem.SumObservationCount).ToLower() && sortOrder == SearchSortOrder.Desc)
            {
                orderedResult = items
                        .OrderByDescending(m => m.SumObservationCount)
                        .ThenBy(m => m.TaxonId);
            }
            else if (sort == nameof(TaxonSumAggregationItem.SumObservationCount).ToLower() && sortOrder == SearchSortOrder.Asc)
            {
                orderedResult = items
                        .OrderBy(m => m.SumObservationCount)
                        .ThenBy(m => m.TaxonId);
            }
            else if (sort == nameof(TaxonSumAggregationItem.ObservationCount).ToLower() && sortOrder == SearchSortOrder.Desc)
            {
                orderedResult = items
                        .OrderByDescending(m => m.ObservationCount)
                        .ThenBy(m => m.TaxonId);
            }
            else if (sort == nameof(TaxonSumAggregationItem.ObservationCount).ToLower() && sortOrder == SearchSortOrder.Asc)
            {
                orderedResult = items
                        .OrderBy(m => m.ObservationCount)
                        .ThenBy(m => m.TaxonId);
            }
            else if (sort == nameof(TaxonSumAggregationItem.SumProvinceCount).ToLower() && sortOrder == SearchSortOrder.Desc)
            {
                orderedResult = items
                        .OrderByDescending(m => m.SumProvinceCount)
                        .ThenBy(m => m.TaxonId);
            }
            else if (sort == nameof(TaxonSumAggregationItem.SumProvinceCount).ToLower() && sortOrder == SearchSortOrder.Asc)
            {
                orderedResult = items
                        .OrderBy(m => m.SumProvinceCount)
                        .ThenBy(m => m.TaxonId);
            }
            else if (sort == nameof(TaxonSumAggregationItem.ProvinceCount).ToLower() && sortOrder == SearchSortOrder.Desc)
            {
                orderedResult = items
                        .OrderByDescending(m => m.ProvinceCount)
                        .ThenBy(m => m.TaxonId);
            }
            else if (sort == nameof(TaxonSumAggregationItem.ProvinceCount).ToLower() && sortOrder == SearchSortOrder.Asc)
            {
                orderedResult = items
                        .OrderBy(m => m.ProvinceCount)
                        .ThenBy(m => m.TaxonId);
            }
            else
            {
                orderedResult = items
                        .OrderByDescending(m => m.SumObservationCount)
                        .ThenBy(m => m.TaxonId);
            }

            return orderedResult;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="processedTaxonRepository"></param>
        /// <param name="filterManager"></param>
        /// <param name="taxonSumAggregationCache"></param>
        /// <param name="taxonManager"></param>
        /// <param name="logger"></param>
        public TaxonSearchManager(
            IProcessedTaxonRepository processedTaxonRepository,
            IFilterManager filterManager,
            IClassCache<ConcurrentDictionary<int, TaxonSumAggregationItem>> taxonSumAggregationCache,
            ITaxonManager taxonManager,
            ILogger<TaxonSearchManager> logger)
        {
            _processedTaxonRepository = processedTaxonRepository ??
                                              throw new ArgumentNullException(nameof(processedTaxonRepository));
            _filterManager = filterManager ?? throw new ArgumentNullException(nameof(filterManager));
            _taxonSumAggregationCache = taxonSumAggregationCache ?? throw new ArgumentNullException(nameof(taxonSumAggregationCache));
            _taxonManager = taxonManager;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Make sure we are working with live data
            _processedTaxonRepository.LiveMode = true;
        }

        /// <inheritdoc />
        public async Task<Result<IEnumerable<GeoGridTileTaxaCell>>> GetCompleteGeoTileTaxaAggregationAsync(
            int? roleId,
            string authorizationApplicationIdentifier,
            SearchFilter filter,
            int zoom)
        {
            try
            {
                await _filterManager.PrepareFilterAsync(roleId, authorizationApplicationIdentifier, filter);
                return await _processedTaxonRepository.GetCompleteGeoTileTaxaAggregationAsync(filter, zoom);
            }
            catch (TimeoutException e)
            {
                _logger.LogError(e, "Get complete geo tile taxa aggregation timeout");
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get complete geo tile taxa aggregation.");
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<Result<GeoGridTileTaxonPageResult>> GetPageGeoTileTaxaAggregationAsync(
            int? roleId,
            string authorizationApplicationIdentifier,
            SearchFilter filter,
            int zoom,
            string geoTilePage,
            int? taxonIdPage)
        {
            try
            {
                await _filterManager.PrepareFilterAsync(roleId, authorizationApplicationIdentifier, filter);
                return await _processedTaxonRepository.GetPageGeoTileTaxaAggregationAsync(filter, zoom, geoTilePage, taxonIdPage);
            }
            catch (TimeoutException e)
            {
                _logger.LogError(e, "Get page geo tile taxa aggregation timeout");
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get page geo tile taxa aggregation");
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TaxonSumAggregationItem>> GetCachedTaxonSumAggregationItemsAsync(
            IEnumerable<int> taxonIds)
        {
            _logger.LogDebug("Start GetCachedTaxonSumAggregationItemsAsync()");
            var taxonIdsSet = taxonIds.ToHashSet();
            var cachedTaxonSumAggregation = await GetCachedTaxonSumAggregation(null);
            var taxonAggregations = cachedTaxonSumAggregation?
                .Values
                .Where(m => taxonIdsSet.Contains(m.TaxonId));

            _logger.LogDebug("Finish GetCachedTaxonSumAggregationItemsAsync()");
            return taxonAggregations;
        }

        /// <inheritdoc />
        public async Task<Result<PagedResult<TaxonSumAggregationItem>>> GetTaxonSumAggregationAsync(
            int userId,
            TaxonFilter taxonFilter,
            int? skip,
            int? take,
            string sortBy,
            SearchSortOrder sortOrder)
        {
            try
            {
                var taxonIds = await _filterManager.GetTaxonIdsFromFilterAsync(taxonFilter);
                var cachedTaxonSumAggregation = await GetCachedTaxonSumAggregation(userId);
                ConcurrentDictionary<int, TaxonSumAggregationItem> aggregationByTaxonId = null;
                if (taxonIds == null)
                {
                    aggregationByTaxonId = cachedTaxonSumAggregation;
                }
                else
                {
                    aggregationByTaxonId = cachedTaxonSumAggregation
                        .Where(m => taxonIds.Contains(m.Key))
                        .ToConcurrentDictionary(m => m.Key, m => m.Value);
                }

                // Update skip and take
                if (skip == null)
                {
                    skip = 0;
                }
                if (skip > aggregationByTaxonId.Count)
                {
                    skip = aggregationByTaxonId.Count;
                }
                if (take == null)
                {
                    take = aggregationByTaxonId.Count - skip;
                }
                else
                {
                    take = Math.Min(aggregationByTaxonId.Count - skip.Value, take.Value);
                }

                // Sort result.
                IEnumerable<TaxonSumAggregationItem> orderedResult = OrderSumTaxonAggregation(aggregationByTaxonId.Values, sortBy, sortOrder);

                var result = orderedResult
                    .Skip(skip.Value)
                    .Take(take.Value)
                    .ToList();

                var pagedResult = new PagedResult<TaxonSumAggregationItem>
                {
                    Records = result,
                    Skip = skip.Value,
                    Take = take.Value,
                    TotalCount = aggregationByTaxonId.Count
                };

                return Result.Success(pagedResult);
            }
            catch (TimeoutException e)
            {
                _logger.LogError(e, "Get taxon aggregation timeout");
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get taxon aggregation");
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<Result<PagedResult<TaxonAggregationItem>>> GetTaxonAggregationAsync(
            int? roleId,
            string authorizationApplicationIdentifier,
            SearchFilter filter,
            int? skip,
            int? take,
            bool sumUnderlyingTaxa = false)
        {
            try
            {
                await _filterManager.PrepareFilterAsync(roleId, authorizationApplicationIdentifier, filter);
                var result = await _processedTaxonRepository.GetTaxonAggregationAsync(filter,
                    skip,
                    take,
                    sumUnderlyingTaxa);

                return result;
            }
            catch (TimeoutException e)
            {
                _logger.LogError(e, "Get taxon aggregation timeout");
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get taxon aggregation");
                throw;
            }
        }

        public async Task<Result<List<int>>> GetObservedTaxaAsync(
            int? roleId,
            string authorizationApplicationIdentifier,
            SearchFilter filter)
        {
            try
            {
                await _filterManager.PrepareFilterAsync(roleId, authorizationApplicationIdentifier, filter);
                var result = await _processedTaxonRepository.GetObservedTaxaAsync(filter);

                return result;
            }
            catch (TimeoutException e)
            {
                _logger.LogError(e, "Get observed taxa timeout");
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get observed taxa");
                throw;
            }
        }

        public async Task<Dictionary<int, TaxonAreaAggregation>> GetTaxonAreaAggregationAsync(
            int? roleId,
            string authorizationApplicationIdentifier,
            SearchFilter filter,
            AreaTypeAggregate? areaType)
        {
            try
            {
                await _filterManager.PrepareFilterAsync(roleId, authorizationApplicationIdentifier, filter);                
                var result = await _processedTaxonRepository.GetTaxonAreaAggregationAsync(filter, areaType);

                return result;
            }
            catch (TimeoutException e)
            {
                _logger.LogError(e, "Get taxon area aggregation timeout");
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get taxon area aggregation");
                throw;
            }
        }

        public async Task<Dictionary<int, TaxonAreaAggregation>> CreateTaxonAreaSumAsync(Dictionary<int, TaxonAreaAggregation> taxonAreaAggByTaxonId)
        {                        
            var treeNodeSumByTaxonId = new Dictionary<int, TaxonAreaTreeNodeSum>();
            var taxonTree = await _taxonManager.GetTaxonTreeAsync();
            foreach (var item in taxonTree.TreeNodeById.Values)
            {
                var taxonAreaAgg = taxonAreaAggByTaxonId.GetValueOrDefault(item.TaxonId);
                var sumNode = new TaxonAreaTreeNodeSum
                {
                    TopologicalIndex = taxonTree.ReverseTopologicalSortById[item.TaxonId],
                    TreeNode = item,
                    ObservationCount = taxonAreaAgg?.ObservationCount ?? 0,
                    SumObservationCount = taxonAreaAgg?.ObservationCount ?? 0,
                    AreaCount = taxonAreaAgg?.ObservationCountByAreaFeatureId?.Count ?? 0,
                    DependentFeatureIds = new HashSet<string>() { },
                    DependentTaxonIds = new HashSet<int>() { item.TaxonId }
                };
                if (taxonAreaAgg != null)
                {
                    sumNode.DependentFeatureIds.UnionWith(taxonAreaAgg?.ObservationCountByAreaFeatureId?.Keys);
                    foreach (var pair in taxonAreaAgg?.ObservationCountByAreaFeatureId)
                    {
                        sumNode.ObservationCountByFeatureId.Add(pair.Key, pair.Value);
                        sumNode.SumObservationCountByFeatureId.Add(pair.Key, pair.Value);
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
                        parentSumNode.DependentFeatureIds.UnionWith(sumNode.DependentFeatureIds);
                        foreach (var taxonId in newDependentTaxonIds)
                        {
                            parentSumNode.SumObservationCount += treeNodeSumByTaxonId[taxonId].ObservationCount;

                            foreach (var pair in treeNodeSumByTaxonId[taxonId].ObservationCountByFeatureId)
                            {
                                if (!parentSumNode.SumObservationCountByFeatureId.TryAdd(pair.Key, pair.Value))
                                {
                                    parentSumNode.SumObservationCountByFeatureId[pair.Key] += pair.Value;
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
                            secondaryParentSumNode.DependentFeatureIds.UnionWith(sumNode.DependentFeatureIds);
                            foreach (var taxonId in newDependentTaxonIds)
                            {
                                secondaryParentSumNode.SumObservationCount += treeNodeSumByTaxonId[taxonId].ObservationCount;
                                foreach (var pair in treeNodeSumByTaxonId[taxonId].ObservationCountByFeatureId)
                                {
                                    if (!secondaryParentSumNode.SumObservationCountByFeatureId.TryAdd(pair.Key, pair.Value))
                                    {
                                        secondaryParentSumNode.SumObservationCountByFeatureId[pair.Key] += pair.Value;
                                    }
                                }
                            }
                        }
                    }
                }
            }         

            var result = new Dictionary<int, TaxonAreaAggregation>();
            foreach (var node in treeNodeSumByTaxonId.Values)
            {
                var agg = new TaxonAreaAggregation()
                {                    
                    ObservationCount = node.ObservationCount,
                    SumObservationCount = node.SumObservationCount,                    
                    ObservationCountByAreaFeatureId = node.ObservationCountByFeatureId,
                    SumObservationCountByAreaFeatureId = node.SumObservationCountByFeatureId
                };

                result.TryAdd(node.TreeNode.TaxonId, agg);
            }

            return result;
        }       
       
        public async Task<Dictionary<int, TaxonAreaAggregation>> CreateTaxonAreaSumUsingLowMemoryAsync(
            Dictionary<int, TaxonAreaAggregation> taxonAreaAggByTaxonId)
        {
            var taxonTree = await _taxonManager.GetTaxonTreeAsync();
            var nodes = taxonTree.TreeNodeById;
            var reverseTopo = taxonTree.ReverseTopologicalSortById;

            // 1) Initialize nodes
            var nodeSums = new Dictionary<int, TaxonAreaTreeNodeSum>(nodes.Count);
            foreach (var node in nodes.Values)
            {
                var agg = taxonAreaAggByTaxonId.GetValueOrDefault(node.TaxonId);
                var obsByFeature = agg?.ObservationCountByAreaFeatureId ?? new();

                nodeSums[node.TaxonId] = new TaxonAreaTreeNodeSum
                {
                    TreeNode = node,
                    TopologicalIndex = reverseTopo[node.TaxonId],
                    ObservationCount = agg?.ObservationCount ?? 0,
                    SumObservationCount = agg?.ObservationCount ?? 0,
                    ObservationCountByFeatureId = new Dictionary<string, int>(obsByFeature),
                    SumObservationCountByFeatureId = new Dictionary<string, int>(obsByFeature)
                };
            }

            // 2) Closure (in order to avoid duplicates)
            var closure = nodes.Values.ToDictionary(n => n.TaxonId, _ => new HashSet<int>());

            // 3) Iterate nodes in topological order (children before parents)
            foreach (var node in nodeSums.Values.OrderBy(n => n.TopologicalIndex))
            {
                var childId = node.TreeNode.TaxonId;

                void AddToParent(int? parentId)
                {
                    if (parentId == null) return;
                    if (!nodeSums.TryGetValue(parentId.Value, out var parent)) return;
                    var parentClosure = closure[parentId.Value];
                    var childClosure = closure[childId];

                    // protect against cycles and duplicate contributions
                    if (childId == parentId || parentClosure.Contains(childId)) return;

                    // find new descendants (that are not already in the parent)
                    var newOnes = new HashSet<int>(childClosure);
                    newOnes.Add(childId);
                    newOnes.ExceptWith(parentClosure);

                    if (newOnes.Count == 0)
                        return;

                    // sum observations for new descendants
                    foreach (var descId in newOnes)
                    {
                        var desc = nodeSums[descId];
                        parent.SumObservationCount += desc.ObservationCount;

                        foreach (var kv in desc.ObservationCountByFeatureId)
                        {
                            if (!parent.SumObservationCountByFeatureId.TryAdd(kv.Key, kv.Value))
                                parent.SumObservationCountByFeatureId[kv.Key] += kv.Value;
                        }

                        parentClosure.Add(descId);
                    }
                }

                AddToParent(node.TreeNode.Parent?.TaxonId);
                if (node.TreeNode.SecondaryParents != null)
                {
                    foreach (var secParent in node.TreeNode.SecondaryParents)
                        AddToParent(secParent.TaxonId);
                }
            }

            // 4) Build result
            return nodeSums.ToDictionary(
                kv => kv.Key,
                kv => new TaxonAreaAggregation
                {
                    ObservationCount = kv.Value.ObservationCount,
                    SumObservationCount = kv.Value.SumObservationCount,                    
                    ObservationCountByAreaFeatureId = kv.Value.ObservationCountByFeatureId,
                    SumObservationCountByAreaFeatureId = kv.Value.SumObservationCountByFeatureId
                });
        }

        public async Task CreateTaxonAreaSumWithLowMemoryAsync(
            Dictionary<int, TaxonAreaAggregation> taxonAreaAggByTaxonId,
            bool aggregateArea,
            bool pruneTaxaTree = false)
        {
            var taxonTree = await _taxonManager.GetTaxonTreeAsync();
            var nodes = taxonTree.TreeNodeById;
            var reverseTopo = taxonTree.ReverseTopologicalSortById;

            // 1) Initialize dictionary so that all nodes exist and initialize sum fields
            foreach (var node in nodes.Values)
            {
                if (!taxonAreaAggByTaxonId.TryGetValue(node.TaxonId, out var existing))
                {
                    existing = new TaxonAreaAggregation();
                    taxonAreaAggByTaxonId[node.TaxonId] = existing;
                }

                existing.SumObservationCount = existing.ObservationCount;

                if (aggregateArea)
                {
                    existing.SumObservationCountByAreaFeatureId =
                        new Dictionary<string, int>(existing.ObservationCountByAreaFeatureId ?? new());
                }                
            }

            // 2) Closure (in order to avoid duplicates)
            var closure = nodes.Values.ToDictionary(n => n.TaxonId, _ => new HashSet<int>());

            // 3) Iterate nodes in topological order (children before parents)
            foreach (var node in nodes.Values.OrderBy(n => reverseTopo[n.TaxonId]))
            {
                var childId = node.TaxonId;

                // Local helper to sum child -> parent but with/without area depending on aggregateByArea
                void AddToParent(int? parentId)
                {
                    if (parentId == null) return;
                    if (!nodes.TryGetValue(parentId.Value, out var parentNode)) return;
                    if (!taxonAreaAggByTaxonId.TryGetValue(parentId.Value, out var parentAgg)) return;

                    var parentClosure = closure[parentId.Value];
                    var childClosure = closure[childId];

                    // protect against cycles and duplicate contributions
                    if (childId == parentId || parentClosure.Contains(childId))
                        return;

                    // find new descendants (that are not already in the parent)
                    var newOnes = new HashSet<int>(childClosure);
                    newOnes.Add(childId);
                    newOnes.ExceptWith(parentClosure);

                    if (newOnes.Count == 0)
                        return;

                    // sum observations (and possibly area dictionaries) for each new descendant
                    foreach (var descId in newOnes)
                    {
                        if (!taxonAreaAggByTaxonId.TryGetValue(descId, out var descAgg))
                            continue;

                        // add area descendantens
                        parentAgg.SumObservationCount += descAgg.ObservationCount;

                        if (aggregateArea)
                        {
                            var descByFeature = descAgg.ObservationCountByAreaFeatureId;
                            if (descByFeature != null)
                            {
                                if (parentAgg.SumObservationCountByAreaFeatureId == null)
                                    parentAgg.SumObservationCountByAreaFeatureId = new Dictionary<string, int>();

                                foreach (var kv in descByFeature)
                                {
                                    if (!parentAgg.SumObservationCountByAreaFeatureId.TryAdd(kv.Key, kv.Value))
                                        parentAgg.SumObservationCountByAreaFeatureId[kv.Key] += kv.Value;
                                }
                            }
                        }

                        // mark this descendant as being included in the parent
                        parentClosure.Add(descId);
                    }
                }

                // main parent
                AddToParent(node.Parent?.TaxonId);

                // secondary parents
                if (node.SecondaryParents != null)
                {
                    foreach (var secParent in node.SecondaryParents)
                        AddToParent(secParent.TaxonId);
                }
            }

            // 4) Prune (remove nodes without observations)
            if (pruneTaxaTree)
            {
                // We run backwards to ensure that we don't remove parents too early
                foreach (var node in nodes.Values.OrderByDescending(n => reverseTopo[n.TaxonId]))
                {
                    if (!taxonAreaAggByTaxonId.TryGetValue(node.TaxonId, out var agg))
                        continue;

                    bool hasAnyCount =
                        (agg.SumObservationCount > 0) ||
                        (agg.ObservationCount > 0);

                    if (!hasAnyCount)
                    {
                        taxonAreaAggByTaxonId.Remove(node.TaxonId);
                    }
                }
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TaxonAggregationItemDto>> GetTaxonExistsIndicationAsync(
            int? roleId,
            string authorizationApplicationIdentifier,
            SearchFilter filter)
        {
            try
            {
                await _filterManager.PrepareFilterAsync(roleId, authorizationApplicationIdentifier, filter, "Sighting", 0, filter?.Location?.Geometries?.UsePointAccuracy, filter?.Location?.Geometries?.UseDisturbanceRadius);

                if (filter?.Taxa?.Ids?.Count() > 10000)
                {
                    throw new TaxonValidationException("Your filter exceeds 10000 taxon id's");
                }

                var result = await _processedTaxonRepository.GetTaxonExistsIndicationAsync(filter);
                return result?.ToTaxonAggregationItemDtos();
            }
            catch (TimeoutException e)
            {
                _logger.LogError(e, "Get taxon exists indication timeout");
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get taxon exists indication");
                throw;
            }
        }
    }
}