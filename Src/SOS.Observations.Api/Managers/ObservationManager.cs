using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Exceptions;
using SOS.Lib.Extensions;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Gis;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search;
using SOS.Observations.Api.Dtos;
using SOS.Observations.Api.Extensions;
using SOS.Observations.Api.Managers.Interfaces;
using SOS.Observations.Api.Repositories.Interfaces;

namespace SOS.Observations.Api.Managers
{
    /// <summary>
    ///     Observation manager class
    /// </summary>
    public class ObservationManager : IObservationManager
    {
        private readonly IVocabularyManager _vocabularyManager;
        private readonly IProcessedObservationRepository _processedObservationRepository;
        private readonly IFilterManager _filterManager;
        private readonly ILogger<ObservationManager> _logger;

        /// <summary>
        ///  Constructor
        /// </summary>
        /// <param name="processedObservationRepository"></param>
        /// <param name="vocabularyManager"></param>
        /// <param name="filterManager"></param>
        /// <param name="logger"></param>
        public ObservationManager(
            IProcessedObservationRepository processedObservationRepository,
            IVocabularyManager vocabularyManager,
            IFilterManager filterManager,
            ILogger<ObservationManager> logger)
        {
            _processedObservationRepository = processedObservationRepository ??
                                              throw new ArgumentNullException(nameof(processedObservationRepository));
            _vocabularyManager = vocabularyManager ?? throw new ArgumentNullException(nameof(vocabularyManager));
            _filterManager = filterManager ?? throw new ArgumentNullException(nameof(filterManager));
           
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public int MaxNrElasticSearchAggregationBuckets => _processedObservationRepository.MaxNrElasticSearchAggregationBuckets;

        /// <inheritdoc />
        public async Task<PagedResult<dynamic>> GetChunkAsync(SearchFilter filter, int skip, int take, string sortBy,
            SearchSortOrder sortOrder)
        {
            try
            {
                await _filterManager.PrepareFilter(filter);
                var processedObservations =
                    await _processedObservationRepository.GetChunkAsync(filter, skip, take, sortBy, sortOrder);
                ResolveLocalizedVocabularyFields(filter.FieldTranslationCultureCode, processedObservations.Records);
                ResolveNonLocalizedVocabularyFields(processedObservations.Records);
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
        public async Task<PagedResult<dynamic>> GetAggregatedChunkAsync(SearchFilter filter, AggregationType aggregationType, int skip, int take)
        {
            try
            {
                await _filterManager.PrepareFilter(filter);

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

        
        /// <summary>
        /// Get aggregated grid cells data.
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="precision"></param>
        /// <param name="bbox"></param>
        /// <returns></returns>
        public async Task<Result<GeoGridResult>> GetGeogridAggregationAsync(SearchFilter filter, int precision, LatLonBoundingBox bbox)
        {
            try
            {
                await _filterManager.PrepareFilter(filter);
                return await _processedObservationRepository.GetGeogridAggregationAsync(filter, precision, bbox);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to aggregate to geogrids.");
                throw;
            }
        }

        /// <summary>
        /// Get aggregated grid cells data.
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="precision"></param>
        /// <param name="bbox"></param>
        /// <returns></returns>
        public async Task<Result<GeoGridTileResult>> GetGeogridTileAggregationAsync(SearchFilter filter, int precision, LatLonBoundingBox bbox)
        {
            try
            {
                await _filterManager.PrepareFilter(filter);
                return await _processedObservationRepository.GetGeogridTileAggregationAsync(filter, precision, bbox);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to aggregate to geogrids.");
                throw;
            }
        }

        /// <summary>
        /// Aggregate observations by GeoTile and Taxa. This method handles all paging and returns the complete result.
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="zoom">The precision to use in the GeoTileGrid aggregation.</param>
        /// <param name="bbox"></param>
        /// <returns></returns>
        public async Task<Result<IEnumerable<GeoGridTileTaxaCell>>> GetCompleteGeoTileTaxaAggregationAsync(
            SearchFilter filter, 
            int zoom, 
            LatLonBoundingBox bbox)
        {
            try
            {
                await _filterManager.PrepareFilter(filter);
                return await _processedObservationRepository.GetCompleteGeoTileTaxaAggregationAsync(filter, zoom, bbox);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to aggregate to geogrids.");
                throw;
            }
        }

        /// <summary>
        /// Aggregate observations by GeoTile and Taxa. This method uses paging.
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="zoom">The precision to use in the GeoTileGrid aggregation.</param>
        /// <param name="bbox"></param>
        /// <param name="geoTilePage">The GeoTile key. Should be null in the first request.</param>
        /// <param name="taxonIdPage">The TaxonId key. Should be null in the first request.</param>
        /// <returns></returns>
        public async Task<Result<GeoGridTileTaxonPageResult>> GetPageGeoTileTaxaAggregationAsync(
            SearchFilter filter,
            int zoom,
            LatLonBoundingBox bbox,
            string geoTilePage,
            int? taxonIdPage)
        {
            try
            {
                await _filterManager.PrepareFilter(filter);
                return await _processedObservationRepository.GetPageGeoTileTaxaAggregationAsync(filter, zoom, bbox, geoTilePage, taxonIdPage);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to aggregate to geogrids.");
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<DateTime> GetLatestModifiedDateForProviderAsync(int providerId)
        {
            return await _processedObservationRepository.GetLatestModifiedDateForProviderAsync(providerId);
        }

        /// <inheritdoc />
        public async Task<long> GetMatchCountAsync(FilterBase filter)
        {
            await _filterManager.PrepareFilter(filter);
            return await _processedObservationRepository.GetMatchCountAsync(filter);
        }

        /// <inheritdoc />
        public async Task<Result<PagedResult<TaxonAggregationItem>>> GetTaxonAggregationAsync(
            SearchFilter filter,
            LatLonBoundingBox bbox,
            int? skip,
            int? take)
        {
            try
            {
                await _filterManager.PrepareFilter(filter);
                return await _processedObservationRepository.GetTaxonAggregationAsync(filter, bbox, skip, take);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get taxon aggregation");
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TaxonAggregationItemDto>> GetTaxonExistsIndicationAsync(
            SearchFilter filter)
        {
            try
            {
                await _filterManager.PrepareFilter(filter);

                if (filter?.TaxonIds?.Count() > 10000)
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

        private void ResolveNonLocalizedVocabularyFields(IEnumerable<object> processedObservations)
        {
            foreach (var observation in processedObservations)
            {
                if (observation is IDictionary<string, object> obs)
                {
                    ResolveFieldMappedValue(obs, VocabularyId.BasisOfRecord,
                        nameof(Observation.BasisOfRecord));
                    ResolveFieldMappedValue(obs, VocabularyId.Type, nameof(Observation.Type));
                    ResolveFieldMappedValue(obs, VocabularyId.AccessRights,
                        nameof(Observation.AccessRights));
                    ResolveFieldMappedValue(obs, VocabularyId.Institution,
                        nameof(Observation.InstitutionCode));

                    if (obs.TryGetValue(nameof(Observation.Location).ToLower(), out var locationObject))
                    {
                        var locationDictionary = locationObject as IDictionary<string, object>;
                        ResolveFieldMappedValue(locationDictionary, VocabularyId.Continent,
                            nameof(Observation.Location.Continent));
                        ResolveFieldMappedValue(locationDictionary, VocabularyId.Country,
                            nameof(Observation.Location.Country));
                    }

                    if (obs.TryGetValue(nameof(Observation.Occurrence).ToLower(), out var occurrenceObject))
                    {
                        var occurrenceDictionary = occurrenceObject as IDictionary<string, object>;
                        ResolveFieldMappedValue(occurrenceDictionary, VocabularyId.EstablishmentMeans,
                            nameof(Observation.Occurrence.EstablishmentMeans));
                        ResolveFieldMappedValue(occurrenceDictionary, VocabularyId.OccurrenceStatus,
                            nameof(Observation.Occurrence.OccurrenceStatus));
                    }
                }
            }
        }

        private void ResolveLocalizedVocabularyFields(string fieldTranslationCultureCode, IEnumerable<dynamic> processedObservations)
        {
            if (string.IsNullOrEmpty(fieldTranslationCultureCode))
            {
                return;
            }

            ResolveLocalizedVocabularyMappedValues(processedObservations, fieldTranslationCultureCode);
        }

        private void ResolveLocalizedVocabularyMappedValues(
            IEnumerable<Observation> processedObservations,
            string cultureCode)
        {
            foreach (var observation in processedObservations)
            {
                TranslateLocalizedValue(observation.Occurrence?.Biotope, VocabularyId.Biotope, cultureCode);
                TranslateLocalizedValue(observation.Event?.DiscoveryMethod, VocabularyId.DiscoveryMethod, cultureCode);
                TranslateLocalizedValue(observation.Identification?.ValidationStatus,
                    VocabularyId.ValidationStatus, cultureCode);
                TranslateLocalizedValue(observation.Occurrence?.Activity, VocabularyId.Activity, cultureCode);
                TranslateLocalizedValue(observation.Occurrence?.Sex, VocabularyId.Sex, cultureCode);
                TranslateLocalizedValue(observation.Occurrence?.Substrate?.Name, VocabularyId.Substrate, cultureCode);
                TranslateLocalizedValue(observation.Occurrence?.ReproductiveCondition, VocabularyId.ReproductiveCondition, cultureCode);
                TranslateLocalizedValue(observation.Occurrence?.LifeStage, VocabularyId.LifeStage, cultureCode);
                TranslateLocalizedValue(observation.Occurrence?.OrganismQuantityUnit, VocabularyId.Unit,
                    cultureCode);
            }
        }

        private void ResolveLocalizedVocabularyMappedValues(
            IEnumerable<dynamic> processedObservations,
            string cultureCode)
        {
            try
            {
                foreach (var observation in processedObservations)
                {
                    if (observation is IDictionary<string, object> obs)
                    {
                        if (obs.TryGetValue(nameof(Observation.Event).ToLower(), out var eventObject))
                        {
                            var eventDictionary = eventObject as IDictionary<string, object>;
                            TranslateLocalizedValue(eventDictionary, VocabularyId.Biotope,
                                nameof(Observation.Occurrence.Biotope), cultureCode);
                            TranslateLocalizedValue(eventDictionary, VocabularyId.DiscoveryMethod,
                                nameof(Observation.Event.DiscoveryMethod), cultureCode);
                        }

                        if (obs.TryGetValue(nameof(Observation.Occurrence).ToLower(),
                            out var occurrenceObject))
                        {
                            var occurrenceDictionary = occurrenceObject as IDictionary<string, object>;
                            TranslateLocalizedValue(occurrenceDictionary, VocabularyId.Activity,
                                nameof(Observation.Occurrence.Activity), cultureCode);
                            TranslateLocalizedValue(occurrenceDictionary, VocabularyId.Sex,
                                nameof(Observation.Occurrence.Sex), cultureCode);
                            TranslateLocalizedValue(occurrenceDictionary, VocabularyId.ReproductiveCondition,
                                nameof(Observation.Occurrence.ReproductiveCondition), cultureCode);
                            TranslateLocalizedValue(occurrenceDictionary, VocabularyId.LifeStage,
                                nameof(Observation.Occurrence.LifeStage), cultureCode);
                            TranslateLocalizedValue(occurrenceDictionary, VocabularyId.Unit,
                                nameof(Observation.Occurrence.OrganismQuantityUnit), cultureCode);

                            if (obs.TryGetValue(nameof(Observation.Occurrence.Substrate).ToLower(),
                                out var substrateObject))
                            {
                                var substrateDictionary = substrateObject as IDictionary<string, object>;
                                TranslateLocalizedValue(substrateDictionary, VocabularyId.Substrate,
                                    nameof(Observation.Occurrence.Substrate.Name), cultureCode);
                            }
                        }

                        if (obs.TryGetValue(nameof(Observation.Identification).ToLower(),
                            out var identificationObject))
                        {
                            var identificationDictionary = identificationObject as IDictionary<string, object>;
                            TranslateLocalizedValue(identificationDictionary, VocabularyId.ValidationStatus,
                                nameof(Observation.Identification.ValidationStatus), cultureCode);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private void ResolveFieldMappedValue(
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

        private void ResolveFieldMappedValue(VocabularyValue fieldMapValue,
            VocabularyId vocabularyId)
        {
            if (fieldMapValue == null) return;
            if (fieldMapValue.Id != VocabularyConstants.NoMappingFoundCustomValueIsUsedId
                && _vocabularyManager.TryGetValue(vocabularyId, fieldMapValue.Id, out var translatedValue))
            {
                fieldMapValue.Value = translatedValue;
            }
        }

        private void TranslateLocalizedValue(
            VocabularyValue fieldMapValue,
            VocabularyId vocabularyId,
            string cultureCode)
        {
            if (fieldMapValue == null) return;

            if (fieldMapValue.Id != VocabularyConstants.NoMappingFoundCustomValueIsUsedId
                && _vocabularyManager.TryGetTranslatedValue(vocabularyId, cultureCode, fieldMapValue.Id,
                    out var translatedValue))
            {
                fieldMapValue.Value = translatedValue;
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
                    var id = (long) fieldNode["id"];
                    if (id != VocabularyConstants.NoMappingFoundCustomValueIsUsedId &&
                        _vocabularyManager.TryGetTranslatedValue(vocabularyId, cultureCode, (int) id,
                            out var translatedValue))
                    {
                        fieldNode["value"] = translatedValue;
                    }
                }
            }
        }

        public async Task<dynamic> GetObservationAsync(string occurrenceId, bool protectedObservations, bool includeInternalFields)
        {
            SearchFilter filter = new SearchFilter();
            if (includeInternalFields)
            {
                filter = new SearchFilterInternal();
            }
            filter.ProtectedObservations = protectedObservations;
            await _filterManager.PrepareFilter(filter);
            var processedObservation = await _processedObservationRepository.GetObservationAsync(occurrenceId, filter);
            var obs = new List<dynamic>() { processedObservation };
            ResolveLocalizedVocabularyFields("", obs);
            ResolveNonLocalizedVocabularyFields(obs);
            return obs.First();
        }
    }
}