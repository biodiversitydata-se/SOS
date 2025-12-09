using NetTopologySuite.Geometries;

namespace SOS.Shared.Api.Dtos;

/// <summary>
/// Area with geometry
/// </summary>
public class AreaWithGeometryDto : AreaBaseDto
{
    public Geometry Geometry { get; set; }
}