using SOS.Lib.Models.Shared;

namespace SOS.Observations.Api.Models.Area
{
    /// <summary>
    /// Area used external
    /// </summary>
    public class ExternalArea
    {
        /// <summary>
        /// Type of area
        /// </summary>
        public string AreaType { get; set; }

        /// <summary>
        /// Area geometry
        /// </summary>
        public GeoJsonGeometry Geometry { get; set; }

        /// <summary>
        /// Area Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name of area
        /// </summary>
        public string Name { get; set; }

        

    }
}
