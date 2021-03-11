using System.Collections.Generic;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;

namespace SOS.Import.Factories.Vocabularies
{
    /// <summary>
    ///     Class for creating Type vocabulary.
    /// </summary>
    public class TypeVocabularyFactory : DwcVocabularyFactoryBase
    {
        protected override VocabularyId FieldId => VocabularyId.Type;
        protected override bool Localized => false;

        protected override ICollection<VocabularyValueInfo> GetVocabularyValues()
        {
            var vocabularyValues = new List<VocabularyValueInfo>
            {
                new VocabularyValueInfo {Id = 0, Value = "Collection"},
                new VocabularyValueInfo {Id = 1, Value = "Dataset"},
                new VocabularyValueInfo {Id = 2, Value = "Event"},
                new VocabularyValueInfo {Id = 3, Value = "Image"},
                new VocabularyValueInfo {Id = 4, Value = "InteractiveResource"},
                new VocabularyValueInfo {Id = 5, Value = "MovingImage"},
                new VocabularyValueInfo {Id = 6, Value = "PhysicalObject"},
                new VocabularyValueInfo {Id = 7, Value = "Service"},
                new VocabularyValueInfo {Id = 8, Value = "Software"},
                new VocabularyValueInfo {Id = 9, Value = "Sound"},
                new VocabularyValueInfo {Id = 10, Value = "StillImage"},
                new VocabularyValueInfo {Id = 11, Value = "Text"}
            };

            return vocabularyValues;
        }

        protected override Dictionary<string, int> GetMappingSynonyms()
        {
            return null;
        }
    }
}