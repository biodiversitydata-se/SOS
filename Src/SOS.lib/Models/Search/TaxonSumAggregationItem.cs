using System.Collections.Generic;

namespace SOS.Lib.Models.Search
{
    /// <summary>
    /// Taxon aggregation item containing sum of underlying taxa values.
    /// </summary>
    public class TaxonSumAggregationItem
    {
        /// <summary>
        /// Taxon id.
        /// </summary>
        public int TaxonId { get; set; }
        
        /// <summary>
        /// Observation count.
        /// </summary>
        public int ObservationCount { get; set; }
        
        /// <summary>
        /// Sum of observation count including underlying taxa observation count.
        /// </summary>
        public int SumObservationCount { get; set; }
        
        /// <summary>
        /// Number of provinces the taxon is observed.
        /// </summary>
        public int ProvinceCount { get; set; }
        
        /// <summary>
        /// Number of provinces the taxon is observed including underlying taxa.
        /// </summary>
        public int SumProvinceCount { get; set; }

        /// <summary>
        /// Sum of observation count including underlying taxa observation count, by Province id.
        /// </summary>
        public Dictionary<string, int> SumObservationCountByProvinceId { get; set; }
    }
}


