namespace SOS.Analysis.Api.Dtos.Search
{
    public class PagedAggregationResultDto<T>
    {
        /// <summary>
        /// Key used to search next n items
        /// </summary>
        public object? AfterKey { get; set; }

        /// <summary>
        /// Search records
        /// </summary>
        public IEnumerable<T>? Records { get; set; }
    }
}
