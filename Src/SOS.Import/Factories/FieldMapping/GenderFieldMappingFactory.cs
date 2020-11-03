using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Import.Repositories.Source.Artportalen.Interfaces;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;

namespace SOS.Import.Factories.FieldMapping
{
    /// <summary>
    ///     Class for creating Gender field mapping.
    /// </summary>
    public class GenderFieldMappingFactory : ArtportalenFieldMappingFactoryBase
    {
        private readonly IMetadataRepository _artportalenMetadataRepository;
        private readonly ILogger<GenderFieldMappingFactory> _logger;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="metadataRepository"></param>
        /// <param name="logger"></param>
        public GenderFieldMappingFactory(
            IMetadataRepository metadataRepository,
            ILogger<GenderFieldMappingFactory> logger)
        {
            _artportalenMetadataRepository =
                metadataRepository ?? throw new ArgumentNullException(nameof(metadataRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override FieldMappingFieldId FieldId => FieldMappingFieldId.Gender;
        protected override bool Localized => true;

        protected override async Task<ICollection<FieldMappingValue>> GetFieldMappingValues()
        {
            var genders = await _artportalenMetadataRepository.GetGendersAsync();
            var fieldMappingValues = base.ConvertToLocalizedFieldMappingValues(genders.ToArray());
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
                Key = FieldMappingKeyFields.DwcSex,
                Description = "The sex term (http://rs.tdwg.org/dwc/terms/sex)",
                Values = dwcMappings.Select(pair => new ExternalSystemMappingValue { Value = pair.Key, SosId = pair.Value }).ToList()
            };

            externalSystemMapping.Mappings.Add(mappingField);
            return externalSystemMapping;
        }

        private Dictionary<string, string> GetDwcMappingSynonyms()
        {
            return new Dictionary<string, string>
            {
                {"m", "male"},
                {"f", "female"},
            };
        }

    }
}