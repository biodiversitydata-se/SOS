using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Models.Gis;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search;
using SOS.Observations.Api.Managers.Interfaces;
using SOS.Observations.Api.Repositories.Interfaces;

namespace SOS.Observations.Api.Managers
{
    /// <summary>
    ///     Observation manager class
    /// </summary>
    public class ObservationManager : IObservationManager
    {
        private readonly IFieldMappingManager _fieldMappingManager;
        private readonly ILogger<ObservationManager> _logger;
        private readonly IProcessedObservationRepository _processedObservationRepository;
        private readonly IFilterManager _filterManager;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="processedObservationRepository"></param>
        /// <param name="fieldMappingManager"></param>
        /// <param name="filterManager"></param>
        /// <param name="logger"></param>
        public ObservationManager(
            IProcessedObservationRepository processedObservationRepository,
            IFieldMappingManager fieldMappingManager,
            IFilterManager filterManager,
            ILogger<ObservationManager> logger)
        {
            _processedObservationRepository = processedObservationRepository ??
                                              throw new ArgumentNullException(nameof(processedObservationRepository));
            _fieldMappingManager = fieldMappingManager ?? throw new ArgumentNullException(nameof(fieldMappingManager));
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
                filter = await _filterManager.PrepareFilter(filter);
                var processedObservations =
                    await _processedObservationRepository.GetChunkAsync(filter, skip, take, sortBy, sortOrder);
                ProcessLocalizedFieldMappings(filter, processedObservations.Records);
                ProcessNonLocalizedFieldMappings(filter, processedObservations.Records);
                return processedObservations;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get chunk of observations");
                return null;
            }
        }

