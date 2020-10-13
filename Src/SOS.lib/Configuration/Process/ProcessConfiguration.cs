namespace SOS.Lib.Configuration.Process
{
    /// <summary>
    ///     Root config
    /// </summary>
    public class ProcessConfiguration
    {
        /// <summary>
        ///     Decides whether parallell processing should be used.
        /// </summary>
        public bool ParallelProcessing { get; set; }

        /// <summary>
        ///     No of threads to run in parallel
        /// </summary>
        public int NoOfThreads { get; set; }

        /// <summary>
        /// Run incremental harvest after full harvest 
        /// </summary>
        public bool RunIncrementalAfterFull { get; set; }

        /// <summary>
        ///     Field mapping
        /// </summary>
        public FieldMappingConfiguration FieldMapping { get; set; }

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
    }
}