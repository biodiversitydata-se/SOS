using SOS.Api.Contracts.Common.Geo;

namespace SOS.Api.Contracts.Areas.Responses;
/// <summary>Basic information and bounds for a geographic area.</summary>
public class AreaBaseDto
{

    /// <summary>Type of area.</summary>
    public AreaTypeDto AreaType { get; set; }

    /// <summary>The numeric area-type identifier.</summary>
    public int AreaTypeId => (int)AreaType;

    /// <summary>Geographic bounding box of the area.</summary>
    public LatLonBoundingBoxDto BoundingBox { get; set; }

    /// <summary>
    /// Feature id
    /// </summary>
    public string FeatureId { get; set; }

    /// <summary>
    /// Name of area
    /// </summary>
    public string Name { get; set; }

    /// <summary>The key composed of the area type and feature identifier.</summary>
    public string Key => $"{AreaType}-{FeatureId}";

}


