using System.Collections.Generic;

namespace SOS.Lib.Models.Analysis
{
    public class PagedAggregationResult<T>
    {
        /// <summary>
        /// Key used to search next n items
        /// </summary>
        public object AfterKey { get; set; }

        /// <summary>
        /// Search records
        /// </summary>
        public IEnumerable<T> Records { get; set; }
    }
}
