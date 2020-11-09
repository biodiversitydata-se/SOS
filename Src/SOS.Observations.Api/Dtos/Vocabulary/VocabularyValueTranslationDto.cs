namespace SOS.Observations.Api.Dtos.Vocabulary
{
    public class VocabularyValueTranslationDto
    {
        /// <summary>
        ///     Culture code. I.e. en-GB, sv-SE
        /// </summary>
        public string CultureCode { get; set; }

        /// <summary>
        ///     Translation
        /// </summary>
        public string Value { get; set; }
    }
}
