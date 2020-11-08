namespace SOS.Lib.Configuration.Process
{
    /// <summary>
    ///     Vocabulary process configuration.
    /// </summary>
    public class VocabularyConfiguration
    {
        /// <summary>
        ///     Decides whether field mapping values should be resolved
        ///     (for debugging purpose) when processing observations.
        /// </summary>
        public bool ResolveValues { get; set; } = false;

        /// <summary>
        ///     Culture code for localized vocabulary fields.
        /// </summary>
        public string LocalizationCultureCode { get; set; } = "en-GB";
    }
}