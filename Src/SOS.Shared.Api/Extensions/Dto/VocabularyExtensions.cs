using SOS.Lib.Models.Shared;
using SOS.Shared.Api.Dtos.Vocabulary;

namespace SOS.Shared.Api.Extensions.Dto;

public static class VocabularyExtensions
{
    extension(VocabularyValueInfo vocabularyValue)
    {
        private VocabularyValueInfoDto ToVocabularyValueInfoDto()
        {
            return new VocabularyValueInfoDto
            {
                Id = vocabularyValue.Id,
                Value = vocabularyValue.Value,
                Description = vocabularyValue.Description,
                Localized = vocabularyValue.Localized,
                Category = vocabularyValue.Category == null
                    ? null!
                    : new VocabularyValueInfoCategoryDto
                    {
                        Id = vocabularyValue.Category.Id,
                        Name = vocabularyValue.Category.Name,
                        Description = vocabularyValue.Category.Description,
                        Localized = vocabularyValue.Category.Localized,
                        Translations = vocabularyValue.Category.Translations?.Select(
                            vocabularyValueCategoryTranslation => new VocabularyValueTranslationDto
                            {
                                CultureCode = vocabularyValueCategoryTranslation.CultureCode,
                                Value = vocabularyValueCategoryTranslation.Value
                            }).ToList()!
                    },
                Translations = vocabularyValue.Translations?.Select(vocabularyValueTranslation =>
                    new VocabularyValueTranslationDto
                    {
                        CultureCode = vocabularyValueTranslation.CultureCode,
                        Value = vocabularyValueTranslation.Value
                    }).ToList()!
            };
        }
    }

    extension(ExternalSystemMapping vocabularyExternalSystemsMapping)
    {
        private ExternalSystemMappingDto ToExternalSystemMappingDto(
)
        {
            return new ExternalSystemMappingDto
            {
                Id = (ExternalSystemIdDto)vocabularyExternalSystemsMapping.Id,
                Name = vocabularyExternalSystemsMapping.Name,
                Description = vocabularyExternalSystemsMapping.Description,
                Mappings = vocabularyExternalSystemsMapping.Mappings?.Select(vocabularyExternalSystemsMappingMapping =>
                    new ExternalSystemMappingFieldDto
                    {
                        Key = vocabularyExternalSystemsMappingMapping.Key,
                        Description = vocabularyExternalSystemsMappingMapping.Description,
                        Values = vocabularyExternalSystemsMappingMapping.Values?.Select(
                            vocabularyExternalSystemsMappingMappingValue => new ExternalSystemMappingValueDto
                            {
                                Value = vocabularyExternalSystemsMappingMappingValue.Value,
                                SosId = vocabularyExternalSystemsMappingMappingValue.SosId
                            }).ToList()!
                    }).ToList()!
            };
        }
    }

    extension(IEnumerable<Vocabulary> vocabularies)
    {
        public IEnumerable<VocabularyDto> ToVocabularyDtos(bool includeSystemMappings = true)
        {
            return vocabularies.Select(vocabulary => vocabulary.ToVocabularyDto(includeSystemMappings));
        }
    }

    extension(Vocabulary vocabulary)
    {
        public VocabularyDto ToVocabularyDto(bool includeSystemMappings = true)
        {
            return new VocabularyDto
            {
                Id = (int)(VocabularyIdDto)vocabulary.Id,
                EnumId = (VocabularyIdDto)vocabulary.Id,
                Name = vocabulary.Name,
                Description = vocabulary.Description,
                Localized = vocabulary.Localized,
                Values = vocabulary.Values.Select(val => val.ToVocabularyValueInfoDto()).ToList(),
                ExternalSystemsMapping = includeSystemMappings == false || vocabulary.ExternalSystemsMapping == null ?
                    null! :
                    vocabulary.ExternalSystemsMapping.Select(m => m.ToExternalSystemMappingDto()).ToList()
            };
        }
    }
}