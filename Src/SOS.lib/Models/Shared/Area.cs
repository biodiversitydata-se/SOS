using MongoDB.Driver.GeoJsonObjectModel;
using SOS.Lib.Enums;
using SOS.Lib.Models.Interfaces;

namespace SOS.Lib.Models.Shared
{
    public class Area : AreaBase, IEntity<int>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="areaType"></param>
        public Area(AreaType areaType) : base(areaType)
        {
            
        }

        /// <summary>
        /// Area geometry
        /// </summary>

        public GeoJsonGeometry<GeoJson2DGeographicCoordinates> Geometry { get; set; }
    }
}
