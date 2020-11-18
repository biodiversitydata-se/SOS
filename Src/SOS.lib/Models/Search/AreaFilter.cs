using SOS.Lib.Enums;

namespace SOS.Lib.Models.Search
{
    /// <summary>
    /// Area filter.
    /// </summary>
    public class AreaFilter
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