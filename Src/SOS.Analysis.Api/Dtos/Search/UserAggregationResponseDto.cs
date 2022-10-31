namespace SOS.Analysis.Api.Dtos.Search
{
    public class UserAggregationResponseDto
    {
        /// <summary>
        /// Aggregated field
        /// </summary>
        public object AggregationField { get; set; }

        /// <summary>
        /// Document count
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Unique taxon count
        /// </summary>
        public int UniqueTaxon { get; set; }
    }

    
}
