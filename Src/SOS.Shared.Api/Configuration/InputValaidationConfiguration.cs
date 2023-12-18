namespace SOS.Shared.Api.Configuration
{
    public class InputValaidationConfiguration
    {
        /// <summary>
        /// Factor used to compute tiles limit
        /// </summary>
        public double CountFactor { get; set; }

        /// <summary>
        /// Max records returned by elastic for internal resourses
        /// </summary>
        public int ElasticSearchMaxRecordsInternal { get; set; }

        /// <summary>
        /// Max records returned by elastic for public resourses
        /// </summary>
        public int ElasticSearchMaxRecordsPublic { get; set; }

        /// <summary>
        /// Max number of documenst in a batch
        /// </summary>
        public int MaxBatchSize { get; set; }

        /// <summary>
        /// Max number of aggregation buckets
        /// </summary>
        public int MaxNrElasticSearchAggregationBuckets { get; set; }

        /// <summary>
        /// Taxon list id's allowed in signal search
        /// </summary>
        public IEnumerable<int>? SignalSearchTaxonListIds { get; set; }

        /// <summary>
        /// Max number of buckets created by aggregations
        /// </summary>
        public int TilesLimitInternal { get; set; }

        /// <summary>
        /// Max number of buckets created by aggregations
        /// </summary>
        public int TilesLimitPublic { get; set; }
    }
}
