using SOS.Shared.Api.Dtos.Enum;

namespace SOS.Shared.Api.Dtos.Filter
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
        public string FeatureId { get; set; }
    }
}