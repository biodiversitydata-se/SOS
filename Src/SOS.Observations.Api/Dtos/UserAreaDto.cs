using SOS.Observations.Api.Dtos.Enum;

namespace SOS.Observations.Api.Dtos
{
    /// <summary>
    /// User role authority area.
    /// </summary>
    public class UserAreaDto
    {
        /// <summary>
        /// Area type.
        /// </summary>
        public AreaTypeDto AreaType { get; set; }
        
        /// <summary>
        /// Area FeatureId.
        /// </summary>
        public string FeatureId { get; set; }
        
        /// <summary>
        /// Area name.
        /// </summary>
        public string Name { get; set; }
    }
}
