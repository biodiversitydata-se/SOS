using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SOS.Import.Entities;
using SOS.Import.Repositories.Destination.FieldMappings.Interfaces;
using SOS.Import.Repositories.Source.SpeciesPortal;
using SOS.Import.Repositories.Source.SpeciesPortal.Interfaces;
using SOS.Import.Services;
using SOS.Import.Services.Interfaces;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;

namespace SOS.Import.Factories.FieldMappings
{
    /// <summary>
    /// Class for creating Gender field mapping.
    /// </summary>
    public class GenderFieldMappingFactory : Interfaces.IGenderFieldMappingFactory
    {
        private readonly IMetadataRepository _metadataRepository;
        private readonly ILogger<GenderFieldMappingFactory> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="metadataRepository"></param>
        /// <param name="logger"></param>
        public GenderFieldMappingFactory(
            IMetadataRepository metadataRepository,
            ILogger<GenderFieldMappingFactory> logger)
        {
            _metadataRepository = metadataRepository ?? throw new ArgumentNullException(nameof(metadataRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<FieldMapping> CreateFieldMappingAsync()
        {
            IEnumerable<MetadataEntity> genderValues = await _metadataRepository.GetGendersAsync();
            ICollection<FieldMappingValue> fieldMappingValues = ConvertToFieldMappingValues(genderValues.ToArray());

            FieldMapping fieldMapping = new FieldMapping
            {
                Id = FieldMappingFieldId.Gender,
                Name = FieldMappingFieldId.Gender.ToString(),
                Localized = true,
                Values = fieldMappingValues,
                ExternalSystemsMapping = new List<ExternalSystemMapping>
                {
                    GetArtportalenExternalSystemMapping(fieldMappingValues),
                    GetDarwinCoreExternalSystemMapping(fieldMappingValues)
                }
            };

            return fieldMapping;
        }

        private ICollection<FieldMappingValue> ConvertToFieldMappingValues(ICollection<MetadataEntity> genderValues)
        {
            var fieldMappingValues = new List<FieldMappingValue>(genderValues.Count());
            foreach (var group in genderValues.GroupBy(m => m.Id))
            {
                var swedishRecord = group.Single(m => m.CultureCode == "sv-SE");
                var englishRecord = group.Single(m => m.CultureCode == "en-GB");
                var val = new FieldMappingValue();
                val.Id = group.Key;
                val.Name = string.IsNullOrWhiteSpace(englishRecord.Translation) ? "empty" : englishRecord.Translation;
                val.Localized = true;
                val.Translations = new List<FieldMappingTranslation>
                {
                    new FieldMappingTranslation()
                    {
                        CultureCode = swedishRecord.CultureCode, Value = swedishRecord.Translation
                    },
                    new FieldMappingTranslation()
                    {
                        CultureCode = englishRecord.CultureCode, Value = englishRecord.Translation
                    }
                };

                fieldMappingValues.Add(val);
            }

            return fieldMappingValues;
        }

        private ExternalSystemMapping GetArtportalenExternalSystemMapping(ICollection<FieldMappingValue> fieldMappingValues)
        {
            ExternalSystemMapping externalSystemMapping = new ExternalSystemMapping();
            externalSystemMapping.Id = ExternalSystemId.Artportalen;
            externalSystemMapping.Name = ExternalSystemId.Artportalen.ToString();
            externalSystemMapping.Mappings = new List<ExternalSystemMappingField>();

            ExternalSystemMappingField mappingField = new ExternalSystemMappingField();
            mappingField.Key = FieldMappingKeyFields.Id;
            mappingField.Description = "The Gender.Id field";
            mappingField.Values = new List<ExternalSystemMappingValue>();

            // 1-1 mapping between Id fields.
            foreach (var fieldMappingValue in fieldMappingValues)
            {
                mappingField.Values.Add(new ExternalSystemMappingValue
                {
                    Value = fieldMappingValue.Id,
                    SosId = fieldMappingValue.Id
                });
            }

            externalSystemMapping.Mappings.Add(mappingField);
            return externalSystemMapping;
        }

        private static ExternalSystemMapping GetDarwinCoreExternalSystemMapping(ICollection<FieldMappingValue> fieldMappingValues)
        {
            ExternalSystemMapping externalSystemMapping = new ExternalSystemMapping
            {
                Id = ExternalSystemId.DarwinCore,
                Name = ExternalSystemId.DarwinCore.ToString(),
                Description = "The Darwin Core format (https://dwc.tdwg.org/terms/)",
                Mappings = new List<ExternalSystemMappingField>()
            };

            ExternalSystemMappingField mappingField = new ExternalSystemMappingField
            {
                Key = FieldMappingKeyFields.DwcSex,
                Description = "The sex term (http://rs.tdwg.org/dwc/terms/sex)",
                Values = new List<ExternalSystemMappingValue>
                {
                    new ExternalSystemMappingValue
                    {
                        Value = "male", SosId = fieldMappingValues.Single(m => m.Name == "male").Id
                    },
                    new ExternalSystemMappingValue
                    {
                        Value = "female", SosId = fieldMappingValues.Single(m => m.Name == "female").Id
                    },
                    new ExternalSystemMappingValue
                    {
                        Value = "hermaphrodite",
                        SosId = fieldMappingValues.Single(m => m.Name == "hermaphroditic").Id
                    },
                    new ExternalSystemMappingValue
                    {
                        Value = "undetermined", SosId = fieldMappingValues.Single(m => m.Name == "empty").Id
                    }
                }
            };

            externalSystemMapping.Mappings.Add(mappingField);
            return externalSystemMapping;
        }
    }
}