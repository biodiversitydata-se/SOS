namespace SOS.Observations.Api.Dtos
{
    public class TranslationDto
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