using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.Sighting;
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
            var fieldMappings = (await _processedFieldMappingRepository.GetFieldMappingsAsync()).ToArray();
            _valueMappingDictionariesByCultureCode =
                new Dictionary<string, Dictionary<FieldMappingFieldId, Dictionary<int, string>>>
                {
                    {Cultures.en_GB, fieldMappings.ToDictionary(fieldMapping => fieldMapping.Id, m => m.CreateValueDictionary(Cultures.en_GB))},
                    {Cultures.sv_SE, fieldMappings.ToDictionary(fieldMapping => fieldMapping.Id, m => m.CreateValueDictionary(Cultures.sv_SE))}
                };
        }

        public void ResolveFieldMappedValues(IEnumerable<ProcessedSighting> processedSightings)
        {
            ResolveFieldMappedValues(processedSightings, _fieldMappingConfiguration.LocalizationCultureCode);
        }

        public void ResolveFieldMappedValues(IEnumerable<ProcessedSighting> processedSightings, string cultureCode)
        {
            if (!_fieldMappingConfiguration.ResolveValues) return;
            var valueMappingDictionaries = _valueMappingDictionariesByCultureCode[cultureCode];

            foreach (var processedSighting in processedSightings)
            {
                ResolveFieldMappedValue(processedSighting.BasisOfRecordId, valueMappingDictionaries[FieldMappingFieldId.BasisOfRecord]);
                ResolveFieldMappedValue(processedSighting.TypeId, valueMappingDictionaries[FieldMappingFieldId.Type]);
                ResolveFieldMappedValue(processedSighting.AccessRightsId, valueMappingDictionaries[FieldMappingFieldId.AccessRights]);
                ResolveFieldMappedValue(processedSighting.InstitutionId, valueMappingDictionaries[FieldMappingFieldId.Institution]);
                ResolveFieldMappedValue(processedSighting.Location?.CountyId, valueMappingDictionaries[FieldMappingFieldId.County]);
                ResolveFieldMappedValue(processedSighting.Location?.MunicipalityId, valueMappingDictionaries[FieldMappingFieldId.Municipality]);
                ResolveFieldMappedValue(processedSighting.Location?.ParishId, valueMappingDictionaries[FieldMappingFieldId.Parish]);
                ResolveFieldMappedValue(processedSighting.Location?.ProvinceId, valueMappingDictionaries[FieldMappingFieldId.Province]);
                ResolveFieldMappedValue(processedSighting.Location?.CountryId, valueMappingDictionaries[FieldMappingFieldId.Country]);
                ResolveFieldMappedValue(processedSighting.Location?.ContinentId, valueMappingDictionaries[FieldMappingFieldId.Continent]);
                ResolveFieldMappedValue(processedSighting.Event?.BiotopeId, valueMappingDictionaries[FieldMappingFieldId.Biotope]);
                ResolveFieldMappedValue(processedSighting.Event?.SubstrateId, valueMappingDictionaries[FieldMappingFieldId.Substrate]);
                ResolveFieldMappedValue(processedSighting.Identification?.ValidationStatusId, valueMappingDictionaries[FieldMappingFieldId.ValidationStatus]);
                ResolveFieldMappedValue(processedSighting.Occurrence?.LifeStageId, valueMappingDictionaries[FieldMappingFieldId.LifeStage]);
                ResolveFieldMappedValue(processedSighting.Occurrence?.ActivityId, valueMappingDictionaries[FieldMappingFieldId.Activity]);
                ResolveFieldMappedValue(processedSighting.Occurrence?.GenderId, valueMappingDictionaries[FieldMappingFieldId.Gender]);
                ResolveFieldMappedValue(processedSighting.Occurrence?.OrganismQuantityUnitId, valueMappingDictionaries[FieldMappingFieldId.Unit]);
                ResolveFieldMappedValue(processedSighting.Occurrence?.EstablishmentMeansId, valueMappingDictionaries[FieldMappingFieldId.EstablishmentMeans]);
                ResolveFieldMappedValue(processedSighting.Occurrence?.OccurrenceStatusId, valueMappingDictionaries[FieldMappingFieldId.OccurrenceStatus]);
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