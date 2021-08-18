using SOS.Administration.Gui.Dtos.Enum;

namespace SOS.Administration.Gui.Dtos
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