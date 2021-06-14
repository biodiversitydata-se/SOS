using System.Collections.Generic;

namespace SOS.Observations.Api.Dtos.Vocabulary
{
    public class VocabularyValueInfoCategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Localized { get; set; }

        /// <summary>
        ///     Translations.
        /// </summary>
        /// <remarks>
        ///     Translations exists if the <see cref="Localized" /> property is set to true.
        /// </remarks>
        public IEnumerable<VocabularyValueTranslationDto> Translations { get; set; }
    }
}
