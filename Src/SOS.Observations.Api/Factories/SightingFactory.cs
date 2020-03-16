using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.Sighting;
using SOS.Lib.Models.Search;
using SOS.Observations.Api.Factories.Interfaces;
using SOS.Observations.Api.Repositories.Interfaces;

namespace SOS.Observations.Api.Factories
{
    /// <summary>
    /// Sighting factory class
    /// </summary>
    public class SightingFactory : Interfaces.ISightingFactory
    {
        private readonly IProcessedSightingRepository _processedSightingRepository;
        private readonly IFieldMappingFactory _fieldMappingFactory;
        private readonly ILogger<SightingFactory> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="processedSightingRepository"></param>
        /// <param name="fieldMappingFactory"></param>
        /// <param name="logger"></param>
        public SightingFactory(
            IProcessedSightingRepository processedSightingRepository,
            IFieldMappingFactory fieldMappingFactory,
            ILogger<SightingFactory> logger)
        {
            _processedSightingRepository = processedSightingRepository ?? throw new ArgumentNullException(nameof(processedSightingRepository));
            _fieldMappingFactory = fieldMappingFactory ?? throw new ArgumentNullException(nameof(fieldMappingFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<IEnumerable<dynamic>> GetChunkAsync(SearchFilter filter, int skip, int take)
        {
            try
            {
                var processedSightings = (await _processedSightingRepository.GetChunkAsync(filter, skip, take)).ToArray();
                ProcessLocalizedFieldMappings(filter, processedSightings);
                ProcessNonLocalizedFieldMappings(filter, processedSightings);
                return processedSightings;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get chunk of sightings");
                return null;
            }
        }

        private void ProcessNonLocalizedFieldMappings(SearchFilter filter, IEnumerable<object> processedSightings)
        {
            if (!filter.TranslateFieldMappedValues) return;

            if (filter.OutputFields == null || !filter.OutputFields.Any()) // ProcessedSighting objects is returned wen OutputFields is not used.
            {
                var sightings = processedSightings.Cast<ProcessedSighting>();
                foreach (var sighting in sightings)
                {
                    ResolveFieldMappedValue(sighting.BasisOfRecordId, FieldMappingFieldId.BasisOfRecord);
                    ResolveFieldMappedValue(sighting.TypeId, FieldMappingFieldId.Type);
                    ResolveFieldMappedValue(sighting.AccessRightsId, FieldMappingFieldId.AccessRights);
                    ResolveFieldMappedValue(sighting.InstitutionId, FieldMappingFieldId.Institution);
                    ResolveFieldMappedValue(sighting.Location?.CountyId, FieldMappingFieldId.County);
                    ResolveFieldMappedValue(sighting.Location?.MunicipalityId, FieldMappingFieldId.Municipality);
                    ResolveFieldMappedValue(sighting.Location?.ProvinceId, FieldMappingFieldId.Province);
                    ResolveFieldMappedValue(sighting.Location?.ParishId, FieldMappingFieldId.Parish);
                    ResolveFieldMappedValue(sighting.Location?.CountryId, FieldMappingFieldId.Country);
                    ResolveFieldMappedValue(sighting.Location?.ContinentId, FieldMappingFieldId.Continent);
                    ResolveFieldMappedValue(sighting.Occurrence?.EstablishmentMeansId, FieldMappingFieldId.EstablishmentMeans);
                    ResolveFieldMappedValue(sighting.Occurrence?.OccurrenceStatusId, FieldMappingFieldId.OccurrenceStatus);
                }
            }
            else // dynamic objects is returned when OutputFields is used
            {
                foreach (var sighting in processedSightings)
                {
                    if (sighting is IDictionary<string, object> obs)
                    {
                        ResolveFieldMappedValue(obs, FieldMappingFieldId.BasisOfRecord, nameof(ProcessedSighting.BasisOfRecordId));
                        ResolveFieldMappedValue(obs, FieldMappingFieldId.Type, nameof(ProcessedSighting.TypeId));
                        ResolveFieldMappedValue(obs, FieldMappingFieldId.AccessRights, nameof(ProcessedSighting.AccessRightsId));
                        ResolveFieldMappedValue(obs, FieldMappingFieldId.Institution, nameof(ProcessedSighting.InstitutionId));

                        if (obs.TryGetValue(nameof(ProcessedSighting.Location), out object locationObject))
                        {
                            var locationDictionary = locationObject as IDictionary<string, object>;
                            ResolveFieldMappedValue(locationDictionary, FieldMappingFieldId.County, nameof(ProcessedSighting.Location.CountyId));
                            ResolveFieldMappedValue(locationDictionary, FieldMappingFieldId.Municipality, nameof(ProcessedSighting.Location.MunicipalityId));
                            ResolveFieldMappedValue(locationDictionary, FieldMappingFieldId.Province, nameof(ProcessedSighting.Location.ProvinceId));
                            ResolveFieldMappedValue(locationDictionary, FieldMappingFieldId.Parish, nameof(ProcessedSighting.Location.ParishId));
                        }

                        if (obs.TryGetValue(nameof(ProcessedSighting.Occurrence), out object occurrenceObject))
                        {
                            var occurrenceDictionary = occurrenceObject as IDictionary<string, object>;
                            ResolveFieldMappedValue(occurrenceDictionary, FieldMappingFieldId.EstablishmentMeans, nameof(ProcessedSighting.Occurrence.EstablishmentMeansId));
                            ResolveFieldMappedValue(occurrenceDictionary, FieldMappingFieldId.OccurrenceStatus, nameof(ProcessedSighting.Occurrence.OccurrenceStatusId));
                        }
                    }
                }
            }
        }

        private void ProcessLocalizedFieldMappings(SearchFilter filter, IEnumerable<dynamic> processedSightings)
        {
            if (!filter.TranslateFieldMappedValues) return;
            string cultureCode = filter.TranslationCultureCode;
            if (filter.OutputFields == null || !filter.OutputFields.Any()) // ProcessedSighting objects is returned wen OutputFields is not used.
            {
                var sightings = processedSightings.Cast<ProcessedSighting>();
                ProcessLocalizedFieldMappedReturnValues(sightings, cultureCode);
            }
            else // dynamic objects is returned when OutputFields is used
            {
                ProcessLocalizedFieldMappedReturnValues(processedSightings, cultureCode);
            }
        }

        private void ProcessLocalizedFieldMappedReturnValues(
            IEnumerable<ProcessedSighting> processedSightings,
            string cultureCode)
        {
            foreach (var sighting in processedSightings)
            {
                TranslateLocalizedValue(sighting.Occurrence?.ActivityId, FieldMappingFieldId.Activity, cultureCode);
                TranslateLocalizedValue(sighting.Occurrence?.GenderId, FieldMappingFieldId.Gender, cultureCode);
                TranslateLocalizedValue(sighting.Occurrence?.LifeStageId, FieldMappingFieldId.LifeStage, cultureCode);
                TranslateLocalizedValue(sighting.Occurrence?.OrganismQuantityUnitId, FieldMappingFieldId.Unit, cultureCode);
                TranslateLocalizedValue(sighting.Event?.BiotopeId, FieldMappingFieldId.Biotope, cultureCode);
                TranslateLocalizedValue(sighting.Event?.SubstrateId, FieldMappingFieldId.Substrate, cultureCode);
                TranslateLocalizedValue(sighting.Identification?.ValidationStatusId, FieldMappingFieldId.ValidationStatus, cultureCode);
            }
        }


        private void ProcessLocalizedFieldMappedReturnValues(
            IEnumerable<dynamic> processedSightings,
            string cultureCode)
        {
            try
            {
                foreach (var sighting in processedSightings)
                {
                    if (sighting is IDictionary<string, object> obs)
                    {
                        if (obs.TryGetValue(nameof(ProcessedSighting.Occurrence), out object occurrenceObject))
                        {
                            var occurrenceDictionary = occurrenceObject as IDictionary<string, object>;
                            TranslateLocalizedValue(occurrenceDictionary, FieldMappingFieldId.Activity, nameof(ProcessedSighting.Occurrence.ActivityId), cultureCode);
                            TranslateLocalizedValue(occurrenceDictionary, FieldMappingFieldId.Gender, nameof(ProcessedSighting.Occurrence.GenderId), cultureCode);
                            TranslateLocalizedValue(occurrenceDictionary, FieldMappingFieldId.LifeStage, nameof(ProcessedSighting.Occurrence.LifeStageId), cultureCode);
                            TranslateLocalizedValue(occurrenceDictionary, FieldMappingFieldId.Unit, nameof(ProcessedSighting.Occurrence.OrganismQuantityUnitId), cultureCode);
                        }

                        if (obs.TryGetValue(nameof(ProcessedSighting.Event), out object eventObject))
                        {
                            var eventDictionary = eventObject as IDictionary<string, object>;
                            TranslateLocalizedValue(eventDictionary, FieldMappingFieldId.Biotope, nameof(ProcessedSighting.Event.BiotopeId), cultureCode);
                            TranslateLocalizedValue(eventDictionary, FieldMappingFieldId.Substrate, nameof(ProcessedSighting.Event.SubstrateId), cultureCode);
                        }

                        if (obs.TryGetValue(nameof(ProcessedSighting.Identification), out object identificationObject))
                        {
                            var identificationDictionary = identificationObject as IDictionary<string, object>;
                            TranslateLocalizedValue(identificationDictionary, FieldMappingFieldId.ValidationStatus, nameof(ProcessedSighting.Identification.ValidationStatusId), cultureCode);
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
            IDictionary<string, object> sightingNode, 
            FieldMappingFieldId fieldMappingFieldId, 
            string fieldName)
        {
            if (sightingNode == null) return;

            if (sightingNode.ContainsKey(fieldName))
            {
                if (sightingNode[fieldName] is IDictionary<string, object> fieldNode && fieldNode.ContainsKey("Value") && fieldNode.ContainsKey("_id"))
                {
                    int id = (int)fieldNode["_id"];
                    if (id != FieldMappingConstants.NoMappingFoundCustomValueIsUsedId && _fieldMappingFactory.TryGetValue(fieldMappingFieldId, id, out var translatedValue))
                    {
                        fieldNode["Value"] = translatedValue;
                    }
                }
            }
        }
        private void ResolveFieldMappedValue(ProcessedFieldMapValue fieldMapValue, FieldMappingFieldId fieldMappingFieldId)
        {
            if (fieldMapValue == null) return;
            if (fieldMapValue.Id != FieldMappingConstants.NoMappingFoundCustomValueIsUsedId
                && _fieldMappingFactory.TryGetValue(fieldMappingFieldId, fieldMapValue.Id, out var translatedValue))
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
                && _fieldMappingFactory.TryGetTranslatedValue(fieldMappingFieldId, cultureCode, fieldMapValue.Id, out var translatedValue))
            {
                fieldMapValue.Value = translatedValue;
            }
        }

        private void TranslateLocalizedValue(
            IDictionary<string, object> sightingNode, 
            FieldMappingFieldId fieldMappingFieldId, 
            string fieldName,
            string cultureCode)
        {
            if (sightingNode == null) return;

            if (sightingNode.ContainsKey(fieldName))
            {
                if (sightingNode[fieldName] is IDictionary<string, object> fieldNode && fieldNode.ContainsKey("Value") && fieldNode.ContainsKey("_id"))
                {
                    int id = (int)fieldNode["_id"];
                    if (id != FieldMappingConstants.NoMappingFoundCustomValueIsUsedId && _fieldMappingFactory.TryGetTranslatedValue(fieldMappingFieldId, cultureCode, id, out var translatedValue))
                    {
                        fieldNode["Value"] = translatedValue;
                    }
                }
            }
        }
    }
}