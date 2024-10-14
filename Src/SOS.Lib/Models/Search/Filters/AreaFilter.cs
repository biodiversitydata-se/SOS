using SOS.Lib.Enums;

namespace SOS.Lib.Models.Search.Filters
{
    /// <summary>
    /// Area filter.
    /// </summary>
    public class AreaFilter
    {
        /// <summary>
        ///     Type of area
        /// </summary>
        public AreaType AreaType { get; set; }

        /// <summary>
        ///    Feature
        /// </summary>
        public string FeatureId { get; set; }
    }
}