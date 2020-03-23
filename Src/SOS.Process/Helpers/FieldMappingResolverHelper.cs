using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.Observation;
using SOS.Process.Repositories.Destination.Interfaces;

namespace SOS.Process.Helpers
{
    /// <summary>
    /// Class that can be used for resolve field mapped values.
    /// </summary>
    public class FieldMappingResolverHelper : Interfaces.IFieldMappingResolverHelper
    {
        private readonly IProcessedFieldMappingRepository _processedFieldMappingRepository;
        private Dictionary<string, Dictionary<FieldMappingFieldId, Dictionary<int, string>>> _valueMappingDictionariesByCultureCode;
        private readonly FieldMappingConfiguration _fieldMappingConfiguration;
        public FieldMappingResolverHelper(
            IProcessedFieldMappingRepository processedFieldMappingRepository,
            FieldMappingConfiguration fieldMappingConfiguration)
        {
            _processedFieldMappingRepository = processedFieldMappingRepository ?? throw new ArgumentNullException(nameof(processedFieldMappingRepository));
            _fieldMappingConfiguration = fieldMappingConfiguration ?? throw new ArgumentNullException(nameof(fieldMappingConfiguration));
            if (fieldMappingConfiguration.ResolveValues)
            {
                Task.Run(InitializeAsync).Wait();
            }
        }

        private async Task InitializeAsync()
        {
            var fieldMappings = await _processedFieldMappingRepository.GetAllAsync();
            _valueMappingDictionariesByCultureCode =
                new Dictionary<string, Dictionary<FieldMappingFieldId, Dictionary<int, string>>>
                {
                    {Cultures.en_GB, fieldMappings.ToDictionary(fieldMapping => fieldMapping.Id, m => m.CreateValueDictionary(Cultures.en_GB))},
                    {Cultures.sv_SE, fieldMappings.ToDictionary(fieldMapping => fieldMapping.Id, m => m.CreateValueDictionary(Cultures.sv_SE))}
                };
        }

        public void ResolveFieldMappedValues(IEnumerable<ProcessedObservation> processedObservations)
        {
            ResolveFieldMappedValues(processedObservations, _fieldMappingConfiguration.LocalizationCultureCode);
        }

        public void ResolveFieldMappedValues(IEnumerable<ProcessedObservation> processedObservations, string cultureCode)
        {
            if (!_fieldMappingConfiguration.ResolveValues) return;
            var valueMappingDictionaries = _valueMappingDictionariesByCultureCode[cultureCode];

            foreach (var observation in processedObservations)
            {
                ResolveFieldMappedValue(observation.BasisOfRecordId, valueMappingDictionaries[FieldMappingFieldId.BasisOfRecord]);
                ResolveFieldMappedValue(observation.TypeId, valueMappingDictionaries[FieldMappingFieldId.Type]);
                ResolveFieldMappedValue(observation.AccessRightsId, valueMappingDictionaries[FieldMappingFieldId.AccessRights]);
                ResolveFieldMappedValue(observation.InstitutionId, valueMappingDictionaries[FieldMappingFieldId.Institution]);
                ResolveFieldMappedValue(observation.Location?.CountyId, valueMappingDictionaries[FieldMappingFieldId.County]);
                ResolveFieldMappedValue(observation.Location?.MunicipalityId, valueMappingDictionaries[FieldMappingFieldId.Municipality]);
                ResolveFieldMappedValue(observation.Location?.ParishId, valueMappingDictionaries[FieldMappingFieldId.Parish]);
                ResolveFieldMappedValue(observation.Location?.ProvinceId, valueMappingDictionaries[FieldMappingFieldId.Province]);
                ResolveFieldMappedValue(observation.Location?.CountryId, valueMappingDictionaries[FieldMappingFieldId.Country]);
                ResolveFieldMappedValue(observation.Location?.ContinentId, valueMappingDictionaries[FieldMappingFieldId.Continent]);
                ResolveFieldMappedValue(observation.Event?.BiotopeId, valueMappingDictionaries[FieldMappingFieldId.Biotope]);
                ResolveFieldMappedValue(observation.Event?.SubstrateId, valueMappingDictionaries[FieldMappingFieldId.Substrate]);
                ResolveFieldMappedValue(observation.Identification?.ValidationStatusId, valueMappingDictionaries[FieldMappingFieldId.ValidationStatus]);
                ResolveFieldMappedValue(observation.Occurrence?.LifeStageId, valueMappingDictionaries[FieldMappingFieldId.LifeStage]);
                ResolveFieldMappedValue(observation.Occurrence?.ActivityId, valueMappingDictionaries[FieldMappingFieldId.Activity]);
                ResolveFieldMappedValue(observation.Occurrence?.GenderId, valueMappingDictionaries[FieldMappingFieldId.Gender]);
                ResolveFieldMappedValue(observation.Occurrence?.OrganismQuantityUnitId, valueMappingDictionaries[FieldMappingFieldId.Unit]);
                ResolveFieldMappedValue(observation.Occurrence?.EstablishmentMeansId, valueMappingDictionaries[FieldMappingFieldId.EstablishmentMeans]);
                ResolveFieldMappedValue(observation.Occurrence?.OccurrenceStatusId, valueMappingDictionaries[FieldMappingFieldId.OccurrenceStatus]);
            }
        }

        private void ResolveFieldMappedValue(
            ProcessedFieldMapValue fieldMapValue,
            Dictionary<int, string> valueById)
        {
            if (fieldMapValue == null) return;
            if (fieldMapValue.Id != FieldMappingConstants.NoMappingFoundCustomValueIsUsedId
                && valueById.TryGetValue(fieldMapValue.Id, out var translatedValue))
            {
                fieldMapValue.Value = translatedValue;
            }
        }
    }
}