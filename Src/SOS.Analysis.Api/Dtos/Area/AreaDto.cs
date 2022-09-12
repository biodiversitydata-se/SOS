using SOS.Lib.Models.Gis;
using SOS.Analysis.Api.Dtos.Area.Enum;

namespace SSOS.Analysis.Api.Dtos.Area
{
    public class AreaDto
    {
        /// <summary>
        ///     Type of area
        /// </summary>
        public AreaTypeDto AreaType { get; set; }

        /// <summary>
        /// Area bounding box
        /// </summary>
        public LatLonBoundingBox BoundingBox { get; set; }

        /// <summary>
        /// Feature id
        /// </summary>
        public string FeatureId { get; set; }

        /// <summary>
        ///     Name of area
        /// </summary>
        public string Name { get; set; }
    }
}