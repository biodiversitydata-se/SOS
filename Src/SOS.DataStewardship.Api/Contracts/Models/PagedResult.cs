namespace SOS.DataStewardship.Api.Contracts.Models
{
    /// <summary>
    /// Result returned by paged query.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PagedResult<T>
    {
        /// <summary>
        /// Ignores the specified number of items and returns a sequence starting at the item after the last skipped item.
        /// </summary>
        public int Skip { get; set; }

        /// <summary>
        /// The number of records requested.
        /// </summary>
        public int Take { get; set; }

        /// <summary>
        /// The number of records returned.
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// The total number of records matching the query.
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// The records.
        /// </summary>
        public IEnumerable<T> Records { get; set; }
    }
}
