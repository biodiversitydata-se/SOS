using SOS.Analysis.Api.Dtos.Filter.Enums;

namespace SOS.Analysis.Api.Dtos.Filter
{
    /// <summary>
    /// Area filter.
    /// </summary>
    public class AreaFilterDto
    {
        /// <summary>
        ///     Type of area
        /// </summary>
        public AreaTypeDto AreaType { get; set; }
       
        /// <summary>
        ///    Feature
        /// </summary>
        public string? FeatureId { get; set; }
    }
}