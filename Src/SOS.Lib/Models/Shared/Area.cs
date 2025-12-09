using NetTopologySuite.Geometries;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Gis;
using SOS.Lib.Models.Interfaces;

namespace SOS.Lib.Models.Shared;

public class Area : IEntity<string>
{
    /// <summary>
    ///  Constructor
    /// </summary>
    /// <param name="areaType"></param>
    /// <param name="featureId"></param>
    public Area(AreaType areaType, string featureId)
    {
        AreaType = areaType;
        FeatureId = featureId;
        Id = AreaType.ToAreaId(featureId);
    }

    /// <summary>
    ///     Type of area
    /// </summary>
    public AreaType AreaType { get; set; }
    public int AreaTypeId => (int)AreaType;

    public LatLonBoundingBox BoundingBox { get; set; }

    /// <summary>
    ///     Feature Id.
    /// </summary>
    public string FeatureId { get; set; }

    /// <summary>
    /// If any type of Atlas grid we now that the geometry will only contain 5 coordinates. Then we can store it here to get quick access to the geometry 
    /// </summary>
    public Geometry GridGeometry { get; set; }

    /// <summary>
    ///     Area Id
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    ///     Name of area
    /// </summary>
    public string Name { get; set; }
}