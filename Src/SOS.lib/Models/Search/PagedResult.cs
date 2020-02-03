using System.Collections.Generic;

namespace SOS.Lib.Models.Search
{
    /// <summary>
    /// Result returned by paged query
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PagedResult<T>
    {
        /// <summary>
        /// Paged records
        /// </summary>
        public IEnumerable<T> Records { get; set; }

        /// <summary>
        /// Total number of records matching the query
        /// </summary>
        public long TotalCount { get; set; }
    }
}
