namespace SOS.Observations.Api.Dtos.Filter
{
    /// <summary>
    /// Internal search filter.
    /// </summary>
    public class SearchFilterInternalDto : SearchFilterDto
    {
        /// <summary>
        /// By default totalCount in search response will not exceed 10 000. If IncludeRealCount is true, totalCount will show the real number of hits. Even if it's more than 10 000 (performance cost)  
        /// </summary>
        public bool IncludeRealCount { get; set; }

        /// <summary>
        /// Artportalen specific search properties
        /// </summary>
        public ArtportalenFilterDto ArtportalenFilter { get; set; }
    }
}