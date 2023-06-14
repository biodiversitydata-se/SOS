using System.Collections.Generic;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;

namespace SOS.Harvest.Factories.Vocabularies
{
    /// <summary>
    ///     Class for creating EstablishmentMeans vocabulary.
    /// </summary>
    public class EstablishmentMeansVocabularyFactory : DwcVocabularyFactoryBase
    {
        protected override VocabularyId FieldId => VocabularyId.EstablishmentMeans;
        protected override bool Localized => false;

        protected override ICollection<VocabularyValueInfo> GetVocabularyValues()
        {
            // Vocabulary from http://rs.gbif.org/vocabulary/gbif/establishment_means.xml.
            var vocabularyValues = new List<VocabularyValueInfo>
            {
                new VocabularyValueInfo {Id = 0, Value = "native"},
                new VocabularyValueInfo {Id = 1, Value = "introduced"},
                new VocabularyValueInfo {Id = 2, Value = "naturalised"},
                new VocabularyValueInfo {Id = 3, Value = "invasive"},
                new VocabularyValueInfo {Id = 4, Value = "managed"},
                new VocabularyValueInfo {Id = 5, Value = "uncertain"}
            };

            return vocabularyValues;
        }

        protected override Dictionary<string, int>? GetMappingSynonyms()
        {
            return null;
        }
    }
}