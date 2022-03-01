using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SOS.Harvest.Entities.Artportalen;
using SOS.Harvest.Extensions;
using SOS.Harvest.Factories.Vocabularies.Interfaces;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;

namespace SOS.Harvest.Factories.Vocabularies
{
    public abstract class ArtportalenVocabularyFactoryBase : IVocabularyFactory
    {
        protected abstract VocabularyId FieldId { get; }
        protected abstract bool Localized { get; }

        public virtual async Task<Vocabulary> CreateVocabularyAsync()
        {
            var vocabularyValues = await GetVocabularyValues();

            var vocabulary = new Vocabulary
            {
                Id = FieldId,
                Name = FieldId.ToString(),
                Localized = Localized,
                Values = vocabularyValues,
                ExternalSystemsMapping = GetExternalSystemMappings(vocabularyValues)
            };

            return vocabulary;
        }

        protected abstract Task<ICollection<VocabularyValueInfo>> GetVocabularyValues();

        protected virtual ICollection<VocabularyValueInfo> ConvertToLocalizedVocabularyValues(
            ICollection<MetadataEntity> metadataEntities)
        {
            metadataEntities.TrimValues();
            var vocabularyValues = new List<VocabularyValueInfo>(metadataEntities.Count());
            foreach (var group in metadataEntities.GroupBy(m => m.Id))
            {
                var swedishRecord = group.Single(m => m.CultureCode == Cultures.sv_SE);
                var englishRecord = group.Single(m => m.CultureCode == Cultures.en_GB);
                var val = new VocabularyValueInfo
                {
                    Id = group.Key,
                    Value = string.IsNullOrWhiteSpace(englishRecord.Translation) ? "empty" : englishRecord.Translation,
                    Localized = true,
                    Translations = new List<VocabularyValueTranslation>
                    {
                        new VocabularyValueTranslation
                        {
                            CultureCode = swedishRecord.CultureCode, Value = swedishRecord.Translation
                        },
                        new VocabularyValueTranslation
                        {
                            CultureCode = englishRecord.CultureCode, Value = englishRecord.Translation
                        }
                    }
                };

                vocabularyValues.Add(val);
            }

            return vocabularyValues;
        }

        protected virtual ICollection<VocabularyValueInfo> ConvertToNonLocalizedVocabularyValues(
            ICollection<MetadataEntity> metadataEntities)
        {
            metadataEntities.TrimValues();
            var vocabularyValues = new List<VocabularyValueInfo>(metadataEntities.Count());
            foreach (var metadataEntity in metadataEntities)
            {
                vocabularyValues.Add(new VocabularyValueInfo
                {
                    Id = metadataEntity.Id,
                    Value = metadataEntity.Translation,
                    Localized = false
                });
            }

            return vocabularyValues;
        }


        protected virtual ICollection<VocabularyValueInfo> ConvertToVocabularyValuesWithCategory(
            ICollection<MetadataWithCategoryEntity> metadataWithCategoryEntities)
        {
            metadataWithCategoryEntities.TrimValues();
            var vocabularyValues = new List<VocabularyValueInfo>(metadataWithCategoryEntities.Count());
            foreach (var group in metadataWithCategoryEntities.GroupBy(m => m.Id))
            {
                var swedishRecord = group.Single(m => m.CultureCode == Cultures.sv_SE);
                var englishRecord = group.Single(m => m.CultureCode == Cultures.en_GB);
                var val = new VocabularyValueInfo
                {
                    Id = group.Key,
                    Value = string.IsNullOrWhiteSpace(englishRecord.Translation) ? "empty" : englishRecord.Translation,
                    Localized = true,
                    Translations =
                        new List<VocabularyValueTranslation>
                        {
                            new VocabularyValueTranslation
                            {
                                CultureCode = swedishRecord.CultureCode, Value = swedishRecord.Translation
                            },
                            new VocabularyValueTranslation
                            {
                                CultureCode = englishRecord.CultureCode, Value = englishRecord.Translation
                            }
                        },
                    Category = new VocabularyValueInfoCategory
                    {
                        Id = swedishRecord.CategoryId,
                        Name = englishRecord.CategoryName,
                        Localized = true,
                        Translations = new List<VocabularyValueTranslation>
                        {
                            new VocabularyValueTranslation
                            {
                                CultureCode = swedishRecord.CultureCode,
                                Value = swedishRecord.CategoryName
                            },
                            new VocabularyValueTranslation
                            {
                                CultureCode = englishRecord.CultureCode,
                                Value = englishRecord.CategoryName
                            }
                        }
                    }
                };

                vocabularyValues.Add(val);
            }

            return vocabularyValues;
        }


