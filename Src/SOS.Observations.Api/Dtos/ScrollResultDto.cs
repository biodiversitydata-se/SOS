using System.Collections.Generic;

namespace SOS.Observations.Api.Dtos
{
    /// <summary>
    ///     Result returned by scroll query
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ScrollResultDto<T>
    {
        /// <summary>
        /// True if more pages can be retrieved using the ScrollId; otherwise false.
        /// </summary>
        public bool HasMorePages { get; set; }

        /// <summary>
        /// The scroll id used for retrieving next page of records.
        /// </summary>
        public string ScrollId { get; set; }

        /// <summary>
        ///     Returns a sequence containing up to the specified number of items.
        /// </summary>
        public int Take { get; set; }

        /// <summary>
        ///     Total number of records matching the query
        /// </summary>
        public long TotalCount { get; set; }

        /// <summary>
        ///     Paged records
        /// </summary>
        public IEnumerable<T> Records { get; set; }
    }

}
