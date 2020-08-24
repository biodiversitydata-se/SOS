using Nest;
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
        public IGeoShape Geometry { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="areaType"></param>
        public AreaWithGeometry(AreaType areaType) : base(areaType)
        {
        }
    }
}