        protected virtual List<ExternalSystemMapping> GetExternalSystemMappings(
            ICollection<VocabularyValueInfo> vocabularyValues)
        {
            return new List<ExternalSystemMapping>
            {
                GetArtportalenExternalSystemMapping(vocabularyValues)
            };
        }


        protected virtual ExternalSystemMapping GetArtportalenExternalSystemMapping(
            ICollection<VocabularyValueInfo> vocabularyValues)
        {
            var artportalenMapping = new ExternalSystemMapping
            {
                Id = ExternalSystemId.Artportalen,
                Name = ExternalSystemId.Artportalen.ToString(),
                Mappings = new List<ExternalSystemMappingField>()
            };

            var mappingField = new ExternalSystemMappingField
            {
                Key = VocabularyMappingKeyFields.Id,
                Description = "Id field mapping",
                Values = new List<ExternalSystemMappingValue>()
            };

            // 1-1 mapping between Id fields.
            foreach (var vocabularyValue in vocabularyValues.Where(f => !f.IsCustomValue))
            {
                mappingField.Values.Add(new ExternalSystemMappingValue
                {
                    Value = vocabularyValue.Id,
                    SosId = vocabularyValue.Id
                });
            }

            artportalenMapping.Mappings.Add(mappingField);
            return artportalenMapping;
        }

        protected Dictionary<string, int> CreateDwcMappings(ICollection<VocabularyValueInfo> vocabularyValues, Dictionary<string, string> dwcMappingSynonyms)
        {
            Dictionary<string, int> sosIdByText = new Dictionary<string, int>();

            foreach (var vocabularyValue in vocabularyValues)
            {
                foreach (var translation in vocabularyValue.Translations)
                {
                    if (!string.IsNullOrWhiteSpace(translation.Value))
                    {
                        if (!sosIdByText.ContainsKey(translation.Value))
                        {
                            sosIdByText.Add(translation.Value, vocabularyValue.Id);
                        }
                    }
                }

                foreach (var keyValuePair in dwcMappingSynonyms.Where(pair => pair.Value == vocabularyValue.Value))
                {
                    if (!sosIdByText.ContainsKey(keyValuePair.Key))
                    {
                        sosIdByText.Add(keyValuePair.Key, vocabularyValue.Id);
                    }
                }
            }

            return sosIdByText;
        }

        protected VocabularyValueInfo CreateVocabularyValue(int id, string value)
        {
            return new VocabularyValueInfo
            {
                Id = id,
                Value = value,
                Localized = true,
                Translations = CreateTranslation(value),
                IsCustomValue = true
            };
        }

        protected VocabularyValueInfo CreateVocabularyValue(int id, string english, string swedish)
        {
            return new VocabularyValueInfo
            {
                Id = id,
                Value = english,
                Localized = true,
                Translations = CreateTranslation(english, swedish),
                IsCustomValue = true
            };
        }

        protected List<VocabularyValueTranslation> CreateTranslation(string english, string swedish)
        {
            return new List<VocabularyValueTranslation>
            {
                new VocabularyValueTranslation {CultureCode = "sv-SE", Value = swedish},
                new VocabularyValueTranslation {CultureCode = "en-GB", Value = english}
            };
        }

        protected List<VocabularyValueTranslation> CreateTranslation(string value)
        {
            return new List<VocabularyValueTranslation>
            {
                new VocabularyValueTranslation {CultureCode = "sv-SE", Value = value},
                new VocabularyValueTranslation {CultureCode = "en-GB", Value = value}
            };
        }
    }
}