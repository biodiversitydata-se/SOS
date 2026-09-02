using GeoJSON.Text.Geometry;

namespace SOS.Api.Contracts.Areas.Responses;

/// <summary>An area together with its exported geometry.</summary>
public sealed class AreaWithGeometryDto : AreaBaseDto
{
    /// <summary>The area geometry.</summary>
    public IGeometryObject? Geometry { get; set; }
}
