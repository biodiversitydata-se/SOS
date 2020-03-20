using MongoDB.Driver.GeoJsonObjectModel;
using SOS.Lib.Enums;
using SOS.Lib.Models.Interfaces;

namespace SOS.Lib.Models.Verbatim.Shared
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


    //public class Area : IEntity<int>
    //{
    //    /// <summary>
    //    /// Constructor
    //    /// </summary>
    //    /// <param name="areaType"></param>
    //    public Area(AreaType areaType)
    //    {
    //        AreaType = areaType;
    //    }

    //    /// <summary>
    //    /// Area Id
    //    /// </summary>
    //    public int Id { get; set; }

    //    /// <summary>
    //    /// Area geometry
    //    /// </summary>

    //    public GeoJsonGeometry<GeoJson2DGeographicCoordinates> Geometry { get; set; }

    //    /// <summary>
    //    /// Name of area
    //    /// </summary>
    //    public string Name { get; set; }

    //    /// <summary>
    //    /// Type of area
    //    /// </summary>
    //    public AreaType AreaType { get; private set; }

    //    /// <summary>
    //    /// Feature Id.
    //    /// </summary>
    //    public int FeatureId { get; set; }

    //    /// <summary>
    //    /// Parent Id.
    //    /// </summary>
    //    public int? ParentId { get; set; }
    //}
}
