namespace SOS.Lib.Models.Search
{
    /// <summary>
    /// Result returned by scroll query
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ScrollResult<T> : PagedResult<T>
    {
        /// <summary>
        /// Scroll id
        /// </summary>
        public string ScrollId { get; set; }
    }
}
