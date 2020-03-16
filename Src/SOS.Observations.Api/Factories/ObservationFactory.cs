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
    public class ObservationFactory : Interfaces.IObservationFactory
    {
        private readonly IProcessedObservationRepository _processedObservationRepository;
        private readonly IFieldMappingFactory _fieldMappingFactory;
        private readonly ILogger<ObservationFactory> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="processedObservationRepository"></param>
        /// <param name="fieldMappingFactory"></param>
        /// <param name="logger"></param>
        public ObservationFactory(
            IProcessedObservationRepository processedObservationRepository,
            IFieldMappingFactory fieldMappingFactory,
            ILogger<ObservationFactory> logger)
        {
            _processedObservationRepository = processedObservationRepository ?? throw new ArgumentNullException(nameof(processedObservationRepository));
            _fieldMappingFactory = fieldMappingFactory ?? throw new ArgumentNullException(nameof(fieldMappingFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<IEnumerable<dynamic>> GetChunkAsync(SearchFilter filter, int skip, int take)
        {
            try
            {
                var processedObservations = (await _processedObservationRepository.GetChunkAsync(filter, skip, take)).ToArray();
                ProcessLocalizedFieldMappings(filter, processedObservations);
                ProcessNonLocalizedFieldMappings(filter, processedObservations);
                return processedObservations;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get chunk of sightings");
                return null;
            }
        }

        private void ProcessNonLocalizedFieldMappings(SearchFilter filter, IEnumerable<object> processedObservations)
        {
            if (!filter.TranslateFieldMappedValues) return;

            if (filter.OutputFields == null || !filter.OutputFields.Any()) // ProcessedObservation objects is returned wen OutputFields is not used.
            {
                var sightings = processedObservations.Cast<ProcessedObservation>();
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
                foreach (var sighting in processedObservations)
                {
                    if (sighting is IDictionary<string, object> obs)
                    {
                        ResolveFieldMappedValue(obs, FieldMappingFieldId.BasisOfRecord, nameof(ProcessedObservation.BasisOfRecordId));
                        ResolveFieldMappedValue(obs, FieldMappingFieldId.Type, nameof(ProcessedObservation.TypeId));
                        ResolveFieldMappedValue(obs, FieldMappingFieldId.AccessRights, nameof(ProcessedObservation.AccessRightsId));
                        ResolveFieldMappedValue(obs, FieldMappingFieldId.Institution, nameof(ProcessedObservation.InstitutionId));

                        if (obs.TryGetValue(nameof(ProcessedObservation.Location), out object locationObject))
                        {
                            var locationDictionary = locationObject as IDictionary<string, object>;
                            ResolveFieldMappedValue(locationDictionary, FieldMappingFieldId.County, nameof(ProcessedObservation.Location.CountyId));
                            ResolveFieldMappedValue(locationDictionary, FieldMappingFieldId.Municipality, nameof(ProcessedObservation.Location.MunicipalityId));
                            ResolveFieldMappedValue(locationDictionary, FieldMappingFieldId.Province, nameof(ProcessedObservation.Location.ProvinceId));
                            ResolveFieldMappedValue(locationDictionary, FieldMappingFieldId.Parish, nameof(ProcessedObservation.Location.ParishId));
                        }

                        if (obs.TryGetValue(nameof(ProcessedObservation.Occurrence), out object occurrenceObject))
                        {
                            var occurrenceDictionary = occurrenceObject as IDictionary<string, object>;
                            ResolveFieldMappedValue(occurrenceDictionary, FieldMappingFieldId.EstablishmentMeans, nameof(ProcessedObservation.Occurrence.EstablishmentMeansId));
                            ResolveFieldMappedValue(occurrenceDictionary, FieldMappingFieldId.OccurrenceStatus, nameof(ProcessedObservation.Occurrence.OccurrenceStatusId));
                        }
                    }
                }
            }
        }

        private void ProcessLocalizedFieldMappings(SearchFilter filter, IEnumerable<dynamic> processedObservations)
        {
            if (!filter.TranslateFieldMappedValues) return;
            string cultureCode = filter.TranslationCultureCode;
            if (filter.OutputFields == null || !filter.OutputFields.Any()) // ProcessedObservation objects is returned wen OutputFields is not used.
            {
                var sightings = processedObservations.Cast<ProcessedObservation>();
                ProcessLocalizedFieldMappedReturnValues(sightings, cultureCode);
            }
            else // dynamic objects is returned when OutputFields is used
            {
                ProcessLocalizedFieldMappedReturnValues(processedObservations, cultureCode);
            }
        }

        private void ProcessLocalizedFieldMappedReturnValues(
            IEnumerable<ProcessedObservation> processedObservations,
            string cultureCode)
        {
            foreach (var sighting in processedObservations)
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
            IEnumerable<dynamic> processedObservations,
            string cultureCode)
        {
            try
            {
                foreach (var sighting in processedObservations)
                {
                    if (sighting is IDictionary<string, object> obs)
                    {
                        if (obs.TryGetValue(nameof(ProcessedObservation.Occurrence), out object occurrenceObject))
                        {
                            var occurrenceDictionary = occurrenceObject as IDictionary<string, object>;
                            TranslateLocalizedValue(occurrenceDictionary, FieldMappingFieldId.Activity, nameof(ProcessedObservation.Occurrence.ActivityId), cultureCode);
                            TranslateLocalizedValue(occurrenceDictionary, FieldMappingFieldId.Gender, nameof(ProcessedObservation.Occurrence.GenderId), cultureCode);
                            TranslateLocalizedValue(occurrenceDictionary, FieldMappingFieldId.LifeStage, nameof(ProcessedObservation.Occurrence.LifeStageId), cultureCode);
                            TranslateLocalizedValue(occurrenceDictionary, FieldMappingFieldId.Unit, nameof(ProcessedObservation.Occurrence.OrganismQuantityUnitId), cultureCode);
                        }

                        if (obs.TryGetValue(nameof(ProcessedObservation.Event), out object eventObject))
                        {
                            var eventDictionary = eventObject as IDictionary<string, object>;
                            TranslateLocalizedValue(eventDictionary, FieldMappingFieldId.Biotope, nameof(ProcessedObservation.Event.BiotopeId), cultureCode);
                            TranslateLocalizedValue(eventDictionary, FieldMappingFieldId.Substrate, nameof(ProcessedObservation.Event.SubstrateId), cultureCode);
                        }

                        if (obs.TryGetValue(nameof(ProcessedObservation.Identification), out object identificationObject))
                        {
                            var identificationDictionary = identificationObject as IDictionary<string, object>;
                            TranslateLocalizedValue(identificationDictionary, FieldMappingFieldId.ValidationStatus, nameof(ProcessedObservation.Identification.ValidationStatusId), cultureCode);
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