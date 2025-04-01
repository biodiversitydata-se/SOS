using SOS.Lib.Configuration.Process;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Repositories.Resource.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace SOS.Lib.Helpers
{
    /// <summary>
    ///     Class that can be used for resolve vocabulary values.
    /// </summary>
    public class VocabularyValueResolver : Interfaces.IVocabularyValueResolver
    {
        private readonly VocabularyConfiguration _vocabularyConfiguration;
        private readonly IVocabularyRepository _processedVocabularyRepository;
        private Dictionary<string, Dictionary<VocabularyId, Dictionary<int, string>>>
            _valueMappingDictionariesByCultureCode;

        private async Task InitializeAsync()
        {
            var vocabularies = await _processedVocabularyRepository.GetAllAsync();
            _valueMappingDictionariesByCultureCode =
                new Dictionary<string, Dictionary<VocabularyId, Dictionary<int, string>>>
                {
                    {
                        Cultures.en_GB,
                        vocabularies.ToDictionary(vocabulary => vocabulary.Id,
                            m => m.CreateValueDictionary(Cultures.en_GB))
                    },
                    {
                        Cultures.sv_SE,
                        vocabularies.ToDictionary(vocabulary => vocabulary.Id,
                            m => m.CreateValueDictionary(Cultures.sv_SE))
                    }
                };

            // Add empty dictionary if vocabulary is missing.
            foreach (VocabularyId vocabularyId in Enum.GetValues(typeof(VocabularyId)))
            {
                if (!_valueMappingDictionariesByCultureCode[Cultures.en_GB].ContainsKey(vocabularyId))
                {
                    _valueMappingDictionariesByCultureCode[Cultures.en_GB].Add(vocabularyId, null);
                }
                if (!_valueMappingDictionariesByCultureCode[Cultures.sv_SE].ContainsKey(vocabularyId))
                {
                    _valueMappingDictionariesByCultureCode[Cultures.sv_SE].Add(vocabularyId, null);
                }
            }
        }

        private void ResolveVocabularyMappedValue(
            VocabularyValue vocabularyValue,
            Dictionary<int, string> valueById)
        {
            if (vocabularyValue == null || valueById == null) return;
            if (vocabularyValue.Id != VocabularyConstants.NoMappingFoundCustomValueIsUsedId
                && valueById.TryGetValue(vocabularyValue.Id, out var translatedValue))
            {
                vocabularyValue.Value = translatedValue;
            }
        }

        private void ResolveVocabularyMappedValues(
            JsonNode obs,
            string cultureCode)
        {
            var valueMappingDictionaries = _valueMappingDictionariesByCultureCode[cultureCode];

            // Record level
            ResolveVocabularyMappedValue(obs, nameof(Observation.BasisOfRecord), valueMappingDictionaries[VocabularyId.BasisOfRecord]);
            ResolveVocabularyMappedValue(obs, nameof(Observation.Type), valueMappingDictionaries[VocabularyId.Type]);
            ResolveVocabularyMappedValue(obs, nameof(Observation.AccessRights), valueMappingDictionaries[VocabularyId.AccessRights]);
            ResolveVocabularyMappedValue(obs, nameof(Observation.InstitutionCode), valueMappingDictionaries[VocabularyId.Institution]);

            // Event
            var eventObject = obs["event"];
            if (eventObject != null)
            {
                ResolveVocabularyMappedValue(eventObject, nameof(Observation.Event.DiscoveryMethod), valueMappingDictionaries[VocabularyId.DiscoveryMethod]);
            }

            // Identification
            var identificationObject = obs["identification"];
            if (identificationObject != null)
            {
                //  ResolveVocabularyMappedValue(identificationDictionary, nameof(Observation.Identification.ValidationStatus), valueMappingDictionaries[VocabularyId.VerificationStatus]);
                ResolveVocabularyMappedValue(identificationObject, nameof(Observation.Identification.VerificationStatus), valueMappingDictionaries[VocabularyId.VerificationStatus]);
                ResolveVocabularyMappedValue(identificationObject, nameof(Observation.Identification.DeterminationMethod), valueMappingDictionaries[VocabularyId.DeterminationMethod]);
            }

            // Occurrence
            var occurrenceObject = obs["occurrence"];
            if (occurrenceObject != null)
            {
                ResolveVocabularyMappedValue(occurrenceObject, nameof(Observation.Occurrence.Activity), valueMappingDictionaries[VocabularyId.Activity]);
                ResolveVocabularyMappedValue(occurrenceObject, nameof(Observation.Occurrence.Behavior), valueMappingDictionaries[VocabularyId.Behavior]);
                ResolveVocabularyMappedValue(occurrenceObject, nameof(Observation.Occurrence.EstablishmentMeans), valueMappingDictionaries[VocabularyId.EstablishmentMeans]);
                ResolveVocabularyMappedValue(occurrenceObject, nameof(Observation.Occurrence.Sex), valueMappingDictionaries[VocabularyId.Sex]);
                ResolveVocabularyMappedValue(occurrenceObject, nameof(Observation.Occurrence.ReproductiveCondition), valueMappingDictionaries[VocabularyId.ReproductiveCondition]);
                ResolveVocabularyMappedValue(occurrenceObject, nameof(Observation.Occurrence.LifeStage), valueMappingDictionaries[VocabularyId.LifeStage]);
                ResolveVocabularyMappedValue(occurrenceObject, nameof(Observation.Occurrence.OrganismQuantityUnit), valueMappingDictionaries[VocabularyId.Unit]);
                ResolveVocabularyMappedValue(occurrenceObject, nameof(Observation.Occurrence.OccurrenceStatus), valueMappingDictionaries[VocabularyId.OccurrenceStatus]);

                // Occurrence.Substrate
                var substrateObject = occurrenceObject["substrate"];
                if (substrateObject != null)
                {
                    ResolveVocabularyMappedValue(substrateObject, nameof(Observation.Occurrence.Substrate.Name), valueMappingDictionaries[VocabularyId.Substrate]);
                }
            }

            // Location
            var locationObject = obs["location"];
            if (locationObject != null)
            {
                ResolveVocabularyMappedValue(locationObject, nameof(Observation.Location.Continent), valueMappingDictionaries[VocabularyId.Continent]);
                ResolveVocabularyMappedValue(locationObject, nameof(Observation.Location.Country), valueMappingDictionaries[VocabularyId.Country]);
            }

            // Taxon
            var taxonObject = obs["taxon"];
            if (taxonObject != null)
            {
                var taxonDictionary = taxonObject as IDictionary<string, object>;

                // Taxon.Attributes
                var taxonAttributesObject = taxonObject["attributes"];
                if (taxonAttributesObject != null)
                {
                    //   ResolveVocabularyMappedValue(taxonAttributesDictionary, nameof(Observation.Taxon.Attributes.ProtectionLevel), valueMappingDictionaries[VocabularyId.SensitivityCategory]);
                    ResolveVocabularyMappedValue(taxonAttributesObject, nameof(Observation.Taxon.Attributes.SensitivityCategory), valueMappingDictionaries[VocabularyId.SensitivityCategory]);
                    ResolveVocabularyMappedValue(taxonAttributesObject, nameof(Observation.Taxon.Attributes.TaxonCategory), valueMappingDictionaries[VocabularyId.TaxonCategory]);
                }
            }
        }

        private void ResolveVocabularyMappedValue(
            JsonNode observationNode,
            string fieldName,
            Dictionary<int, string> valueById)
        {
            if (observationNode == null || valueById == null) return;
            var camelCaseName = fieldName.ToCamelCase();

            var node = observationNode[camelCaseName];
            if (node != null)
            {
                var id = (long?)node["id"];
                if (id.HasValue) {
                    if (id != VocabularyConstants.NoMappingFoundCustomValueIsUsedId &&
                            valueById.TryGetValue((int)id, out var translatedValue))
                    {
                        node["value"] = translatedValue;
                    }
                }
            }
        }

        public VocabularyValueResolver(
            IVocabularyRepository processedVocabularyRepository,
            VocabularyConfiguration vocabularyConfiguration)
        {
            _processedVocabularyRepository = processedVocabularyRepository ??
                                               throw new ArgumentNullException(nameof(processedVocabularyRepository));
            _vocabularyConfiguration = vocabularyConfiguration ??
                                         throw new ArgumentNullException(nameof(vocabularyConfiguration));
            Task.Run(InitializeAsync).Wait();
        }

        public VocabularyConfiguration Configuration => _vocabularyConfiguration;

        public void ResolveVocabularyMappedValues(IEnumerable<Observation> processedObservations, bool forceResolve = false)
        {
            ResolveVocabularyMappedValues(processedObservations, _vocabularyConfiguration.LocalizationCultureCode, forceResolve);
        }

        public void ResolveVocabularyMappedValues(IEnumerable<Observation> processedObservations,
            string cultureCode,
            bool forceResolve = false)
        {
            // If culture equals swedish (default), no need to translate
            if (!forceResolve && ((cultureCode?.Equals(Cultures.sv_SE, StringComparison.CurrentCultureIgnoreCase) ?? false) || !_vocabularyConfiguration.ResolveValues))
            {
                return;
            }

            if (string.IsNullOrEmpty(cultureCode))
            {
                cultureCode = Cultures.en_GB;
            }

            var valueMappingDictionaries = _valueMappingDictionariesByCultureCode[cultureCode];

            foreach (var observation in processedObservations)
            {
                ResolveVocabularyMappedValue(observation.BasisOfRecord,
                    valueMappingDictionaries[VocabularyId.BasisOfRecord]);
                ResolveVocabularyMappedValue(observation.Type, valueMappingDictionaries[VocabularyId.Type]);
                ResolveVocabularyMappedValue(observation.AccessRights,
                    valueMappingDictionaries[VocabularyId.AccessRights]);
                ResolveVocabularyMappedValue(observation.InstitutionCode,
                    valueMappingDictionaries[VocabularyId.Institution]);
                ResolveVocabularyMappedValue(observation.Location?.Country,
                    valueMappingDictionaries[VocabularyId.Country]);
                ResolveVocabularyMappedValue(observation.Location?.Continent,
                    valueMappingDictionaries[VocabularyId.Continent]);
                ResolveVocabularyMappedValue(observation.Occurrence?.Biotope,
                    valueMappingDictionaries[VocabularyId.Biotope]);
                ResolveVocabularyMappedValue(observation.Event?.DiscoveryMethod,
                    valueMappingDictionaries[VocabularyId.DiscoveryMethod]);
                /* ResolveVocabularyMappedValue(observation.Identification?.ValidationStatus,
                     valueMappingDictionaries[VocabularyId.VerificationStatus]);*/
                ResolveVocabularyMappedValue(observation.Identification?.VerificationStatus,
                    valueMappingDictionaries[VocabularyId.VerificationStatus]);
                ResolveVocabularyMappedValue(observation.Occurrence?.LifeStage,
                    valueMappingDictionaries[VocabularyId.LifeStage]);
                ResolveVocabularyMappedValue(observation.Occurrence?.Activity,
                    valueMappingDictionaries[VocabularyId.Activity]);
                ResolveVocabularyMappedValue(observation.Occurrence?.Sex,
                    valueMappingDictionaries[VocabularyId.Sex]);
                ResolveVocabularyMappedValue(observation.Occurrence?.Substrate?.Name,
                    valueMappingDictionaries[VocabularyId.Substrate]);
                ResolveVocabularyMappedValue(observation.Occurrence?.ReproductiveCondition,
                    valueMappingDictionaries[VocabularyId.ReproductiveCondition]);
                ResolveVocabularyMappedValue(observation.Occurrence?.Behavior,
                    valueMappingDictionaries[VocabularyId.Behavior]);
                ResolveVocabularyMappedValue(observation.Occurrence?.OrganismQuantityUnit,
                    valueMappingDictionaries[VocabularyId.Unit]);
                ResolveVocabularyMappedValue(observation.Occurrence?.EstablishmentMeans,
                    valueMappingDictionaries[VocabularyId.EstablishmentMeans]);
                ResolveVocabularyMappedValue(observation.Occurrence?.OccurrenceStatus,
                    valueMappingDictionaries[VocabularyId.OccurrenceStatus]);
                ResolveVocabularyMappedValue(observation.Identification?.DeterminationMethod,
                    valueMappingDictionaries[VocabularyId.DeterminationMethod]);
                /*ResolveVocabularyMappedValue(observation.Taxon?.Attributes?.ProtectionLevel,
                    valueMappingDictionaries[VocabularyId.SensitivityCategory]);*/
                ResolveVocabularyMappedValue(observation.Taxon?.Attributes?.SensitivityCategory,
                    valueMappingDictionaries[VocabularyId.SensitivityCategory]);
                ResolveVocabularyMappedValue(observation.Taxon?.Attributes.TaxonCategory,
                    valueMappingDictionaries[VocabularyId.TaxonCategory]);
            }
        }

        public void ResolveVocabularyMappedValues(IEnumerable<JsonNode> processedRecords,
            string cultureCode,
            bool forceResolve = false)
        {
            // If culture equals swedish (default), no need to translate
            if (!forceResolve && ((cultureCode?.Equals(Cultures.sv_SE, StringComparison.CurrentCultureIgnoreCase) ?? false) || !_vocabularyConfiguration.ResolveValues))
            {
                return;
            }

            if (string.IsNullOrEmpty(cultureCode))
            {
                cultureCode = "en-GB";
            }

            foreach (var record in processedRecords)
            {
                ResolveVocabularyMappedValues(record, cultureCode);
            }
        }
    }
}