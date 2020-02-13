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
using SOS.Import.Repositories.Source.SpeciesPortal.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;

namespace SOS.Import.Factories.FieldMappings
{
    /// <summary>
    /// Class for creating Activity field mapping.
    /// </summary>
    public class ActivityFieldMappingFactory : Interfaces.IActivityFieldMappingFactory
    {
        private readonly IMetadataRepository _metadataRepository;
        private readonly ILogger<ActivityFieldMappingFactory> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="metadataRepository"></param>
        /// <param name="logger"></param>
        public ActivityFieldMappingFactory(
            IMetadataRepository metadataRepository,
            ILogger<ActivityFieldMappingFactory> logger)
        {
            _metadataRepository = metadataRepository ?? throw new ArgumentNullException(nameof(metadataRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<FieldMapping> CreateFieldMappingAsync()
        {
            IEnumerable<MetadataWithCategoryEntity> activities = await _metadataRepository.GetActivitiesAsync();
            var fieldMappingValues = ConvertToFieldMappingValues(activities.ToArray());

            FieldMapping fieldMapping = new FieldMapping
            {
                Id = (int)FieldMappingFieldId.Activity,
                FieldMappingFieldId = FieldMappingFieldId.Activity,
                Name = FieldMappingFieldId.Activity.ToString(),
                Localized = true,
                Values = fieldMappingValues,
                ExternalSystemsMapping = new List<ExternalSystemMapping>
                {
                    GetExternalSystemMapping(fieldMappingValues)
                }
            };

            return fieldMapping;
        }

        private ICollection<FieldMappingValue> ConvertToFieldMappingValues(ICollection<MetadataWithCategoryEntity> activities)
        {
            List<FieldMappingValue> fieldMappingValues = new List<FieldMappingValue>(activities.Count());
            foreach (var group in activities.GroupBy(m => m.Id))
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

                val.Category = new FieldMappingValueCategory();
                val.Category.Id = swedishRecord.CategoryId;
                val.Category.Name = englishRecord.CategoryName;
                val.Category.Localized = true;
                val.Category.Translations = new List<FieldMappingTranslation>
                {
                    new FieldMappingTranslation()
                    {
                        CultureCode = swedishRecord.CultureCode, Value = swedishRecord.CategoryName
                    },
                    new FieldMappingTranslation()
                    {
                        CultureCode = englishRecord.CultureCode, Value = englishRecord.CategoryName
                    }
                };

                fieldMappingValues.Add(val);
            }

            return fieldMappingValues;
        }

        private ExternalSystemMapping GetExternalSystemMapping(ICollection<FieldMappingValue> fieldMappingValues)
        {
            ExternalSystemMapping externalSystemMapping = new ExternalSystemMapping();
            externalSystemMapping.Id = (int)VerbatimDataProviderTypeId.Artportalen;
            externalSystemMapping.VerbatimDataProviderTypeId = VerbatimDataProviderTypeId.Artportalen;
            externalSystemMapping.ExternalSystemId = ExternalSystemId.Artportalen;
            externalSystemMapping.Name = "Artportalen";
            externalSystemMapping.Mappings = new List<ExternalSystemMappingField>();

            ExternalSystemMappingField mappingField = new ExternalSystemMappingField();
            mappingField.Key = "Id";
            mappingField.Description = "The Activity.Id field";
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
    }
}