using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Exceptions;
using SOS.Lib.Extensions;
using SOS.Lib.Managers.Interfaces;
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
        private readonly IVocabularyManager _vocabularyManager;
        private readonly IProcessedObservationRepository _processedObservationRepository;
        private readonly IProtectedLogRepository _protectedLogRepository;
        private readonly IFilterManager _filterManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
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
                foreach (var observation in processedObservations)
                {
                    if (observation is IDictionary<string, object> obs)
                    {
                        if (!string.IsNullOrEmpty(cultureCode))
                        {
                            ResolveLocalizedVocabularyMappedValues(obs, cultureCode);
                        }

                        ResolveNonLocalizedVocabularyMappedValues(obs);

                        if (protectedObservations && obs.TryGetValue(nameof(Observation.Occurrence).ToLower(),
                            out var occurrenceObject))
                        {
                            var occurrenceDictionary = occurrenceObject as IDictionary<string, object>;
                            if (occurrenceDictionary.TryGetValue("occurrenceId", out var occurenceId))
                            {
                                occurenceIds.Add(occurenceId as string);
                            }
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

        private void ResolveLocalizedVocabularyMappedValues(
            IDictionary<string, object> obs,
            string cultureCode)
        {
            if(obs.TryGetValue(nameof(Observation.Event).ToLower(), out var eventObject))
            {
                var eventDictionary = eventObject as IDictionary<string, object>;
                TranslateLocalizedValue(eventDictionary, VocabularyId.Biotope,
                    nameof(Observation.Occurrence.Biotope), cultureCode);
                TranslateLocalizedValue(eventDictionary, VocabularyId.DiscoveryMethod,
                    nameof(Observation.Event.DiscoveryMethod), cultureCode);
            }

            if (obs.TryGetValue(nameof(Observation.Identification).ToLower(),
                out var identificationObject))
            {
                var identificationDictionary = identificationObject as IDictionary<string, object>;
                TranslateLocalizedValue(identificationDictionary, VocabularyId.ValidationStatus,
                    nameof(Observation.Identification.ValidationStatus), cultureCode);
            }

            if (obs.TryGetValue(nameof(Observation.Occurrence).ToLower(),
                            out var occurrenceObject))
            {
                var occurrenceDictionary = occurrenceObject as IDictionary<string, object>;
               
                TranslateLocalizedValue(occurrenceDictionary, VocabularyId.Activity,
                    nameof(Observation.Occurrence.Activity), cultureCode);
                TranslateLocalizedValue(occurrenceDictionary, VocabularyId.Behavior,
                    nameof(Observation.Occurrence.Behavior), cultureCode);
                TranslateLocalizedValue(occurrenceDictionary, VocabularyId.Sex,
                    nameof(Observation.Occurrence.Sex), cultureCode);
                TranslateLocalizedValue(occurrenceDictionary, VocabularyId.ReproductiveCondition,
                    nameof(Observation.Occurrence.ReproductiveCondition), cultureCode);
                TranslateLocalizedValue(occurrenceDictionary, VocabularyId.LifeStage,
                    nameof(Observation.Occurrence.LifeStage), cultureCode);
                TranslateLocalizedValue(occurrenceDictionary, VocabularyId.Unit,
                    nameof(Observation.Occurrence.OrganismQuantityUnit), cultureCode);

                if (occurrenceDictionary.TryGetValue(nameof(Observation.Occurrence.Substrate).ToLower(),
                    out var substrateObject))
                {
                    var substrateDictionary = substrateObject as IDictionary<string, object>;
                    TranslateLocalizedValue(substrateDictionary, VocabularyId.Substrate,
                        nameof(Observation.Occurrence.Substrate.Name), cultureCode);
                }
            }
        }

        private void ResolveNonLocalizedVocabularyMappedValues(
            IDictionary<string, object> obs)
        {
            if (obs.TryGetValue(nameof(Observation.Occurrence).ToLower(),
                            out var occurrenceObject))
            {
                var occurrenceDictionary = occurrenceObject as IDictionary<string, object>;
                
                // Resolve Non Localized Vocabulary Fields
                ResolveVocabularyMappedValue(occurrenceDictionary, VocabularyId.EstablishmentMeans,
                    nameof(Observation.Occurrence.EstablishmentMeans));
                ResolveVocabularyMappedValue(occurrenceDictionary, VocabularyId.OccurrenceStatus,
                    nameof(Observation.Occurrence.OccurrenceStatus));
            }

            // Resolve Non Localized Vocabulary Fields
            ResolveVocabularyMappedValue(obs, VocabularyId.BasisOfRecord,
                nameof(Observation.BasisOfRecord));
            ResolveVocabularyMappedValue(obs, VocabularyId.Type, nameof(Observation.Type));
            ResolveVocabularyMappedValue(obs, VocabularyId.AccessRights,
                nameof(Observation.AccessRights));
            ResolveVocabularyMappedValue(obs, VocabularyId.Institution,
                nameof(Observation.InstitutionCode));

            if (obs.TryGetValue(nameof(Observation.Location).ToLower(), out var locationObject))
            {
                var locationDictionary = locationObject as IDictionary<string, object>;
                ResolveVocabularyMappedValue(locationDictionary, VocabularyId.Continent,
                    nameof(Observation.Location.Continent));
                ResolveVocabularyMappedValue(locationDictionary, VocabularyId.Country,
                    nameof(Observation.Location.Country));
            }
        }

        private void ResolveVocabularyMappedValue(
            IDictionary<string, object> observationNode,
            VocabularyId vocabularyId,
            string fieldName)
        {
            if (observationNode == null) return;

            var camelCaseName = fieldName.ToCamelCase();
            if (observationNode.ContainsKey(camelCaseName))
            {
                if (observationNode[camelCaseName] is IDictionary<string, object> fieldNode &&
                    fieldNode.ContainsKey("id"))
                {
                    var id = Convert.ToInt32(fieldNode["id"]);
                    if (id != VocabularyConstants.NoMappingFoundCustomValueIsUsedId &&
                        _vocabularyManager.TryGetValue(vocabularyId, id, out var translatedValue))
                    {
                        fieldNode["value"] = translatedValue;
                    }
                }
            }
        }

        private void TranslateLocalizedValue(
            IDictionary<string, object> observationNode,
            VocabularyId vocabularyId,
            string fieldName,
            string cultureCode)
        {
            if (observationNode == null) return;
            var camelCaseName = fieldName.ToCamelCase();
            if (observationNode.ContainsKey(camelCaseName))
            {
                if (observationNode[camelCaseName] is IDictionary<string, object> fieldNode &&
                    fieldNode.ContainsKey("id"))
                {
                    var id = (long)fieldNode["id"];
                    if (id != VocabularyConstants.NoMappingFoundCustomValueIsUsedId &&
                        _vocabularyManager.TryGetTranslatedValue(vocabularyId, cultureCode, (int)id,
                            out var translatedValue))
                    {
                        fieldNode["value"] = translatedValue;
                    }
                }
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="processedObservationRepository"></param>
        /// <param name="protectedLogRepository"></param>
        /// <param name="vocabularyManager"></param>
        /// <param name="filterManager"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="logger"></param>
        public ObservationManager(
            IProcessedObservationRepository processedObservationRepository,
            IProtectedLogRepository protectedLogRepository,
            IVocabularyManager vocabularyManager,
            IFilterManager filterManager,
            IHttpContextAccessor httpContextAccessor,
            ILogger<ObservationManager> logger)
        {
            _processedObservationRepository = processedObservationRepository ??
                                              throw new ArgumentNullException(nameof(processedObservationRepository));
            _protectedLogRepository =
                protectedLogRepository ?? throw new ArgumentNullException(nameof(protectedLogRepository));
            _vocabularyManager = vocabularyManager ?? throw new ArgumentNullException(nameof(vocabularyManager));
            _filterManager = filterManager ?? throw new ArgumentNullException(nameof(filterManager));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));

             _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
        public async Task<long> GetMatchCountAsync(int? roleId, string authorizationApplicationIdentifier, FilterBase filter)
        {
            await _filterManager.PrepareFilter(roleId, authorizationApplicationIdentifier, filter);
            return await _processedObservationRepository.GetMatchCountAsync(filter);
        }

        /// <inheritdoc />
        public async Task<Result<PagedResult<TaxonAggregationItem>>> GetTaxonAggregationAsync(
            int? roleId,
            string authorizationApplicationIdentifier,
            SearchFilter filter,
            int? skip,
            int? take)
        {
            try
            {
                await _filterManager.PrepareFilter(roleId, authorizationApplicationIdentifier, filter);
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

        public async Task<dynamic> GetObservationAsync(int? roleId, string authorizationApplicationIdentifier, string occurrenceId, OutputFieldSet outputFieldSet, string translationCultureCode, bool protectedObservations, bool includeInternalFields)
        {
            var filter = includeInternalFields ? new SearchFilterInternal() : new SearchFilter();

            filter.PopulateOutputFields(outputFieldSet);
            filter.ExtendedAuthorization.ProtectedObservations = protectedObservations;

            await _filterManager.PrepareFilter(roleId, authorizationApplicationIdentifier, filter, null, null, null, null, false);
            
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