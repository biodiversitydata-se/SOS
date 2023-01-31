namespace SOS.Lib.Configuration.Shared
{
    /// <summary>
    ///     ElasticSearch configuration properties
    /// </summary>
    public class ElasticSearchIndexConfiguration
    {
        /// <summary>
        /// Name of index
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     How many items to read in a time when scrolling
        /// </summary>
        public int ReadBatchSize { get; set; }

        /// <summary>
        ///     How many items to write in a time
        /// </summary>
        public int WriteBatchSize { get; set; }

        /// <summary>
        /// How many items to retrieve in each scroll request.
        /// </summary>
        public int ScrollBatchSize { get; set; } = 5000;

        /// <summary>
        /// Scroll timeout.
        /// </summary>
        public string ScrollTimeout { get; set; } = "300s";

        /// <summary>
        /// Max number of aggregation buckets.
        /// </summary>
        public int MaxNrAggregationBuckets { get; set; } = 65535;

        /// <summary>
        /// Number of shards
        /// </summary>
        public int NumberOfShards { get; set; } = 6;

        /// <summary>
        /// Number of shards for protected index
        /// </summary>
        public int NumberOfShardsProtected { get; set; } = 1;

        /// <summary>
        /// Number of replicas
        /// </summary>
        public int NumberOfReplicas { get; set; }
    }
}