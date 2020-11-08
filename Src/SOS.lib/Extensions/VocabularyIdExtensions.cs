using System;
using SOS.Lib.Enums;

namespace SOS.Lib.Extensions
{
    public static class VocabularyIdExtensions
    {
        public static bool IsGeographicRegionField(this VocabularyId vocabularyId)
        {
            return vocabularyId == VocabularyId.County
                   || vocabularyId == VocabularyId.Municipality
                   || vocabularyId == VocabularyId.Parish
                   || vocabularyId == VocabularyId.Province;
        }

        public static AreaType GetAreaType(this VocabularyId vocabularyId)
        {
            switch (vocabularyId)
            {
                case VocabularyId.County:
                    return AreaType.County;
                case VocabularyId.Municipality:
                    return AreaType.Municipality;
                case VocabularyId.Parish:
                    return AreaType.Parish;
                case VocabularyId.Province:
                    return AreaType.Province;
                default:
                    throw new ArgumentException($"GetAreaType(): '{vocabularyId}' is not mapped to an AreaType");
            }
        }
    }
}