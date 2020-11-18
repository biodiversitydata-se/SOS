using SOS.Lib.Models.Shared;

namespace SOS.Observations.Api.Models.Area
{
    /// <summary>
    ///     Area used external
    /// </summary>
    public class ExternalArea : ExternalSimpleArea
    {
        /// <summary>
        ///     Area geometry
        /// </summary>
        public GeoJsonGeometry Geometry { get; set; }
    }
}