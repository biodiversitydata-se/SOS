using System.Collections.Generic;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;

namespace SOS.Import.Factories.Vocabularies
{
    /// <summary>
    ///     Class for creating BasisOfRecord field mapping.
    /// </summary>
    public class BasisOfRecordVocabularyFactory : DwcVocabularyFactoryBase
    {
        protected override VocabularyId FieldId => VocabularyId.BasisOfRecord;
        protected override bool Localized => false;

        protected override ICollection<VocabularyValueInfo> GetVocabularyValues()
        {
            // Vocabulary from https://dwc.tdwg.org/terms/#dwc:basisOfRecord and GBIF website.
            var vocabularyValues = new List<VocabularyValueInfo>
            {
                new VocabularyValueInfo {Id = 0, Value = "HumanObservation"},
                new VocabularyValueInfo {Id = 1, Value = "PreservedSpecimen"},
                new VocabularyValueInfo {Id = 2, Value = "FossilSpecimen"},
                new VocabularyValueInfo {Id = 3, Value = "LivingSpecimen"},
                new VocabularyValueInfo {Id = 4, Value = "MaterialSample"},
                new VocabularyValueInfo {Id = 5, Value = "Event"},
                new VocabularyValueInfo {Id = 6, Value = "MachineObservation"},
                new VocabularyValueInfo {Id = 7, Value = "Taxon"},
                new VocabularyValueInfo {Id = 8, Value = "Occurrence"},
                new VocabularyValueInfo {Id = 9, Value = "Literature"},
                new VocabularyValueInfo {Id = 10, Value = "Unknown"}
            };

            return vocabularyValues;
        }

        protected override Dictionary<string, int> GetMappingSynonyms()
        {
            return null;
        }
    }
}