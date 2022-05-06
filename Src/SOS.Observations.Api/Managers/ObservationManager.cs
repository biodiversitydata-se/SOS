using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Hangfire;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Exceptions;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Jobs.Import;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Cache;
using SOS.Lib.Models.Gis;
using SOS.Lib.Models.Log;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Observations.Api.Dtos;
using SOS.Observations.Api.Extensions;
using SOS.Observations.Api.Managers.Interfaces;

namespace SOS.Observations.Api.Managers
{
    /// <summary>
    ///     Observation manager class
    /// </summary>
    public class ObservationManager : IObservationManager
    {
        private readonly IProcessedObservationRepository _processedObservationRepository;
        private readonly IProtectedLogRepository _protectedLogRepository;
        private readonly IFilterManager _filterManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IVocabularyValueResolver _vocabularyValueResolver;
        private readonly ITaxonObservationCountCache _taxonObservationCountCache;
        private readonly IArtportalenApiManager _artportalenApiManager;
        private readonly IClassCache<Dictionary<int, TaxonSumAggregationItem>> _taxonSumAggregationCache;
        private readonly ILogger<ObservationManager> _logger;        

        private void PostProcessObservations(bool protectedObservations, IEnumerable<dynamic> processedObservations, string cultureCode)
        {
            if (!processedObservations?.Any() ?? true)
            {
                return;
            }

            try
            {
                var occurenceIds = new HashSet<string>();
                var observations = processedObservations.Cast<IDictionary<string, object>>().ToList();
                LocalDateTimeConverterHelper.ConvertToLocalTime(observations);
                
                // Resolve vocabulary values.
                _vocabularyValueResolver.ResolveVocabularyMappedValues(observations, cultureCode);
               
                foreach (var obs in observations)
                {
                    if (protectedObservations && obs.TryGetValue(nameof(Observation.Occurrence).ToLower(),
                        out var occurrenceObject))
                    {
                        if (occurrenceObject is IDictionary<string, object> occurrenceDictionary && occurrenceDictionary.TryGetValue("occurrenceId", out var occurenceId))
                        {
                            occurenceIds.Add(occurenceId as string);
                        }
                    }
                }

                // Log protected observations
                if (protectedObservations)
                {
                    var user =  _httpContextAccessor.HttpContext?.User;

                    var protectedLog = new ProtectedLog
                    {
                        ApplicationIdentifier = user.Claims.Where(c =>
                            c.Type.Equals("client_id",
                                StringComparison.CurrentCultureIgnoreCase)).Select(c => c.Value).FirstOrDefault(),
                        Ip = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString(),
                        IssueDate = DateTime.Now,
                        User = user.Claims.Where(c =>
                            c.Type.Equals("name",
                                StringComparison.CurrentCultureIgnoreCase)).Select(c => c.Value).FirstOrDefault(),
                        UserId = user.Claims.Where(c =>
                            c.Type.Equals("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",
                                StringComparison.CurrentCultureIgnoreCase)).Select(c => c.Value).FirstOrDefault(),
                        OccurenceIds = occurenceIds
                    };
                    _protectedLogRepository.AddAsync(protectedLog);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error in postprocessing observations");
                throw;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="processedObservationRepository"></param>
        /// <param name="protectedLogRepository"></param>
        /// <param name="vocabularyValueResolver"></param>
        /// <param name="filterManager"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="taxonObservationCountCache"></param>
        /// <param name="artportalenApiManager"></param>
        /// <param name="taxonSumAggregationCache"></param>
        /// <param name="logger"></param>
        public ObservationManager(
            IProcessedObservationRepository processedObservationRepository,
            IProtectedLogRepository protectedLogRepository,
            IVocabularyValueResolver vocabularyValueResolver,
            IFilterManager filterManager,
            IHttpContextAccessor httpContextAccessor,
            ITaxonObservationCountCache taxonObservationCountCache,
            IArtportalenApiManager artportalenApiManager,
            IClassCache<Dictionary<int, TaxonSumAggregationItem>> taxonSumAggregationCache,
            ILogger<ObservationManager> logger)
        {
            _processedObservationRepository = processedObservationRepository ??
                                              throw new ArgumentNullException(nameof(processedObservationRepository));
            _protectedLogRepository =
                protectedLogRepository ?? throw new ArgumentNullException(nameof(protectedLogRepository));
            _vocabularyValueResolver = vocabularyValueResolver ?? throw new ArgumentNullException(nameof(vocabularyValueResolver));
            _filterManager = filterManager ?? throw new ArgumentNullException(nameof(filterManager));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _taxonObservationCountCache = taxonObservationCountCache ?? throw new ArgumentNullException(nameof(taxonObservationCountCache));            
            _taxonSumAggregationCache = taxonSumAggregationCache ?? throw new ArgumentNullException(nameof(taxonSumAggregationCache));

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Make sure we are working with live data
            _processedObservationRepository.LiveMode = true;
        }


        public int MaxNrElasticSearchAggregationBuckets => _processedObservationRepository.MaxNrElasticSearchAggregationBuckets;

        /// <inheritdoc />
        public async Task<PagedResult<dynamic>> GetChunkAsync(
            int? roleId,
            string authorizationApplicationIdentifier,
            SearchFilter filter, int skip, int take, string sortBy,
            SearchSortOrder sortOrder)
        {
            try
            {
                await _filterManager.PrepareFilter(roleId, authorizationApplicationIdentifier, filter);
                var processedObservations =
                    await _processedObservationRepository.GetChunkAsync(filter, skip, take, sortBy, sortOrder);
                PostProcessObservations(filter.ExtendedAuthorization.ProtectedObservations, processedObservations.Records, filter.FieldTranslationCultureCode);

                return processedObservations;
            }
            catch (AuthenticationRequiredException e)
            {
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get chunk of observations");
                return null;
            }
        }

        /// <inheritdoc />
        public async Task<ScrollResult<dynamic>> GetObservationsByScrollAsync(
            int? roleId,
            string authorizationApplicationIdentifier,
            SearchFilter filter,
            int take, 
            string sortBy,
            SearchSortOrder sortOrder,
            string scrollId)
        {
            try
            {
                await _filterManager.PrepareFilter(roleId, authorizationApplicationIdentifier, filter);
                var processedObservations =
                    await _processedObservationRepository.GetObservationsByScrollAsync(filter, take, sortBy, sortOrder, scrollId);
                PostProcessObservations(filter.ExtendedAuthorization.ProtectedObservations, processedObservations.Records, filter.FieldTranslationCultureCode);
                return processedObservations;
            }
            catch (AuthenticationRequiredException e)
            {
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get observations by scroll");
                return null;
            }
        }


        /// <inheritdoc />
        public async Task<PagedResult<dynamic>> GetAggregatedChunkAsync(
            int? roleId,
            string authorizationApplicationIdentifier, SearchFilter filter, AggregationType aggregationType, int skip, int take)
        {
            try
            {
                await _filterManager.PrepareFilter(roleId, authorizationApplicationIdentifier, filter);

                if(aggregationType.IsDateHistogram())
                    return await _processedObservationRepository.GetAggregatedHistogramChunkAsync(filter, aggregationType);

                if(aggregationType.IsSpeciesSightingsList())
                    return await _processedObservationRepository.GetAggregatedChunkAsync(filter, aggregationType, skip, take);

                return null;
            }
            catch (AuthenticationRequiredException e)
            {
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get aggregated chunk of observations");
                return null;
            }
        }


        /// <inheritdoc />
        public async Task<Result<GeoGridResult>> GetGeogridAggregationAsync(int? roleId, string authorizationApplicationIdentifier, SearchFilter filter, int precision)
        {
            try
            {
                await _filterManager.PrepareFilter(roleId, authorizationApplicationIdentifier, filter);
                return await _processedObservationRepository.GetGeogridAggregationAsync(filter, precision);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to aggregate to geogrids.");
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<Result<GeoGridTileResult>> GetGeogridTileAggregationAsync(int? roleId, string authorizationApplicationIdentifier, SearchFilter filter, int precision)
        {
            try
            {
                await _filterManager.PrepareFilter(roleId, authorizationApplicationIdentifier, filter);
                return await _processedObservationRepository.GetGeogridTileAggregationAsync(filter, precision);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to aggregate to geogrids.");
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<Result<GeoGridMetricResult>> GetMetricGridAggregationAsync(int? roleId, string authorizationApplicationIdentifier,
                SearchFilter filter, int gridCellSizeInMeters)
        {
            try
            {
                await _filterManager.PrepareFilter(roleId, authorizationApplicationIdentifier, filter);
                return await _processedObservationRepository.GetMetricGridAggregationAsync(filter, gridCellSizeInMeters);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to aggregate to metric tiles.");
                throw;
            }
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
                await _filterManager.PrepareFilter(roleId, authorizationApplicationIdentifier, filter);
                return await _processedObservationRepository.GetCompleteGeoTileTaxaAggregationAsync(filter, zoom);
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
                await _filterManager.PrepareFilter(roleId, authorizationApplicationIdentifier, filter);
                return await _processedObservationRepository.GetPageGeoTileTaxaAggregationAsync(filter, zoom, geoTilePage, taxonIdPage);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to aggregate to geogrids.");
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<DateTime?> GetLatestModifiedDateForProviderAsync(int providerId)
        {
            return await _processedObservationRepository.GetLatestModifiedDateForProviderAsync(providerId);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<LocationDto>> GetLocationsAsync(IEnumerable<string> locationIds)
        {
            var locations =  await _processedObservationRepository.GetLocationsAsync(locationIds);

            return locations?.Select(l => l.ToDto());
        }        

        /// <inheritdoc />
        public async Task<long> GetMatchCountAsync(int? roleId, string authorizationApplicationIdentifier, SearchFilterBase filter)
        {
            await _filterManager.PrepareFilter(roleId, authorizationApplicationIdentifier, filter);
            return await _processedObservationRepository.GetMatchCountAsync(filter);
        }

        private async Task<long> GetProvinceCountAsync(int? roleId, string authorizationApplicationIdentifier, SearchFilterBase filter)
        {
            await _filterManager.PrepareFilter(roleId, authorizationApplicationIdentifier, filter);
            return await _processedObservationRepository.GetProvinceCountAsync(filter);
        }

        public async Task<IEnumerable<TaxonObservationCountDto>> GetCachedCountAsync(SearchFilterBase filter, TaxonObservationCountSearch taxonObservationCountSearch)
        {
            var taxonObservationCountCacheKey = TaxonObservationCountCacheKey.Create(taxonObservationCountSearch);
            var countByTaxonId = new Dictionary<int, TaxonCount>();
            var notCachedTaxonIds = new HashSet<int>();
            foreach (var taxonId in taxonObservationCountSearch.TaxonIds)
            {
                taxonObservationCountCacheKey.TaxonId = taxonId;
                if (_taxonObservationCountCache.TryGetCount(taxonObservationCountCacheKey, out var count))
                {
                    countByTaxonId.Add(taxonId, count);
                }
                else
                {
                    notCachedTaxonIds.Add(taxonId);
                }
            }

            if (notCachedTaxonIds.Any())
            {
                foreach (var notCachedTaxonId in notCachedTaxonIds)
                {
                    filter.Taxa.Ids = new[] { notCachedTaxonId };
                    int observationCount = Convert.ToInt32(await GetMatchCountAsync(null, null, filter));                    
                    int provinceCount = Convert.ToInt32(await GetProvinceCountAsync(null, null, filter));
                    var taxonCount = new TaxonCount { ObservationCount = observationCount, ProvinceCount = provinceCount };
                    var cacheKey = TaxonObservationCountCacheKey.Create(taxonObservationCountSearch, notCachedTaxonId);
                    countByTaxonId.Add(notCachedTaxonId, taxonCount);
                    _taxonObservationCountCache.Add(cacheKey, taxonCount);
                }
            }

            var taxonCountDtos = countByTaxonId
                .Select(m => new TaxonObservationCountDto
                {
                    TaxonId = m.Key,
                    ObservationCount = m.Value.ObservationCount,
                    ProvinceCount = m.Value.ProvinceCount
                });

            return taxonCountDtos;
        }

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
                        _logger.LogDebug("Start create taxonSumAggregationCache");
                        var searchFilter = new SearchFilter();
                        searchFilter.PositiveSightings = true;
                        _filterManager.PrepareFilter(null, null, searchFilter).Wait();
                        Stopwatch sp = Stopwatch.StartNew();
                        taxonAggregation = await _processedObservationRepository.GetTaxonSumAggregationAsync(searchFilter);
                        sp.Stop();
                        _taxonSumAggregationCache.Set(taxonAggregation);
                        _logger.LogDebug($"Finish create taxonSumAggregationCache. Elapsed time: {sp.Elapsed.Seconds} seconds");
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
                var aggregationByTaxonId = cachedTaxonSumAggregation
                    .Where(m => taxonIds.Contains(m.Key))
                    .ToDictionary(m => m.Key, m => m.Value);

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
                await _filterManager.PrepareFilter(roleId, authorizationApplicationIdentifier, filter);                
                return await _processedObservationRepository.GetTaxonAggregationAsync(filter, 
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
                await _filterManager.PrepareFilter(roleId, authorizationApplicationIdentifier, filter, "Sighting", 0, filter?.Location?.Geometries?.UsePointAccuracy, filter?.Location?.Geometries?.UseDisturbanceRadius);

                if (filter?.Taxa?.Ids?.Count() > 10000)
                {
                    throw new TaxonValidationException("Your filter exceeds 10000 taxon id's");
                }

                var result = await _processedObservationRepository.GetTaxonExistsIndicationAsync(filter);
                return result?.ToTaxonAggregationItemDtos();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get taxon exists indication");
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<bool> SignalSearchInternalAsync(
            int? roleId,
            string authorizationApplicationIdentifier,
            SearchFilter filter,
            int areaBuffer,
            bool onlyAboveMyClearance)
        {
            try
            {
                await _filterManager.PrepareFilter(roleId, authorizationApplicationIdentifier, filter, "SightingIndication", areaBuffer, filter?.Location?.Geometries?.UsePointAccuracy, filter?.Location?.Geometries?.UseDisturbanceRadius);

                if (!filter.ExtendedAuthorization?.ExtendedAreas?.Any() ?? true)
                {
                    throw new AuthenticationRequiredException("User don't have the SightingIndication permission that is required");
                }

                var result = await _processedObservationRepository.SignalSearchInternalAsync(filter, onlyAboveMyClearance);
                return result;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Signal search failed");
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<string>> TryToGetOccurenceIdDuplicatesAsync(bool activeInstance, bool protectedIndex, int maxReturnedItems)
        {
            return await _processedObservationRepository.TryToGetOccurenceIdDuplicatesAsync(activeInstance,
                protectedIndex, maxReturnedItems);
        }

        /// <inheritdoc />
        public async Task<dynamic> GetObservationAsync(int? roleId, string authorizationApplicationIdentifier, string occurrenceId, OutputFieldSet outputFieldSet,
            string translationCultureCode, bool protectedObservations, bool includeInternalFields, bool ensureArtportalenUpdated = false)
        {
            if (ensureArtportalenUpdated && (occurrenceId?.Contains("artportalen", StringComparison.CurrentCultureIgnoreCase) ?? false))
            {
                var regex = new Regex(@"\d+$");
                var match = regex.Match(occurrenceId);
                if (int.TryParse(match.Value, out var sightingId))
                {
                    var jobId = BackgroundJob.Enqueue<IObservationsHarvestJob>(
                        job => job.RunHarvestArtportalenObservationsAsync(new[] { sightingId },
                        JobCancellationToken.Null));

                    using var connection = JobStorage.Current.GetConnection();
                    var stateData = connection.GetStateData(jobId);

                    while (stateData.Name.Equals("Enqueued", StringComparison.CurrentCultureIgnoreCase) ||
                           stateData.Name.Equals("Processing", StringComparison.CurrentCultureIgnoreCase))
                    {
                        Thread.Sleep(100);
                        stateData = connection.GetStateData(jobId);
                    }
                }
            }

            var filter = includeInternalFields ? new SearchFilterInternal() : new SearchFilter();

            filter.PopulateOutputFields(outputFieldSet);
            filter.ExtendedAuthorization.ProtectedObservations = protectedObservations;

            await _filterManager.PrepareFilter(roleId, authorizationApplicationIdentifier, filter, "Sighting", null, null, null, false);

            var processedObservation = await _processedObservationRepository.GetObservationAsync(occurrenceId, filter);

            PostProcessObservations(protectedObservations, processedObservation, translationCultureCode);

            return (processedObservation?.Count ?? 0) == 1 ? processedObservation[0] : null;
        }

        public async Task<dynamic> GetObservationFromArtportalenApiAsync(int? roleId,
            string authorizationApplicationIdentifier,
            string occurrenceId,
            OutputFieldSet outputFieldSet,
            string translationCultureCode,
            bool protectedObservations,
            bool includeInternalFields)
        {
            dynamic processedObservation;            
            var sighting = await _artportalenApiManager.GetObservationAsync(occurrenceId);
            processedObservation = sighting.ToDynamic();
            processedObservation = new List<dynamic>() { processedObservation };           
            PostProcessObservations(protectedObservations, processedObservation, translationCultureCode);
            return (processedObservation?.Count ?? 0) == 1 ? processedObservation[0] : null;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<YearMonthCountResultDto>> GetUserYearMonthCountAsync(SearchFilter filter)
        {
            try
            {
                // Make sure mandatory properties is set
                filter = filter ?? new SearchFilter();
                filter.ExtendedAuthorization = filter.ExtendedAuthorization ?? new ExtendedAuthorizationFilter();
                filter.ExtendedAuthorization.ReportedByMe = true;
                filter.DiffusionStatuses = new List<DiffusionStatus> { DiffusionStatus.NotDiffused };
                filter.ExtendedAuthorization.ProtectedObservations = false;
                await _filterManager.PrepareFilter(null, null, filter);

                if (filter.ExtendedAuthorization.UserId == 0)
                {
                    throw new AuthenticationRequiredException("You have to login in order to use this end point");
                }

                var result = await _processedObservationRepository.GetUserYearMonthCountAsync(filter);
                return result?.ToDtos();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get user year, month count");
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<YearMonthDayCountResultDto>> GetUserYearMonthDayCountAsync(SearchFilter filter, int skip, int take)
        {
            try
            {
                // Make sure mandatory properties is set
                filter = filter ?? new SearchFilter();
                filter.ExtendedAuthorization = filter.ExtendedAuthorization ?? new ExtendedAuthorizationFilter();
                filter.ExtendedAuthorization.ReportedByMe = true;
                filter.DiffusionStatuses = new List<DiffusionStatus> { DiffusionStatus.NotDiffused };
                filter.ExtendedAuthorization.ProtectedObservations = false;
                await _filterManager.PrepareFilter(null, null, filter);

                if (filter.ExtendedAuthorization.UserId == 0)
                {
                    throw new AuthenticationRequiredException("You have to login in order to use this end point");
                }
              
                var result = await _processedObservationRepository.GetUserYearMonthDayCountAsync(filter, skip, take);
                return result?.ToDtos();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get user year, month count");
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<bool> HasIndexOccurrenceIdDuplicatesAsync(bool protectedIndex)
        {
            return await _processedObservationRepository.HasIndexOccurrenceIdDuplicatesAsync(protectedIndex);
        }

        /// <inheritdoc />
        public async Task<long> IndexCountAsync(bool protectedIndex = false)
        {
            return await _processedObservationRepository.IndexCountAsync(protectedIndex);
        }
    }
}