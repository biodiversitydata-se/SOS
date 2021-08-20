using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Repositories.Resource.Interfaces;

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
            if (string.IsNullOrEmpty(cultureCode))
            {
                cultureCode = "en-GB";
            }

            if (!forceResolve && !_vocabularyConfiguration.ResolveValues) return;
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
                ResolveVocabularyMappedValue(observation.Identification?.ValidationStatus,
                    valueMappingDictionaries[VocabularyId.ValidationStatus]);
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
            }
        }

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
    }
}