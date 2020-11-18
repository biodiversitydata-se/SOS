using SOS.Lib.Enums;

namespace SOS.Observations.Api.Dtos.Filter
{
    /// <summary>
    /// Area filter.
    /// </summary>
    public class AreaFilterDto
    {
        /// <summary>
        ///     Type of area
        /// </summary>
        public AreaType Type { get; set; }
       
        /// <summary>
        ///    Feature
        /// </summary>
        public string Feature { get; set; }
    }
}