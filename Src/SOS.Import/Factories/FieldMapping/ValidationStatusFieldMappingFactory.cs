using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SOS.Import.Repositories.Source.Artportalen.Interfaces;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;

namespace SOS.Import.Factories.FieldMapping
{
    /// <summary>
    ///     Class for creating verification status field mapping.
    /// </summary>
    public class ValidationStatusFieldMappingFactory : ArtportalenFieldMappingFactoryBase
    {
        private readonly IMetadataRepository _artportalenMetadataRepository;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="artportalenMetadataRepository"></param>
        public ValidationStatusFieldMappingFactory(IMetadataRepository artportalenMetadataRepository)
        {
            _artportalenMetadataRepository = artportalenMetadataRepository ??
                                             throw new ArgumentNullException(nameof(artportalenMetadataRepository));
        }

        protected override FieldMappingFieldId FieldId => FieldMappingFieldId.ValidationStatus;
        protected override bool Localized => true;

        protected override async Task<ICollection<FieldMappingValue>> GetFieldMappingValues()
        {
            var validationStatusList = await _artportalenMetadataRepository.GetValidationStatusAsync();
            var fieldMappingValues = base.ConvertToLocalizedFieldMappingValues(validationStatusList.ToArray());
            //int id = fieldMappingValues.Max(f => f.Id);
            fieldMappingValues.Add(CreateFieldMappingValue(0, "Verified", "Validerad"));
            fieldMappingValues = fieldMappingValues.OrderBy(f => f.Id).ToList();
            return fieldMappingValues;
        }

        protected override List<ExternalSystemMapping> GetExternalSystemMappings(
            ICollection<FieldMappingValue> fieldMappingValues)
        {
            return new List<ExternalSystemMapping>
            {
                GetArtportalenExternalSystemMapping(fieldMappingValues),
                GetDarwinCoreExternalSystemMapping(fieldMappingValues)
            };
        }

        private ExternalSystemMapping GetDarwinCoreExternalSystemMapping(
            ICollection<FieldMappingValue> fieldMappingValues)
        {
            var externalSystemMapping = new ExternalSystemMapping
            {
                Id = ExternalSystemId.DarwinCore,
                Name = ExternalSystemId.DarwinCore.ToString(),
                Description = "The Darwin Core format (https://dwc.tdwg.org/terms/)",
                Mappings = new List<ExternalSystemMappingField>()
            };

            var dwcMappingSynonyms = GetDwcMappingSynonyms();
            var dwcMappings = CreateDwcMappings(fieldMappingValues, dwcMappingSynonyms);
            var mappingField = new ExternalSystemMappingField
            {
                Key = FieldMappingKeyFields.DwcIdentificationVerificationStatus,
                Description = "The identificationVerificationStatus term (http://rs.tdwg.org/dwc/terms/identificationVerificationStatus)",
                Values = dwcMappings.Select(pair => new ExternalSystemMappingValue { Value = pair.Key, SosId = pair.Value }).ToList()
            };

            externalSystemMapping.Mappings.Add(mappingField);
            return externalSystemMapping;
        }

        private Dictionary<string, string> GetDwcMappingSynonyms()
        {
            return new Dictionary<string, string>
            {
                {"unverified", "Unvalidated"}
            };
        }
    }
}