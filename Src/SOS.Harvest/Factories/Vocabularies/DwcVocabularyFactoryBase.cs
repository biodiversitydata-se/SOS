﻿using SOS.Harvest.Extensions;
using SOS.Harvest.Factories.Vocabularies.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using System.Text.RegularExpressions;

namespace SOS.Harvest.Factories.Vocabularies
{
    public abstract class DwcVocabularyFactoryBase : IVocabularyFactory
    {
        protected abstract VocabularyId FieldId { get; }
        protected abstract bool Localized { get; }

        public virtual async Task<Lib.Models.Shared.Vocabulary> CreateVocabularyAsync()
        {
            var vocabularyValues = GetVocabularyValues();

            return await Task.Run(() =>
            {
                var vocabulary = new Lib.Models.Shared.Vocabulary
                {
                    Id = FieldId,
                    Name = FieldId.ToString(),
                    Localized = Localized,
                    Values = vocabularyValues,
                    ExternalSystemsMapping = GetExternalSystemMappings(vocabularyValues)
                };

                return vocabulary;
            });
        }

        protected abstract ICollection<VocabularyValueInfo> GetVocabularyValues();
        protected abstract Dictionary<string, int>? GetMappingSynonyms();

        protected virtual List<ExternalSystemMapping> GetExternalSystemMappings(
            ICollection<VocabularyValueInfo> vocabularyValues)
        {
            return new List<ExternalSystemMapping>
            {
                GetDwcExternalSystemMapping(vocabularyValues)
            };
        }

        protected virtual ExternalSystemMapping GetDwcExternalSystemMapping(
            ICollection<VocabularyValueInfo> vocabularyValues)
        {
            var mappingSynonyms = GetMappingSynonyms();
            var dwcMapping = new ExternalSystemMapping
            {
                Id = ExternalSystemId.DarwinCore,
                Name = ExternalSystemId.DarwinCore.ToString(),
                Description = "The Darwin Core format (https://dwc.tdwg.org/terms/)",
                Mappings = new List<ExternalSystemMappingField>()
            };

            var term = FieldId.ToString().ToLowerFirstChar();
            var mappingField = new ExternalSystemMappingField
            {
                Key = term,
                Description = $"The {term} term (http://rs.tdwg.org/dwc/terms/{term})",
                Values = new List<ExternalSystemMappingValue>()
            };

            foreach (var vocabularyValue in vocabularyValues)
            {
                mappingField.Values.Add(new ExternalSystemMappingValue
                {
                    Value = vocabularyValue.Value,
                    SosId = vocabularyValue.Id
                });

                var stringVariations = GetStringVariations(vocabularyValue.Value);
                foreach (var stringVariation in stringVariations)
                {
                    mappingField.Values.Add(new ExternalSystemMappingValue
                    {
                        Value = stringVariation,
                        SosId = vocabularyValue.Id
                    });
                }

                if (mappingSynonyms != null)
                {
                    foreach (var keyValuePair in mappingSynonyms.Where(pair => pair.Value == vocabularyValue.Id))
                    {
                        mappingField.Values.Add(new ExternalSystemMappingValue
                        {
                            Value = keyValuePair.Key,
                            SosId = vocabularyValue.Id
                        });
                    }
                }
            }

            dwcMapping.Mappings.Add(mappingField);
            return dwcMapping;
        }

        private List<string> GetStringVariations(string str)
        {
            var stringVariations = new List<string>();
            var wordParts = SplitCamelCaseString(str);
            if (wordParts.Length > 1)
            {
                var strVariation = string.Join(" ", wordParts);
                if (strVariation != str)
                {
                    stringVariations.Add(strVariation);
                }
            }

            return stringVariations;
        }

        private string[] SplitCamelCaseString(string str)
        {
            return Regex.Matches(str, "(^[a-z]+|[A-Z]+(?![a-z])|[A-Z][a-z]+)")
                .OfType<Match>()
                .Select(m => m.Value)
                .ToArray();
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