using System.Collections.Generic;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;

namespace SOS.Import.Factories.Vocabularies
{
    /// <summary>
    ///     Class for creating Continent vocabulary.
    /// </summary>
    public class ContinentVocabularyFactory : DwcVocabularyFactoryBase
    {
        protected override VocabularyId FieldId => VocabularyId.Continent;
        protected override bool Localized => false;

        protected override ICollection<VocabularyValueInfo> GetVocabularyValues()
        {
            // Vocabulary from https://dwc.tdwg.org/terms/#dwc:continent
            var vocabularyValues = new List<VocabularyValueInfo>
            {
                new VocabularyValueInfo {Id = 0, Value = "Africa"},
                new VocabularyValueInfo {Id = 1, Value = "Antarctica"},
                new VocabularyValueInfo {Id = 2, Value = "Asia"},
                new VocabularyValueInfo {Id = 3, Value = "Oceania"},
                new VocabularyValueInfo {Id = 4, Value = "Europe"},
                new VocabularyValueInfo {Id = 5, Value = "North America"},
                new VocabularyValueInfo {Id = 6, Value = "South America"}
            };

            return vocabularyValues;
        }

        protected override Dictionary<string, int> GetMappingSynonyms()
        {
            return null;
        }
    }
}