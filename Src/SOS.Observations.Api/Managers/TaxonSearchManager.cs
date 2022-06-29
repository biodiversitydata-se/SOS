﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Exceptions;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Gis;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Models.Search.Result;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Observations.Api.Dtos;
using SOS.Observations.Api.Extensions;
using ITaxonSearchManager = SOS.Observations.Api.Managers.Interfaces.ITaxonSearchManager;

namespace SOS.Observations.Api.Managers
{
    /// <summary>
    ///     Observation manager class
    /// </summary>
    public class TaxonSearchManager : ITaxonSearchManager
    {
        private readonly IProcessedTaxonRepository _processedTaxonRepository;
        private readonly IFilterManager _filterManager;
        private readonly IClassCache<Dictionary<int, TaxonSumAggregationItem>> _taxonSumAggregationCache;
        private readonly ILogger<TaxonSearchManager> _logger;

        private readonly SemaphoreSlim _taxonSumAggregationSemaphore = new SemaphoreSlim(1, 1);

        private async Task<Dictionary<int, TaxonSumAggregationItem>> GetCachedTaxonSumAggregation()
        {
            var taxonAggregation = _taxonSumAggregationCache.Get();
            if (taxonAggregation == null)
            {
                try
                {
                    await _taxonSumAggregationSemaphore.WaitAsync();
                    taxonAggregation = _taxonSumAggregationCache.Get();
                    if (taxonAggregation == null)
                    {
                        _logger.LogInformation("Start create taxonSumAggregationCache");
                        var searchFilter = new SearchFilter();
                        searchFilter.PositiveSightings = true;
                        _filterManager.PrepareFilterAsync(null, null, searchFilter).Wait();
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
        /// <param name="processedObservationRepository"></param>
        /// <param name="filterManager"></param>
        /// <param name="taxonSumAggregationCache"></param>
        /// <param name="logger"></param>
        public TaxonSearchManager(
            IProcessedTaxonRepository processedObservationRepository,
            IFilterManager filterManager,
            IClassCache<Dictionary<int, TaxonSumAggregationItem>> taxonSumAggregationCache,
            ILogger<TaxonSearchManager> logger)
        {
            _processedTaxonRepository = processedObservationRepository ??
                                              throw new ArgumentNullException(nameof(processedObservationRepository));
            _filterManager = filterManager ?? throw new ArgumentNullException(nameof(filterManager));     
            _taxonSumAggregationCache = taxonSumAggregationCache ?? throw new ArgumentNullException(nameof(taxonSumAggregationCache));

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
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to aggregate to geogrids.");
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
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to aggregate to geogrids.");
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TaxonSumAggregationItem>> GetCachedTaxonSumAggregationItemsAsync(IEnumerable<int> taxonIds)
        {
            _logger.LogDebug("Start GetCachedTaxonSumAggregationItemsAsync()");
            var taxonIdsSet = taxonIds.ToHashSet();
            var cachedTaxonSumAggregation = await GetCachedTaxonSumAggregation();
            var taxonAggregations = cachedTaxonSumAggregation?
                .Values
                .Where(m => taxonIdsSet.Contains(m.TaxonId));

            _logger.LogDebug("Finish GetCachedTaxonSumAggregationItemsAsync()");
            return taxonAggregations;
        }

        /// <inheritdoc />
        public async Task<Result<PagedResult<TaxonSumAggregationItem>>> GetTaxonSumAggregationAsync(
            TaxonFilter taxonFilter,
            int? skip,
            int? take,
            string sortBy,
            SearchSortOrder sortOrder)
        {
            try
            {
                var taxonIds = _filterManager.GetTaxonIdsFromFilter(taxonFilter);
                var cachedTaxonSumAggregation = await GetCachedTaxonSumAggregation();
                Dictionary<int, TaxonSumAggregationItem> aggregationByTaxonId = null;
                if (taxonIds == null)
                {
                    aggregationByTaxonId = cachedTaxonSumAggregation;
                }
                else
                {
                    aggregationByTaxonId = cachedTaxonSumAggregation
                        .Where(m => taxonIds.Contains(m.Key))
                        .ToDictionary(m => m.Key, m => m.Value);
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
                return await _processedTaxonRepository.GetTaxonAggregationAsync(filter, 
                    skip, 
                    take, 
                    sumUnderlyingTaxa);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get taxon aggregation");
                throw;
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
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get taxon exists indication");
                throw;
            }
        }
    }
}