using SOS.Lib.Models.Shared;

namespace SOS.Observations.Api.Dtos
{
    /// <summary>
    ///     Area used external
    /// </summary>
    public class AreaDto : AreaBaseDto
    {
        /// <summary>
        ///     Area geometry
        /// </summary>
        public GeoJsonGeometry Geometry { get; set; }
    }
}