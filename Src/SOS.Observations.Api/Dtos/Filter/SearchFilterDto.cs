namespace SOS.Observations.Api.Dtos.Filter
{
    /// <summary>
    /// Search filter.
    /// </summary>
    public class SearchFilterDto : SearchFilterBaseDto
    {
        /// <summary>
        /// Response output settings
        /// </summary>
        public OutputFilterDto Output { get; set; }
    }
}