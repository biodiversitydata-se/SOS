﻿using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;

namespace SOS.Harvest.Factories.Vocabularies
{
    /// <summary>
    ///     Class for creating sensitivity category vocabulary.
    /// </summary>
    public class SensitivityCategoryVocabularyFactory : ArtportalenVocabularyFactoryBase
    {
        protected override VocabularyId FieldId => VocabularyId.SensitivityCategory;
        protected override bool Localized => true;

        protected override async Task<ICollection<VocabularyValueInfo>?> GetVocabularyValues()
        {
            return await Task.Run<ICollection<VocabularyValueInfo>>(() => new List<VocabularyValueInfo>
            {
                CreateVocabularyValue(1, "1. Full access and free use for all", "1. Fullständig åtkomst och fri användning för alla"),
                CreateVocabularyValue(3, "3. Generalized to a 5*5 km grid cell for public usage", "3. Maximal upplösning 5*5 km för allmänheten"),
                CreateVocabularyValue(4, "4. Generalized to a 25*25 km grid cell for public usage", "4. Maximal upplösning 25*25 km för allmänheten"),
                CreateVocabularyValue(5, "5. Generalized to a 50*50 km grid cell for public usage", "5. Maximal upplösning 50*50 km för allmänheten")
            });
        }

        protected override List<ExternalSystemMapping> GetExternalSystemMappings(
           ICollection<VocabularyValueInfo>? vocabularyValues)
        {
            return new List<ExternalSystemMapping>
            {
                //GetArtportalenExternalSystemMapping(vocabularyValues)
            };
        }
    }
}