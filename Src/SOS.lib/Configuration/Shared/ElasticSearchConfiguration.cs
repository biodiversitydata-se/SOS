using System;

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
        /// Max number of aggregation buckets.
        /// </summary>
        public int MaxNrAggregationBuckets { get; set; } = 65535;

        /// <summary>
        ///     dev, st or at. prod is empty
        /// </summary>
        public string IndexPrefix
        {
            get
            {
                // If prefix is "sos-local", add the machine name to the index
                // to let developers have their own environment when running locally.
                if (_indexPrefix == "sos-local")
                {
                    return $"sos-local-{Environment.MachineName}";
                }

                return _indexPrefix;
            }
            set => _indexPrefix = value;
        }

        private string _indexPrefix;
    }
}