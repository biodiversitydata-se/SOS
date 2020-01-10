namespace SOS.Lib.Models.Verbatim.SpeciesPortal
{
    /// <summary>
    /// Represents a metadata translation
    /// </summary>
    public class MetadataTranslation
    {
        /// <summary>
        /// Culture i.e. en-GB, sv-SE
        /// </summary>
        public string Culture { get; set; }

        /// <summary>
        /// Translation
        /// </summary>
        public string Value { get; set; }
    }
}
