namespace SOS.Lib.Configuration.Shared
{
    /// <summary>
    ///     ElasticSearch configuration properties
    /// </summary>
    public class ElasticSearchConfiguration
    {
        /// <summary>
        ///     How many items to read in a time when scrolling
        /// </summary>
        public int ReadBatchSize { get; set; }

        /// <summary>
        ///     How many items to write in a time
        /// </summary>
        public int WriteBatchSize { get; set; }

        /// <summary>
        ///     Host
        /// </summary>
        public string[] Hosts { get; set; }

        /// <summary>
        ///     dev, st or at. prod is empty
        /// </summary>
        public string IndexPrefix { get; set; }
    }
}