        /// <inheritdoc />
        public async Task<PagedResult<dynamic>> GetAggregatedChunkAsync(SearchFilter filter, AggregationType aggregationType, int skip, int take, string sortBy, SearchSortOrder sortOrder)
        {
            try
            {
                filter = await _filterManager.PrepareFilter(filter);

                if(aggregationType.IsDateHistogram())
                    return await _processedObservationRepository.GetAggregatedHistogramChunkAsync(filter, aggregationType);

                if(aggregationType.IsSpeciesSightingsList())
                    return await _processedObservationRepository.GetAggregatedChunkAsync(filter, aggregationType, skip, take, sortBy, sortOrder);

                return null;
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
                filter = await _filterManager.PrepareFilter(filter);
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
                filter = await _filterManager.PrepareFilter(filter);
                return await _processedObservationRepository.GetGeogridTileAggregationAsync(filter, precision, bbox);
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
            return await _processedObservationRepository.GetMatchCountAsync(filter);
        }

        public async Task<Result<PagedResult<TaxonAggregationItem>>> GetTaxonAggregationAsync(
            SearchFilter filter,
            LatLonBoundingBox bbox,
            int skip,
            int take)
        {
            try
            {
                filter = await _filterManager.PrepareFilter(filter);
                return await _processedObservationRepository.GetTaxonAggregationAsync(filter, bbox, skip, take);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get taxon aggregation");
                throw;
            }
        }

        private void ProcessNonLocalizedFieldMappings(SearchFilter filter, IEnumerable<object> processedObservations)
        {
            if (string.IsNullOrEmpty(filter.FieldTranslationCultureCode))
            {
                return;
            }

            foreach (var observation in processedObservations)
            {
                if (observation is IDictionary<string, object> obs)
                {
                    ResolveFieldMappedValue(obs, FieldMappingFieldId.BasisOfRecord,
                        nameof(Observation.BasisOfRecord));
                    ResolveFieldMappedValue(obs, FieldMappingFieldId.Type, nameof(Observation.Type));
                    ResolveFieldMappedValue(obs, FieldMappingFieldId.AccessRights,
                        nameof(Observation.AccessRights));
                    ResolveFieldMappedValue(obs, FieldMappingFieldId.Institution,
                        nameof(Observation.InstitutionCode));

                    if (obs.TryGetValue(nameof(Observation.Location).ToLower(), out var locationObject))
                    {
                        var locationDictionary = locationObject as IDictionary<string, object>;
                        ResolveFieldMappedValue(locationDictionary, FieldMappingFieldId.County,
                            nameof(Observation.Location.County));
                        ResolveFieldMappedValue(locationDictionary, FieldMappingFieldId.Municipality,
                            nameof(Observation.Location.Municipality));
                        ResolveFieldMappedValue(locationDictionary, FieldMappingFieldId.Province,
                            nameof(Observation.Location.Province));
                        ResolveFieldMappedValue(locationDictionary, FieldMappingFieldId.Parish,
                            nameof(Observation.Location.Parish));
                    }

                    if (obs.TryGetValue(nameof(Observation.Occurrence).ToLower(), out var occurrenceObject))
                    {
                        var occurrenceDictionary = occurrenceObject as IDictionary<string, object>;
                        ResolveFieldMappedValue(occurrenceDictionary, FieldMappingFieldId.EstablishmentMeans,
                            nameof(Observation.Occurrence.EstablishmentMeans));
                        ResolveFieldMappedValue(occurrenceDictionary, FieldMappingFieldId.OccurrenceStatus,
                            nameof(Observation.Occurrence.OccurrenceStatus));
                    }
                }
            }
        }

        private void ProcessLocalizedFieldMappings(SearchFilter filter, IEnumerable<dynamic> processedObservations)
        {
            if (string.IsNullOrEmpty(filter.FieldTranslationCultureCode))
            {
                return;
            }

            ProcessLocalizedFieldMappedReturnValues(processedObservations, filter.FieldTranslationCultureCode);
        }

        private void ProcessLocalizedFieldMappedReturnValues(
            IEnumerable<Observation> processedObservations,
            string cultureCode)
        {
            foreach (var observation in processedObservations)
            {
                TranslateLocalizedValue(observation.Occurrence?.Activity, FieldMappingFieldId.Activity, cultureCode);
                TranslateLocalizedValue(observation.Occurrence?.Gender, FieldMappingFieldId.Gender, cultureCode);
                TranslateLocalizedValue(observation.Occurrence?.LifeStage, FieldMappingFieldId.LifeStage, cultureCode);
                TranslateLocalizedValue(observation.Occurrence?.OrganismQuantityUnit, FieldMappingFieldId.Unit,
                    cultureCode);
                TranslateLocalizedValue(observation.Event?.Biotope, FieldMappingFieldId.Biotope, cultureCode);
                TranslateLocalizedValue(observation.Event?.Substrate, FieldMappingFieldId.Substrate, cultureCode);
                TranslateLocalizedValue(observation.Identification?.ValidationStatus,
                    FieldMappingFieldId.ValidationStatus, cultureCode);
            }
        }


        private void ProcessLocalizedFieldMappedReturnValues(
            IEnumerable<dynamic> processedObservations,
            string cultureCode)
        {
            try
            {
                foreach (var observation in processedObservations)
                {
                    if (observation is IDictionary<string, object> obs)
                    {
                        if (obs.TryGetValue(nameof(Observation.Occurrence).ToLower(),
                            out var occurrenceObject))
                        {
                            var occurrenceDictionary = occurrenceObject as IDictionary<string, object>;
                            TranslateLocalizedValue(occurrenceDictionary, FieldMappingFieldId.Activity,
                                nameof(Observation.Occurrence.Activity), cultureCode);
                            TranslateLocalizedValue(occurrenceDictionary, FieldMappingFieldId.Gender,
                                nameof(Observation.Occurrence.Gender), cultureCode);
                            TranslateLocalizedValue(occurrenceDictionary, FieldMappingFieldId.LifeStage,
                                nameof(Observation.Occurrence.LifeStage), cultureCode);
                            TranslateLocalizedValue(occurrenceDictionary, FieldMappingFieldId.Unit,
                                nameof(Observation.Occurrence.OrganismQuantityUnit), cultureCode);
                        }

                        if (obs.TryGetValue(nameof(Observation.Event).ToLower(), out var eventObject))
                        {
                            var eventDictionary = eventObject as IDictionary<string, object>;
                            TranslateLocalizedValue(eventDictionary, FieldMappingFieldId.Biotope,
                                nameof(Observation.Event.Biotope), cultureCode);
                            TranslateLocalizedValue(eventDictionary, FieldMappingFieldId.Substrate,
                                nameof(Observation.Event.Substrate), cultureCode);
                        }

                        if (obs.TryGetValue(nameof(Observation.Identification).ToLower(),
                            out var identificationObject))
                        {
                            var identificationDictionary = identificationObject as IDictionary<string, object>;
                            TranslateLocalizedValue(identificationDictionary, FieldMappingFieldId.ValidationStatus,
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
            FieldMappingFieldId fieldMappingFieldId,
            string fieldName)
        {
            if (observationNode == null) return;

            var camelCaseName = char.ToLower(fieldName[0]) + fieldName.Substring(1);
            if (observationNode.ContainsKey(camelCaseName))
            {
                if (observationNode[camelCaseName] is IDictionary<string, object> fieldNode && 
                    fieldNode.ContainsKey("id"))
                {
                    var id = Convert.ToInt32(fieldNode["id"]);
                    if (id != FieldMappingConstants.NoMappingFoundCustomValueIsUsedId &&
                        _fieldMappingManager.TryGetValue(fieldMappingFieldId, id, out var translatedValue))
                    {
                        fieldNode["Value"] = translatedValue;
                    }
                }
            }
        }

        private void ResolveFieldMappedValue(VocabularyValue fieldMapValue,
            FieldMappingFieldId fieldMappingFieldId)
        {
            if (fieldMapValue == null) return;
            if (fieldMapValue.Id != FieldMappingConstants.NoMappingFoundCustomValueIsUsedId
                && _fieldMappingManager.TryGetValue(fieldMappingFieldId, fieldMapValue.Id, out var translatedValue))
            {
                fieldMapValue.Value = translatedValue;
            }
        }

        private void TranslateLocalizedValue(
            VocabularyValue fieldMapValue,
            FieldMappingFieldId fieldMappingFieldId,
            string cultureCode)
        {
            if (fieldMapValue == null) return;

            if (fieldMapValue.Id != FieldMappingConstants.NoMappingFoundCustomValueIsUsedId
                && _fieldMappingManager.TryGetTranslatedValue(fieldMappingFieldId, cultureCode, fieldMapValue.Id,
                    out var translatedValue))
            {
                fieldMapValue.Value = translatedValue;
            }
        }

        private void TranslateLocalizedValue(
            IDictionary<string, object> observationNode,
            FieldMappingFieldId fieldMappingFieldId,
            string fieldName,
            string cultureCode)
        {
            if (observationNode == null) return;
            var lowerCaseName = char.ToLower(fieldName[0]) + fieldName.Substring(1);
            if (observationNode.ContainsKey(lowerCaseName))
            {
                if (observationNode[lowerCaseName] is IDictionary<string, object> fieldNode &&
                    fieldNode.ContainsKey("id"))
                {
                    var id = (long) fieldNode["id"];
                    if (id != FieldMappingConstants.NoMappingFoundCustomValueIsUsedId &&
                        _fieldMappingManager.TryGetTranslatedValue(fieldMappingFieldId, cultureCode, (int) id,
                            out var translatedValue))
                    {
                        fieldNode["Value"] = translatedValue;
                    }
                }
            }
        }
    }
}