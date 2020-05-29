using System.Collections.Generic;

namespace SOS.Lib.Models.Search
{
    /// <summary>
    ///     Result returned by paged query
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PagedResult<T>
    {
        /// <summary>
        ///     Paged records
        /// </summary>
        public IEnumerable<T> Records { get; set; }

        /// <summary>
        ///     Ignores the specified number of items and returns a sequence starting at the item after the last skipped item (if
        ///     any)
        /// </summary>
        public int Skip { get; set; }

        /// <summary>
        ///     Returns a sequence containing up to the specified number of items. Anything after the count is ignored
        /// </summary>
        public int Take { get; set; }

        /// <summary>
        ///     Total number of records matching the query
        /// </summary>
        public long TotalCount { get; set; }
    }
}