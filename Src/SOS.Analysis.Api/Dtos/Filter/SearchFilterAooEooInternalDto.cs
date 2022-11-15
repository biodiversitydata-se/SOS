namespace SOS.Analysis.Api.Dtos.Filter
{
    /// <summary>
    /// Search filter.
    /// </summary>
    public class SearchFilterAooEooInternalDto : SearchFilterInternalDto
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public SearchFilterAooEooInternalDto()
        {
            EdgeLengths = new[] { 0.5 };
        }

        /// <summary>
        /// The target edge length ratio when useEdgeLengthRatio is true, else the target maximum edge lengths
        /// </summary>
        public IEnumerable<double> EdgeLengths { get; set; }
    }
}
