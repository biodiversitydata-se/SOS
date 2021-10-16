using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Exceptions;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers.Interfaces;
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
                _vocabularyValueResolver.ResolveVocabularyMappedValues(observations, cultureCode, true);

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
                Console.WriteLine(e);
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
        /// <param name="logger"></param>
        public ObservationManager(
            IProcessedObservationRepository processedObservationRepository,
            IProtectedLogRepository protectedLogRepository,
            IVocabularyValueResolver vocabularyValueResolver,
            IFilterManager filterManager,
            IHttpContextAccessor httpContextAccessor,
            ITaxonObservationCountCache taxonObservationCountCache,
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

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public int MaxNrElasticSearchAggregationBuckets => _processedObservationRepository.MaxNrElasticSearchAggregationBuckets;

        /// <inheritdoc />
        public async Task<PagedResult<dynamic>> GetChunkAsync(
            string authorizationApplicationIdentifier,
            SearchFilter filter, int skip, int take, string sortBy,
            SearchSortOrder sortOrder)
        {
            try
            {
                await _filterManager.PrepareFilter(authorizationApplicationIdentifier, filter);
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
            string authorizationApplicationIdentifier,
            SearchFilter filter,
            int take, 
            string sortBy,
            SearchSortOrder sortOrder,
            string scrollId)
        {
            try
            {
                await _filterManager.PrepareFilter(authorizationApplicationIdentifier, filter);
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
        public async Task<PagedResult<dynamic>> GetAggregatedChunkAsync(string authorizationApplicationIdentifier, SearchFilter filter, AggregationType aggregationType, int skip, int take)
        {
            try
            {
                await _filterManager.PrepareFilter(authorizationApplicationIdentifier, filter);

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
        public async Task<Result<GeoGridResult>> GetGeogridAggregationAsync(string authorizationApplicationIdentifier, SearchFilter filter, int precision)
        {
            try
            {
                await _filterManager.PrepareFilter(authorizationApplicationIdentifier, filter);
                return await _processedObservationRepository.GetGeogridAggregationAsync(filter, precision);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to aggregate to geogrids.");
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<Result<GeoGridTileResult>> GetGeogridTileAggregationAsync(string authorizationApplicationIdentifier, SearchFilter filter, int precision)
        {
            try
            {
                await _filterManager.PrepareFilter(authorizationApplicationIdentifier, filter);
                return await _processedObservationRepository.GetGeogridTileAggregationAsync(filter, precision);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to aggregate to geogrids.");
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<Result<IEnumerable<GeoGridTileTaxaCell>>> GetCompleteGeoTileTaxaAggregationAsync(
            string authorizationApplicationIdentifier,
            SearchFilter filter, 
            int zoom)
        {
            try
            {
                await _filterManager.PrepareFilter(authorizationApplicationIdentifier, filter);
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
            string authorizationApplicationIdentifier,
            SearchFilter filter,
            int zoom,
            string geoTilePage,
            int? taxonIdPage)
        {
            try
            {
                await _filterManager.PrepareFilter(authorizationApplicationIdentifier, filter);
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
        public async Task<long> GetMatchCountAsync(string authorizationApplicationIdentifier, FilterBase filter)
        {
            await _filterManager.PrepareFilter(authorizationApplicationIdentifier, filter);
            return await _processedObservationRepository.GetMatchCountAsync(filter);
        }

        public async Task<IEnumerable<TaxonObservationCountDto>> GetCachedCountAsync(FilterBase filter, TaxonObservationCountSearch taxonObservationCountSearch)
        {
            TaxonObservationCountCacheKey taxonObservationCountCacheKey = TaxonObservationCountCacheKey.Create(taxonObservationCountSearch);
            Dictionary<int, int> countByTaxonId = new Dictionary<int, int>();
            HashSet<int> notCachedTaxonIds = new HashSet<int>();
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
                    int count = Convert.ToInt32(await GetMatchCountAsync(null, filter));
                    countByTaxonId.Add(notCachedTaxonId, count);
                    var cacheKey = TaxonObservationCountCacheKey.Create(taxonObservationCountSearch, notCachedTaxonId);
                    _taxonObservationCountCache.Add(cacheKey, count);
                }
            }

            var taxonCountDtos = countByTaxonId
                .Select(m => new TaxonObservationCountDto { Count = m.Value, TaxonId = m.Key });
            return taxonCountDtos;
        }

        /// <inheritdoc />
        public async Task<Result<PagedResult<TaxonAggregationItem>>> GetTaxonAggregationAsync(
            string authorizationApplicationIdentifier,
            SearchFilter filter,
            int? skip,
            int? take)
        {
            try
            {
                await _filterManager.PrepareFilter(authorizationApplicationIdentifier, filter);
                return await _processedObservationRepository.GetTaxonAggregationAsync(filter, skip, take);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get taxon aggregation");
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TaxonAggregationItemDto>> GetTaxonExistsIndicationAsync(
            string authorizationApplicationIdentifier,
            SearchFilter filter)
        {
            try
            {
                await _filterManager.PrepareFilter(authorizationApplicationIdentifier, filter, "Sighting", 0, filter?.Geometries?.UsePointAccuracy, filter?.Geometries?.UseDisturbanceRadius);

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
            string authorizationApplicationIdentifier,
            SearchFilter filter,
            int areaBuffer,
            bool onlyAboveMyClearance)
        {
            try
            {
                await _filterManager.PrepareFilter(authorizationApplicationIdentifier, filter, "SightingIndication", areaBuffer, filter?.Geometries?.UsePointAccuracy, filter?.Geometries?.UseDisturbanceRadius);

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

        public async Task<dynamic> GetObservationAsync(string authorizationApplicationIdentifier, string occurrenceId, OutputFieldSet outputFieldSet, string translationCultureCode, bool protectedObservations, bool includeInternalFields)
        {
            var filter = includeInternalFields ? new SearchFilterInternal() : new SearchFilter();
           
            filter.PopulateOutputFields(outputFieldSet);
           
            filter.ExtendedAuthorization.ProtectedObservations = protectedObservations;

            await _filterManager.PrepareFilter(authorizationApplicationIdentifier, filter, null, null, null, null, false);
            
            // If no user is authenticated, reset 
            if (filter.ExtendedAuthorization.UserId == 0)
            {
                filter.ExtendedAuthorization.ObservedByMe = false;
                filter.ExtendedAuthorization.ReportedByMe = false;
            }
            
            var processedObservation = await _processedObservationRepository.GetObservationAsync(occurrenceId, filter);

            PostProcessObservations(protectedObservations, processedObservation, translationCultureCode);
           
            return (processedObservation?.Count ?? 0) == 1 ? processedObservation[0] : null;
        }

        /// <inheritdoc />
        public async Task<long> IndexCountAsync(bool protectedIndex = false)
        {
            return await _processedObservationRepository.IndexCountAsync(protectedIndex);
        }
    }
}