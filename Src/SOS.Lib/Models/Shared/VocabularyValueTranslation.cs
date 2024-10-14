namespace SOS.Lib.Models.Shared
{
    public class VocabularyValueTranslation
    {
        /// <summary>
        ///     Culture code. I.e. en-GB, sv-SE
        /// </summary>
        public string CultureCode { get; set; }

        /// <summary>
        ///     Translation
        /// </summary>
        public string Value { get; set; }

        public override string ToString()
        {
            return $"{CultureCode} = \"{Value}\"";
        }
    }
}