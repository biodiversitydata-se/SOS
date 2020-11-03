using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SOS.Import.Entities.Artportalen;
using SOS.Import.Extensions;
using SOS.Import.Factories.FieldMapping.Interfaces;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;

namespace SOS.Import.Factories.FieldMapping
{
    public abstract class ArtportalenFieldMappingFactoryBase : IFieldMappingCreatorFactory
    {
        protected abstract FieldMappingFieldId FieldId { get; }
        protected abstract bool Localized { get; }

        public virtual async Task<Lib.Models.Shared.FieldMapping> CreateFieldMappingAsync()
        {
            var fieldMappingValues = await GetFieldMappingValues();

            var fieldMapping = new Lib.Models.Shared.FieldMapping
            {
                Id = FieldId,
                Name = FieldId.ToString(),
                Localized = Localized,
                Values = fieldMappingValues,
                ExternalSystemsMapping = GetExternalSystemMappings(fieldMappingValues)
            };

            return fieldMapping;
        }

        protected abstract Task<ICollection<FieldMappingValue>> GetFieldMappingValues();

        protected virtual ICollection<FieldMappingValue> ConvertToLocalizedFieldMappingValues(
            ICollection<MetadataEntity> metadataEntities)
        {
            metadataEntities.TrimValues();
            var fieldMappingValues = new List<FieldMappingValue>(metadataEntities.Count());
            foreach (var group in metadataEntities.GroupBy(m => m.Id))
            {
                var swedishRecord = group.Single(m => m.CultureCode == Cultures.sv_SE);
                var englishRecord = group.Single(m => m.CultureCode == Cultures.en_GB);
                var val = new FieldMappingValue
                {
                    Id = group.Key,
                    Value = string.IsNullOrWhiteSpace(englishRecord.Translation) ? "empty" : englishRecord.Translation,
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

        protected virtual ICollection<FieldMappingValue> ConvertToNonLocalizedFieldMappingValues(
            ICollection<MetadataEntity> metadataEntities)
        {
            metadataEntities.TrimValues();
            var fieldMappingValues = new List<FieldMappingValue>(metadataEntities.Count());
            foreach (var metadataEntity in metadataEntities)
            {
                fieldMappingValues.Add(new FieldMappingValue
                {
                    Id = metadataEntity.Id,
                    Value = metadataEntity.Translation,
                    Localized = false
                });
            }

            return fieldMappingValues;
        }


        protected virtual ICollection<FieldMappingValue> ConvertToFieldMappingValuesWithCategory(
            ICollection<MetadataWithCategoryEntity> metadataWithCategoryEntities)
        {
            metadataWithCategoryEntities.TrimValues();
            var fieldMappingValues = new List<FieldMappingValue>(metadataWithCategoryEntities.Count());
            foreach (var group in metadataWithCategoryEntities.GroupBy(m => m.Id))
            {
                var swedishRecord = group.Single(m => m.CultureCode == Cultures.sv_SE);
                var englishRecord = group.Single(m => m.CultureCode == Cultures.en_GB);
                var val = new FieldMappingValue
                {
                    Id = group.Key,
                    Value = string.IsNullOrWhiteSpace(englishRecord.Translation) ? "empty" : englishRecord.Translation,
                    Localized = true,
                    Translations =
                        new List<FieldMappingTranslation>
                        {
                            new FieldMappingTranslation
                            {
                                CultureCode = swedishRecord.CultureCode, Value = swedishRecord.Translation
                            },
                            new FieldMappingTranslation
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
                            new FieldMappingTranslation
                            {
                                CultureCode = swedishRecord.CultureCode,
                                Value = swedishRecord.CategoryName
                            },
                            new FieldMappingTranslation
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


        protected virtual List<ExternalSystemMapping> GetExternalSystemMappings(
            ICollection<FieldMappingValue> fieldMappingValues)
        {
            return new List<ExternalSystemMapping>
            {
                GetArtportalenExternalSystemMapping(fieldMappingValues)
            };
        }


        protected virtual ExternalSystemMapping GetArtportalenExternalSystemMapping(
            ICollection<FieldMappingValue> fieldMappingValues)
        {
            var artportalenMapping = new ExternalSystemMapping
            {
                Id = ExternalSystemId.Artportalen,
                Name = ExternalSystemId.Artportalen.ToString(),
                Mappings = new List<ExternalSystemMappingField>()
            };

            var mappingField = new ExternalSystemMappingField
            {
                Key = FieldMappingKeyFields.Id,
                Description = "Id field mapping",
                Values = new List<ExternalSystemMappingValue>()
            };

            // 1-1 mapping between Id fields.
            foreach (var fieldMappingValue in fieldMappingValues.Where(f => !f.IsCustomValue))
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

        protected Dictionary<string, int> CreateDwcMappings(ICollection<FieldMappingValue> fieldMappingValues, Dictionary<string, string> dwcMappingSynonyms)
        {
            Dictionary<string, int> sosIdByText = new Dictionary<string, int>();

            foreach (var fieldMappingValue in fieldMappingValues)
            {
                if (fieldMappingValue.Value == "larva/nymph")
                {
                    int x = 8;
                }

                foreach (var fieldMappingTranslation in fieldMappingValue.Translations)
                {
                    if (!string.IsNullOrWhiteSpace(fieldMappingTranslation.Value))
                    {
                        if (!sosIdByText.ContainsKey(fieldMappingTranslation.Value))
                        {
                            sosIdByText.Add(fieldMappingTranslation.Value, fieldMappingValue.Id);
                        }
                    }
                }

                foreach (var keyValuePair in dwcMappingSynonyms.Where(pair => pair.Value == fieldMappingValue.Value))
                {
                    if (!sosIdByText.ContainsKey(keyValuePair.Key))
                    {
                        sosIdByText.Add(keyValuePair.Key, fieldMappingValue.Id);
                    }
                }
            }

            return sosIdByText;
        }

        protected FieldMappingValue CreateFieldMappingValue(int id, string value)
        {
            return new FieldMappingValue
            {
                Id = id,
                Value = value,
                Localized = true,
                Translations = CreateTranslation(value),
                IsCustomValue = true
            };
        }

        protected FieldMappingValue CreateFieldMappingValue(int id, string english, string swedish)
        {
            return new FieldMappingValue
            {
                Id = id,
                Value = english,
                Localized = true,
                Translations = CreateTranslation(english, swedish),
                IsCustomValue = true
            };
        }

        protected List<FieldMappingTranslation> CreateTranslation(string english, string swedish)
        {
            return new List<FieldMappingTranslation>
            {
                new FieldMappingTranslation {CultureCode = "sv-SE", Value = swedish},
                new FieldMappingTranslation {CultureCode = "en-GB", Value = english}
            };
        }

        protected List<FieldMappingTranslation> CreateTranslation(string value)
        {
            return new List<FieldMappingTranslation>
            {
                new FieldMappingTranslation {CultureCode = "sv-SE", Value = value},
                new FieldMappingTranslation {CultureCode = "en-GB", Value = value}
            };
        }
    }
}