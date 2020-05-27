namespace SOS.Lib.Configuration.Shared
{
    /// <summary>
    /// ElasticSearch configuration properties
    /// </summary>
    public class ElasticSearchConfiguration
    {
        /// <summary>
        /// Batch size when scrolling
        /// </summary>
        public int BatchSize { get; set; }

        /// <summary>
        /// Host
        /// </summary>
        public string[] Hosts { get; set; }

        /// <summary>
        /// dev, st or at. prod is empty
        /// </summary>
        public string IndexPrefix { get; set; }
    }
}