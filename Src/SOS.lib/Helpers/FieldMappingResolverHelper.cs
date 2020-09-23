//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using SOS.Lib.Configuration.Process;
//using SOS.Lib.Constants;
//using SOS.Lib.Enums;
//using SOS.Lib.Models.Processed.Observation;
//using SOS.Lib.Repositories.Processed.Interfaces;

//namespace SOS.Lib.Helpers
//{
//    /// <summary>
//    ///     Class that can be used for resolve field mapped values.
//    /// </summary>
//    public class FieldMappingResolverHelper
//    {
//        private readonly FieldMappingConfiguration _fieldMappingConfiguration;
//        private readonly IProcessedFieldMappingRepository _processedFieldMappingRepository;
//        private Dictionary<string, Dictionary<FieldMappingFieldId, Dictionary<int, string>>>
//            _valueMappingDictionariesByCultureCode;

//        public FieldMappingResolverHelper(
//            IProcessedFieldMappingRepository processedFieldMappingRepository,
//            FieldMappingConfiguration fieldMappingConfiguration)
//        {
//            _processedFieldMappingRepository = processedFieldMappingRepository ??
//                                               throw new ArgumentNullException(nameof(processedFieldMappingRepository));
//            _fieldMappingConfiguration = fieldMappingConfiguration ??
//                                         throw new ArgumentNullException(nameof(fieldMappingConfiguration));
//            Task.Run(InitializeAsync).Wait();
//        }

//        public FieldMappingConfiguration Configuration => _fieldMappingConfiguration;

//        public void ResolveFieldMappedValues(IEnumerable<ProcessedObservation> processedObservations)
//        {
//            ResolveFieldMappedValues(processedObservations, _fieldMappingConfiguration.LocalizationCultureCode);
//        }

//        public void ResolveFieldMappedValues(IEnumerable<ProcessedObservation> processedObservations,
//            string cultureCode)
//        {
//            if (!_fieldMappingConfiguration.ResolveValues) return;
//            var valueMappingDictionaries = _valueMappingDictionariesByCultureCode[cultureCode];

//            foreach (var observation in processedObservations)
//            {
//                ResolveFieldMappedValue(observation.BasisOfRecord,
//                    valueMappingDictionaries[FieldMappingFieldId.BasisOfRecord]);
//                ResolveFieldMappedValue(observation.Type, valueMappingDictionaries[FieldMappingFieldId.Type]);
//                ResolveFieldMappedValue(observation.AccessRights,
//                    valueMappingDictionaries[FieldMappingFieldId.AccessRights]);
//                ResolveFieldMappedValue(observation.InstitutionCode,
//                    valueMappingDictionaries[FieldMappingFieldId.Institution]);
//                ResolveFieldMappedValue(observation.Location?.County,
//                    valueMappingDictionaries[FieldMappingFieldId.County]);
//                ResolveFieldMappedValue(observation.Location?.Municipality,
//                    valueMappingDictionaries[FieldMappingFieldId.Municipality]);
//                ResolveFieldMappedValue(observation.Location?.Parish,
//                    valueMappingDictionaries[FieldMappingFieldId.Parish]);
//                ResolveFieldMappedValue(observation.Location?.Province,
//                    valueMappingDictionaries[FieldMappingFieldId.Province]);
//                ResolveFieldMappedValue(observation.Location?.Country,
//                    valueMappingDictionaries[FieldMappingFieldId.Country]);
//                ResolveFieldMappedValue(observation.Location?.Continent,
//                    valueMappingDictionaries[FieldMappingFieldId.Continent]);
//                ResolveFieldMappedValue(observation.Event?.Biotope,
//                    valueMappingDictionaries[FieldMappingFieldId.Biotope]);
//                ResolveFieldMappedValue(observation.Event?.Substrate,
//                    valueMappingDictionaries[FieldMappingFieldId.Substrate]);
//                ResolveFieldMappedValue(observation.Identification?.ValidationStatus,
//                    valueMappingDictionaries[FieldMappingFieldId.ValidationStatus]);
//                ResolveFieldMappedValue(observation.Occurrence?.LifeStage,
//                    valueMappingDictionaries[FieldMappingFieldId.LifeStage]);
//                ResolveFieldMappedValue(observation.Occurrence?.Activity,
//                    valueMappingDictionaries[FieldMappingFieldId.Activity]);
//                ResolveFieldMappedValue(observation.Occurrence?.Gender,
//                    valueMappingDictionaries[FieldMappingFieldId.Gender]);
//                ResolveFieldMappedValue(observation.Occurrence?.OrganismQuantityUnit,
//                    valueMappingDictionaries[FieldMappingFieldId.Unit]);
//                ResolveFieldMappedValue(observation.Occurrence?.EstablishmentMeans,
//                    valueMappingDictionaries[FieldMappingFieldId.EstablishmentMeans]);
//                ResolveFieldMappedValue(observation.Occurrence?.OccurrenceStatus,
//                    valueMappingDictionaries[FieldMappingFieldId.OccurrenceStatus]);
//                ResolveFieldMappedValue(observation.Occurrence?.DiscoveryMethod, 
//                    valueMappingDictionaries[FieldMappingFieldId.DiscoveryMethod]);
//                ResolveFieldMappedValue(observation.Identification?.DeterminationMethod,
//                    valueMappingDictionaries[FieldMappingFieldId.DeterminationMethod]);
//            }
//        }

//        private async Task InitializeAsync()
//        {
//            var fieldMappings = await _processedFieldMappingRepository.GetAllAsync();
//            _valueMappingDictionariesByCultureCode =
//                new Dictionary<string, Dictionary<FieldMappingFieldId, Dictionary<int, string>>>
//                {
//                    {
//                        Cultures.en_GB,
//                        fieldMappings.ToDictionary(fieldMapping => fieldMapping.Id,
//                            m => m.CreateValueDictionary(Cultures.en_GB))
//                    },
//                    {
//                        Cultures.sv_SE,
//                        fieldMappings.ToDictionary(fieldMapping => fieldMapping.Id,
//                            m => m.CreateValueDictionary(Cultures.sv_SE))
//                    }
//                };
//        }

//        private void ResolveFieldMappedValue(
//            ProcessedFieldMapValue fieldMapValue,
//            Dictionary<int, string> valueById)
//        {
//            if (fieldMapValue == null) return;
//            if (fieldMapValue.Id != FieldMappingConstants.NoMappingFoundCustomValueIsUsedId
//                && valueById.TryGetValue(fieldMapValue.Id, out var translatedValue))
//            {
//                fieldMapValue.Value = translatedValue;
//            }
//        }
//    }

//    public class FieldMappingResolver
//    {

//    }
//}