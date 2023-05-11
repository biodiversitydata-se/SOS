using SOS.Observations.Api.Dtos.Enum;

namespace SOS.Observations.Api.Dtos.Filter
{
    /// <summary>
    /// Internal search filter.
    /// </summary>
    public class SearchFilterInternalBaseDto : SearchFilterBaseDto
    {
        /// <summary>
        /// Artportalen specific search properties
        /// </summary>
        public ExtendedFilterDto ExtendedFilter{ get; set; }

        /// <summary>
        /// Observation protection filter
        /// </summary>
        public ProtectionFilterDto? ProtectionFilter { get; set; }
    }
}