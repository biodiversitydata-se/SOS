using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
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
        private const int BiotaTaxonId = 0;
        private readonly IAreaRepository _areaRepository;
        private readonly IFieldMappingManager _fieldMappingManager;
        private readonly ILogger<ObservationManager> _logger;
        private readonly IProcessedObservationRepository _processedObservationRepository;
        private readonly ITaxonManager _taxonManager;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="areaRepository"></param>
        /// <param name="processedObservationRepository"></param>
        /// <param name="fieldMappingManager"></param>
        /// <param name="areaManager"></param>
        /// <param name="taxonManager"></param>
        /// <param name="logger"></param>
        public ObservationManager(
            IAreaRepository areaRepository,
            IProcessedObservationRepository processedObservationRepository,
            IFieldMappingManager fieldMappingManager,
            IAreaManager areaManager,
            ITaxonManager taxonManager,
            ILogger<ObservationManager> logger)
        {
            _areaRepository = areaRepository ?? throw new ArgumentNullException(nameof(areaRepository));
            _processedObservationRepository = processedObservationRepository ??
                                              throw new ArgumentNullException(nameof(processedObservationRepository));
            _fieldMappingManager = fieldMappingManager ?? throw new ArgumentNullException(nameof(fieldMappingManager));
            _taxonManager = taxonManager ?? throw new ArgumentNullException(nameof(taxonManager));

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<PagedResult<dynamic>> GetChunkAsync(SearchFilter filter, int skip, int take, string sortBy,
            SearchSortOrder sortOrder)
        {
            try
            {
                filter = await PrepareFilter(filter);

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

        private async Task<SearchFilter> PrepareFilter(SearchFilter filter)
        {
            var preparedFilter = filter.Clone();

            if (preparedFilter.IncludeUnderlyingTaxa && preparedFilter.TaxonIds != null &&
                preparedFilter.TaxonIds.Any())
            {
                if (preparedFilter.TaxonIds.Contains(BiotaTaxonId)) // If Biota, then clear taxon filter
                {
                    preparedFilter.TaxonIds = new List<int>();
                }
                else
                {
                    preparedFilter.TaxonIds =
                        _taxonManager.TaxonTree.GetUnderlyingTaxonIds(preparedFilter.TaxonIds, true);
                }
            }

            // handle the area ids search
            if (preparedFilter.AreaIds != null && preparedFilter.AreaIds.Any())
            {
                foreach (var areaId in preparedFilter.AreaIds)
                {
                    var area = await _areaRepository.GetAreaAsync(areaId);

                    if (area != null)
                    {
                        //if we already have the info needed for the search we skip polygon searches
                        if (area.AreaType == AreaType.County ||
                            area.AreaType == AreaType.Municipality ||
                            area.AreaType == AreaType.Province)
                        {
                            if (area.AreaType == AreaType.County)
                            {
                                if (preparedFilter.CountyIds == null)
                                {
                                    preparedFilter.CountyIds = new List<int>();
                                }

                                var list = preparedFilter.CountyIds.ToList();
                                list.Add(area.Id);
                                preparedFilter.CountyIds = list;
                            }
                            else if (area.AreaType == AreaType.Municipality)
                            {
                                if (preparedFilter.MunicipalityIds == null)
                                {
                                    preparedFilter.MunicipalityIds = new List<int>();
                                }

                                var list = preparedFilter.MunicipalityIds.ToList();
                                list.Add(area.Id);
                                preparedFilter.MunicipalityIds = list;
                            }
                            else if (area.AreaType == AreaType.Province)
                            {
                                if (preparedFilter.ProvinceIds == null)
                                {
                                    preparedFilter.ProvinceIds = new List<int>();
                                }

                                var list = preparedFilter.ProvinceIds.ToList();
                                list.Add(area.Id);
                                preparedFilter.ProvinceIds = list;
                            }
                        }
                        else // we need to use the geometry filter
                        {
                            var geometry = await _areaRepository.GetGeometryAsync(areaId);

                            if (preparedFilter.GeometryFilter == null)
                            {
                                preparedFilter.GeometryFilter = new GeometryFilter();
                                preparedFilter.GeometryFilter.MaxDistanceFromPoint = 0;
                            }

                            preparedFilter.GeometryFilter.Geometries.Add(geometry);
                        }
                    }
                }
            }

            return preparedFilter;
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
                        nameof(ProcessedObservation.BasisOfRecord));
                    ResolveFieldMappedValue(obs, FieldMappingFieldId.Type, nameof(ProcessedObservation.Type));
                    ResolveFieldMappedValue(obs, FieldMappingFieldId.AccessRights,
                        nameof(ProcessedObservation.AccessRights));
                    ResolveFieldMappedValue(obs, FieldMappingFieldId.Institution,
                        nameof(ProcessedObservation.InstitutionId));

                    if (obs.TryGetValue(nameof(ProcessedObservation.Location), out var locationObject))
                    {
                        var locationDictionary = locationObject as IDictionary<string, object>;
                        ResolveFieldMappedValue(locationDictionary, FieldMappingFieldId.County,
                            nameof(ProcessedObservation.Location.County));
                        ResolveFieldMappedValue(locationDictionary, FieldMappingFieldId.Municipality,
                            nameof(ProcessedObservation.Location.Municipality));
                        ResolveFieldMappedValue(locationDictionary, FieldMappingFieldId.Province,
                            nameof(ProcessedObservation.Location.Province));
                        ResolveFieldMappedValue(locationDictionary, FieldMappingFieldId.Parish,
                            nameof(ProcessedObservation.Location.Parish));
                    }

                    if (obs.TryGetValue(nameof(ProcessedObservation.Occurrence), out var occurrenceObject))
                    {
                        var occurrenceDictionary = occurrenceObject as IDictionary<string, object>;
                        ResolveFieldMappedValue(occurrenceDictionary, FieldMappingFieldId.EstablishmentMeans,
                            nameof(ProcessedObservation.Occurrence.EstablishmentMeans));
                        ResolveFieldMappedValue(occurrenceDictionary, FieldMappingFieldId.OccurrenceStatus,
                            nameof(ProcessedObservation.Occurrence.OccurrenceStatus));
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
            IEnumerable<ProcessedObservation> processedObservations,
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
                        if (obs.TryGetValue(nameof(ProcessedObservation.Occurrence).ToLower(),
                            out var occurrenceObject))
                        {
                            var occurrenceDictionary = occurrenceObject as IDictionary<string, object>;
                            TranslateLocalizedValue(occurrenceDictionary, FieldMappingFieldId.Activity,
                                nameof(ProcessedObservation.Occurrence.Activity), cultureCode);
                            TranslateLocalizedValue(occurrenceDictionary, FieldMappingFieldId.Gender,
                                nameof(ProcessedObservation.Occurrence.Gender), cultureCode);
                            TranslateLocalizedValue(occurrenceDictionary, FieldMappingFieldId.LifeStage,
                                nameof(ProcessedObservation.Occurrence.LifeStage), cultureCode);
                            TranslateLocalizedValue(occurrenceDictionary, FieldMappingFieldId.Unit,
                                nameof(ProcessedObservation.Occurrence.OrganismQuantityUnit), cultureCode);
                        }

                        if (obs.TryGetValue(nameof(ProcessedObservation.Event).ToLower(), out var eventObject))
                        {
                            var eventDictionary = eventObject as IDictionary<string, object>;
                            TranslateLocalizedValue(eventDictionary, FieldMappingFieldId.Biotope,
                                nameof(ProcessedObservation.Event.Biotope), cultureCode);
                            TranslateLocalizedValue(eventDictionary, FieldMappingFieldId.Substrate,
                                nameof(ProcessedObservation.Event.Substrate), cultureCode);
                        }

                        if (obs.TryGetValue(nameof(ProcessedObservation.Identification).ToLower(),
                            out var identificationObject))
                        {
                            var identificationDictionary = identificationObject as IDictionary<string, object>;
                            TranslateLocalizedValue(identificationDictionary, FieldMappingFieldId.ValidationStatus,
                                nameof(ProcessedObservation.Identification.ValidationStatus), cultureCode);
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

            if (observationNode.ContainsKey(fieldName))
            {
                if (observationNode[fieldName] is IDictionary<string, object> fieldNode &&
                    fieldNode.ContainsKey("Value") && fieldNode.ContainsKey("_id"))
                {
                    var id = (int) fieldNode["_id"];
                    if (id != FieldMappingConstants.NoMappingFoundCustomValueIsUsedId &&
                        _fieldMappingManager.TryGetValue(fieldMappingFieldId, id, out var translatedValue))
                    {
                        fieldNode["Value"] = translatedValue;
                    }
                }
            }
        }

        private void ResolveFieldMappedValue(ProcessedFieldMapValue fieldMapValue,
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
            ProcessedFieldMapValue fieldMapValue,
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