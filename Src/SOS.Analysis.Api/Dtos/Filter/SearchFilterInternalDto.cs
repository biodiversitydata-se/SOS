namespace SOS.Analysis.Api.Dtos.Filter
{
    /// <summary>
    /// Search filter.
    /// </summary>
    public class SearchFilterInternalDto : SearchFilterDto
    {
        /// <summary>
        /// Extended search parameters
        /// </summary>
        public ExtendedFilterDto? ExtendedFilter { get; set; }
    }
}