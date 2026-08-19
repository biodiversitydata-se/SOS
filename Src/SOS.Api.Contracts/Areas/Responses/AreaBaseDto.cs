using SOS.Api.Contracts.Common.Geo;

namespace SOS.Api.Contracts.Areas.Responses;
public class AreaBaseDto
{

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public AreaTypeDto AreaType { get; set; }

    /// <summary>The numeric area-type identifier.</summary>
    public int AreaTypeId => (int)AreaType;

    public LatLonBoundingBox BoundingBox { get; set; }

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


