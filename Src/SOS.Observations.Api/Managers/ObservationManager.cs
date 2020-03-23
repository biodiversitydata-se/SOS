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
    /// Observation manager class
    /// </summary>
    public class ObservationManager : Interfaces.IObservationManager
    {
        private readonly IProcessedObservationRepository _processedObservationRepository;
        private readonly IFieldMappingManager _fieldMappingManager;
        private readonly ILogger<ObservationManager> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="processedObservationRepository"></param>
        /// <param name="fieldMappingManager"></param>
        /// <param name="logger"></param>
        public ObservationManager(
            IProcessedObservationRepository processedObservationRepository,
            IFieldMappingManager fieldMappingManager,
            ILogger<ObservationManager> logger)
        {
            _processedObservationRepository = processedObservationRepository ?? throw new ArgumentNullException(nameof(processedObservationRepository));
            _fieldMappingManager = fieldMappingManager ?? throw new ArgumentNullException(nameof(fieldMappingManager));
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
                _logger.LogError(e, "Failed to get chunk of observations");
                return null;
            }
        }

        private void ProcessNonLocalizedFieldMappings(SearchFilter filter, IEnumerable<object> processedObservations)
        {
            if (!filter.TranslateFieldMappedValues) return;

            if (filter.OutputFields == null || !filter.OutputFields.Any()) // ProcessedObservation objects is returned wen OutputFields is not used.
            {
                var observations = processedObservations.Cast<ProcessedObservation>();
                foreach (var observation in observations)
                {
                    ResolveFieldMappedValue(observation.BasisOfRecordId, FieldMappingFieldId.BasisOfRecord);
                    ResolveFieldMappedValue(observation.TypeId, FieldMappingFieldId.Type);
                    ResolveFieldMappedValue(observation.AccessRightsId, FieldMappingFieldId.AccessRights);
                    ResolveFieldMappedValue(observation.InstitutionId, FieldMappingFieldId.Institution);
                    ResolveFieldMappedValue(observation.Location?.County, FieldMappingFieldId.County);
                    ResolveFieldMappedValue(observation.Location?.Municipality, FieldMappingFieldId.Municipality);
                    ResolveFieldMappedValue(observation.Location?.Province, FieldMappingFieldId.Province);
                    ResolveFieldMappedValue(observation.Location?.Parish, FieldMappingFieldId.Parish);
                    ResolveFieldMappedValue(observation.Location?.Country, FieldMappingFieldId.Country);
                    ResolveFieldMappedValue(observation.Location?.Continent, FieldMappingFieldId.Continent);
                    ResolveFieldMappedValue(observation.Occurrence?.EstablishmentMeans, FieldMappingFieldId.EstablishmentMeans);
                    ResolveFieldMappedValue(observation.Occurrence?.OccurrenceStatus, FieldMappingFieldId.OccurrenceStatus);
                }
            }
            else // dynamic objects is returned when OutputFields is used
            {
                foreach (var observation in processedObservations)
                {
                    if (observation is IDictionary<string, object> obs)
                    {
                        ResolveFieldMappedValue(obs, FieldMappingFieldId.BasisOfRecord, nameof(ProcessedObservation.BasisOfRecordId));
                        ResolveFieldMappedValue(obs, FieldMappingFieldId.Type, nameof(ProcessedObservation.TypeId));
                        ResolveFieldMappedValue(obs, FieldMappingFieldId.AccessRights, nameof(ProcessedObservation.AccessRightsId));
                        ResolveFieldMappedValue(obs, FieldMappingFieldId.Institution, nameof(ProcessedObservation.InstitutionId));

                        if (obs.TryGetValue(nameof(ProcessedObservation.Location), out object locationObject))
                        {
                            var locationDictionary = locationObject as IDictionary<string, object>;
                            ResolveFieldMappedValue(locationDictionary, FieldMappingFieldId.County, nameof(ProcessedObservation.Location.County));
                            ResolveFieldMappedValue(locationDictionary, FieldMappingFieldId.Municipality, nameof(ProcessedObservation.Location.Municipality));
                            ResolveFieldMappedValue(locationDictionary, FieldMappingFieldId.Province, nameof(ProcessedObservation.Location.Province));
                            ResolveFieldMappedValue(locationDictionary, FieldMappingFieldId.Parish, nameof(ProcessedObservation.Location.Parish));
                        }

                        if (obs.TryGetValue(nameof(ProcessedObservation.Occurrence), out object occurrenceObject))
                        {
                            var occurrenceDictionary = occurrenceObject as IDictionary<string, object>;
                            ResolveFieldMappedValue(occurrenceDictionary, FieldMappingFieldId.EstablishmentMeans, nameof(ProcessedObservation.Occurrence.EstablishmentMeans));
                            ResolveFieldMappedValue(occurrenceDictionary, FieldMappingFieldId.OccurrenceStatus, nameof(ProcessedObservation.Occurrence.OccurrenceStatus));
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
                var observations = processedObservations.Cast<ProcessedObservation>();
                ProcessLocalizedFieldMappedReturnValues(observations, cultureCode);
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
            foreach (var observation in processedObservations)
            {
                TranslateLocalizedValue(observation.Occurrence?.Activity, FieldMappingFieldId.Activity, cultureCode);
                TranslateLocalizedValue(observation.Occurrence?.Gender, FieldMappingFieldId.Gender, cultureCode);
                TranslateLocalizedValue(observation.Occurrence?.LifeStage, FieldMappingFieldId.LifeStage, cultureCode);
                TranslateLocalizedValue(observation.Occurrence?.OrganismQuantityUnit, FieldMappingFieldId.Unit, cultureCode);
                TranslateLocalizedValue(observation.Event?.Biotope, FieldMappingFieldId.Biotope, cultureCode);
                TranslateLocalizedValue(observation.Event?.Substrate, FieldMappingFieldId.Substrate, cultureCode);
                TranslateLocalizedValue(observation.Identification?.ValidationStatusId, FieldMappingFieldId.ValidationStatus, cultureCode);
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
                        if (obs.TryGetValue(nameof(ProcessedObservation.Occurrence), out object occurrenceObject))
                        {
                            var occurrenceDictionary = occurrenceObject as IDictionary<string, object>;
                            TranslateLocalizedValue(occurrenceDictionary, FieldMappingFieldId.Activity, nameof(ProcessedObservation.Occurrence.Activity), cultureCode);
                            TranslateLocalizedValue(occurrenceDictionary, FieldMappingFieldId.Gender, nameof(ProcessedObservation.Occurrence.Gender), cultureCode);
                            TranslateLocalizedValue(occurrenceDictionary, FieldMappingFieldId.LifeStage, nameof(ProcessedObservation.Occurrence.LifeStage), cultureCode);
                            TranslateLocalizedValue(occurrenceDictionary, FieldMappingFieldId.Unit, nameof(ProcessedObservation.Occurrence.OrganismQuantityUnit), cultureCode);
                        }

                        if (obs.TryGetValue(nameof(ProcessedObservation.Event), out object eventObject))
                        {
                            var eventDictionary = eventObject as IDictionary<string, object>;
                            TranslateLocalizedValue(eventDictionary, FieldMappingFieldId.Biotope, nameof(ProcessedObservation.Event.Biotope), cultureCode);
                            TranslateLocalizedValue(eventDictionary, FieldMappingFieldId.Substrate, nameof(ProcessedObservation.Event.Substrate), cultureCode);
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
            IDictionary<string, object> observationNode, 
            FieldMappingFieldId fieldMappingFieldId, 
            string fieldName)
        {
            if (observationNode == null) return;

            if (observationNode.ContainsKey(fieldName))
            {
                if (observationNode[fieldName] is IDictionary<string, object> fieldNode && fieldNode.ContainsKey("Value") && fieldNode.ContainsKey("_id"))
                {
                    int id = (int)fieldNode["_id"];
                    if (id != FieldMappingConstants.NoMappingFoundCustomValueIsUsedId && _fieldMappingManager.TryGetValue(fieldMappingFieldId, id, out var translatedValue))
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
                && _fieldMappingManager.TryGetTranslatedValue(fieldMappingFieldId, cultureCode, fieldMapValue.Id, out var translatedValue))
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

            if (observationNode.ContainsKey(fieldName))
            {
                if (observationNode[fieldName] is IDictionary<string, object> fieldNode && fieldNode.ContainsKey("Value") && fieldNode.ContainsKey("_id"))
                {
                    int id = (int)fieldNode["_id"];
                    if (id != FieldMappingConstants.NoMappingFoundCustomValueIsUsedId && _fieldMappingManager.TryGetTranslatedValue(fieldMappingFieldId, cultureCode, id, out var translatedValue))
                    {
                        fieldNode["Value"] = translatedValue;
                    }
                }
            }
        }
    }
}