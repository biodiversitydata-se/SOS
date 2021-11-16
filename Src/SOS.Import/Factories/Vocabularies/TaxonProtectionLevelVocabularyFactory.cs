using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Import.Repositories.Source.Artportalen.Interfaces;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;

namespace SOS.Import.Factories.Vocabularies
{
    /// <summary>
    ///     Class for creating TaxonProtectionLevel vocabulary.
    /// </summary>
    public class TaxonProtectionLevelVocabularyFactory : ArtportalenVocabularyFactoryBase
    {
        protected override VocabularyId FieldId => VocabularyId.TaxonSensitivityCategory;
        protected override bool Localized => true;

        protected override async Task<ICollection<VocabularyValueInfo>> GetVocabularyValues()
        {
            var vocabularyValues = new List<VocabularyValueInfo>();
            vocabularyValues.Add(CreateVocabularyValue(1, "1. Full access and free use for all", "1. Fullständig åtkomst och fri användning för alla"));
            vocabularyValues.Add(CreateVocabularyValue(3, "3. Generalized to a 5*5 km grid cell for public usage", "3. Maximal upplösning 5*5 km för allmänheten"));
            vocabularyValues.Add(CreateVocabularyValue(4, "4. Generalized to a 25*25 km grid cell for public usage", "4. Maximal upplösning 25*25 km för allmänheten"));
            vocabularyValues.Add(CreateVocabularyValue(5, "5. Generalized to a 50*50 km grid cell for public usage", "5. Maximal upplösning 50*50 km för allmänheten"));
            return vocabularyValues;
        }

        protected override List<ExternalSystemMapping> GetExternalSystemMappings(
           ICollection<VocabularyValueInfo> vocabularyValues)
        {
            return new List<ExternalSystemMapping>
            {
                //GetArtportalenExternalSystemMapping(vocabularyValues)
            };
        }
    }
}