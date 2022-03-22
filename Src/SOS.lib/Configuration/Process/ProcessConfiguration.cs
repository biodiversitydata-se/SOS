namespace SOS.Lib.Configuration.Process
{
    /// <summary>
    ///     Root config
    /// </summary>
    public class ProcessConfiguration
    {
        /// <summary>
        /// URL to Artportalen
        /// </summary>
        public string ArtportalenUrl { get; set; }

        /// <summary>
        /// True if diffusion should be enabled
        /// </summary>
        public bool Diffusion { get; set; }

        /// <summary>
        /// True if time manager should be enabled
        /// </summary>
        public bool EnableTimeManager { get; set; }

        /// <summary>
        ///     No of threads to run in parallel
        /// </summary>
        public int NoOfThreads { get; set; }

        /// <summary>
        /// Run incremental harvest after full harvest 
        /// </summary>
        public bool RunIncrementalAfterFull { get; set; }

        /// <summary>
        /// Look for garbage chars i any string field and log every occurrence
        /// Will slow down processing 
        /// </summary>
        public bool LogGarbageCharFields { get; set; }

        /// <summary>
        ///     Vocabulary configuration
        /// </summary>
        public VocabularyConfiguration VocabularyConfiguration { get; set; }

        /// <summary>
        ///     taxon attribute service configuration
        /// </summary>
        public TaxonAttributeServiceConfiguration TaxonAttributeServiceConfiguration { get; set; }

        /// <summary>
        ///     Taxon service configuration
        /// </summary>
        public TaxonServiceConfiguration TaxonServiceConfiguration { get; set; }

        /// <summary>
        /// Blob storage container for export files
        /// </summary>
        public string Export_Container { get; set; }

        /// <summary>
        /// Minimum observations processed for switching active instance
        /// </summary>
        public long MinObservationCount { get; set; }
    }
}