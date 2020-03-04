using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SOS.Import.Entities;
using SOS.Import.Factories.FieldMappings.Interfaces;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;

namespace SOS.Import.Factories.FieldMappings
{
    public abstract class FieldMappingFactoryBase : IFieldMappingCreatorFactory
    {
        protected abstract FieldMappingFieldId FieldId { get; }
        //protected abstract Task<IEnumerable<MetadataEntity>> GetMetadataEntitiesCore();
        protected abstract Task<ICollection<FieldMappingValue>> GetFieldMappingValues();
        public virtual async Task<FieldMapping> CreateFieldMappingAsync()
        {
            return await CreateLocalizedFieldMappingAsync();

        }

        protected virtual async Task<FieldMapping> CreateLocalizedFieldMappingAsync()
        {
            //IEnumerable<MetadataEntity> metadataEntities = await GetMetadataEntitiesCore();
            var fieldMappingValues = await GetFieldMappingValues(); // ConvertToFieldMappingValues(metadataEntities.ToArray());

            FieldMapping fieldMapping = new FieldMapping
            {
                Id = FieldId,
                Name = FieldId.ToString(),
                Localized = true,
                Values = fieldMappingValues,
                ExternalSystemsMapping = GetExternalSystemMappings(fieldMappingValues)
            };

            return fieldMapping;
        }

        protected virtual ICollection<FieldMappingValue> ConvertToFieldMappingValues(ICollection<MetadataEntity> metadataEntities)
        {
            List<FieldMappingValue> fieldMappingValues = new List<FieldMappingValue>(metadataEntities.Count());
            foreach (var group in metadataEntities.GroupBy(m => m.Id))
            {
                var swedishRecord = group.Single(m => m.CultureCode == Cultures.sv_SE);
                var englishRecord = group.Single(m => m.CultureCode == Cultures.en_GB);
                var val = new FieldMappingValue
                {
                    Id = group.Key,
                    Name = string.IsNullOrWhiteSpace(englishRecord.Translation) ? "empty" : englishRecord.Translation,
                    Localized = true,
                    Translations = new List<FieldMappingTranslation>
                    {
                        new FieldMappingTranslation
                        {
                            CultureCode = swedishRecord.CultureCode, Value = swedishRecord.Translation
                        },
                        new FieldMappingTranslation
                        {
                            CultureCode = englishRecord.CultureCode, Value = englishRecord.Translation
                        }
                    }
                };

                fieldMappingValues.Add(val);
            }

            return fieldMappingValues;
        }

        protected virtual ICollection<FieldMappingValue> ConvertToFieldMappingValuesWithCategory(ICollection<MetadataWithCategoryEntity> metadataWithCategoryEntities)
        {
            List<FieldMappingValue> fieldMappingValues = new List<FieldMappingValue>(metadataWithCategoryEntities.Count());
            foreach (var group in metadataWithCategoryEntities.GroupBy(m => m.Id))
            {
                var swedishRecord = group.Single(m => m.CultureCode == Cultures.sv_SE);
                var englishRecord = group.Single(m => m.CultureCode == Cultures.en_GB);
                var val = new FieldMappingValue
                {
                    Id = group.Key,
                    Name = string.IsNullOrWhiteSpace(englishRecord.Translation) ? "empty" : englishRecord.Translation,
                    Localized = true,
                    Translations =
                        new List<FieldMappingTranslation>
                        {
                            new FieldMappingTranslation()
                            {
                                CultureCode = swedishRecord.CultureCode, Value = swedishRecord.Translation
                            },
                            new FieldMappingTranslation()
                            {
                                CultureCode = englishRecord.CultureCode, Value = englishRecord.Translation
                            }
                        },
                    Category = new FieldMappingValueCategory
                    {
                        Id = swedishRecord.CategoryId,
                        Name = englishRecord.CategoryName,
                        Localized = true,
                        Translations = new List<FieldMappingTranslation>
                        {
                            new FieldMappingTranslation()
                            {
                                CultureCode = swedishRecord.CultureCode,
                                Value = swedishRecord.CategoryName
                            },
                            new FieldMappingTranslation()
                            {
                                CultureCode = englishRecord.CultureCode,
                                Value = englishRecord.CategoryName
                            }
                        }
                    }
                };

                fieldMappingValues.Add(val);
            }

            return fieldMappingValues;
        }


        protected virtual List<ExternalSystemMapping> GetExternalSystemMappings(ICollection<FieldMappingValue> fieldMappingValues)
        {
            return new List<ExternalSystemMapping>()
            {
                GetArtportalenExternalSystemMapping(fieldMappingValues)
            };
        }


        protected virtual ExternalSystemMapping GetArtportalenExternalSystemMapping(ICollection<FieldMappingValue> fieldMappingValues)
        {
            ExternalSystemMapping artportalenMapping = new ExternalSystemMapping
            {
                Id = ExternalSystemId.Artportalen,
                Name = ExternalSystemId.Artportalen.ToString(),
                Mappings = new List<ExternalSystemMappingField>()
            };

            ExternalSystemMappingField mappingField = new ExternalSystemMappingField
            {
                Key = FieldMappingKeyFields.Id,
                Description = "Id field mapping",
                Values = new List<ExternalSystemMappingValue>()
            };

            // 1-1 mapping between Id fields.
            foreach (var fieldMappingValue in fieldMappingValues)
            {
                mappingField.Values.Add(new ExternalSystemMappingValue
                {
                    Value = fieldMappingValue.Id,
                    SosId = fieldMappingValue.Id
                });
            }

            artportalenMapping.Mappings.Add(mappingField);
            return artportalenMapping;
        }
    }
}