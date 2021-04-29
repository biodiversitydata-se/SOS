using NetTopologySuite.Geometries;
using SOS.Lib.Enums;

namespace SOS.Lib.Models.Shared
{
    /// <summary>
    /// Area with geometry.
    /// </summary>
    public class AreaWithGeometry : Area
    {
        /// <summary>
        /// Geometry.
        /// </summary>
        public Geometry Geometry { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="areaType"></param>
        public AreaWithGeometry(AreaType areaType, string featureId) : base(areaType, featureId)
        {
        }
    }
}