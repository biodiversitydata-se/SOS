using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;

namespace SOS.Harvest.Factories.Vocabularies
{
    /// <summary>
    ///     Class for creating AccessRights vocabulary.
    /// </summary>
    public class AccessRightsVocabularyFactory : DwcVocabularyFactoryBase
    {
        protected override VocabularyId FieldId => VocabularyId.AccessRights;
        protected override bool Localized => false;

        protected override ICollection<VocabularyValueInfo> GetVocabularyValues()
        {
            var vocabularyValues = new List<VocabularyValueInfo>
            {
                new VocabularyValueInfo {Id = 0, Value = "Free usage"},
                new VocabularyValueInfo {Id = 1, Value = "Not for public usage"},
                new VocabularyValueInfo {Id = 2, Value = "CC0"},
                new VocabularyValueInfo {Id = 3, Value = "CC BY"},
                new VocabularyValueInfo {Id = 4, Value = "CC BY-NC"}
            };

            return vocabularyValues;
        }

        protected override Dictionary<string, int> GetMappingSynonyms()
        {
            return new Dictionary<string, int>
            {
                {"Free to use", 0}
            };
        }
    }
}