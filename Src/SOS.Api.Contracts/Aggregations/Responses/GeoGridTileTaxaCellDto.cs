
using SOS.Api.Contracts.Common.Geo;

namespace SOS.Api.Contracts.Aggregations.Responses;

/// <summary>
/// A map grid cell containing observation counts per taxon.
/// </summary>
public class GeoGridTileTaxaCellDto
{
    /// <summary>The tile bounds in latitude and longitude.</summary>
    public LatLonBoundingBoxDto BoundingBox { get; set; }
    /// <summary>The tile key in zoom/x/y format.</summary>
    public string GeoTile { get; set; }
    /// <summary>The tile's zoom level.</summary>
    public int Zoom { get; set; }
    /// <summary>The tile's horizontal index at the specified zoom level.</summary>
    public int X { get; set; }
    /// <summary>The tile's vertical index at the specified zoom level.</summary>
    public int Y { get; set; }
    /// <summary>The observation counts for each taxon in the cell.</summary>
    public IEnumerable<GeoGridTileTaxonObservationCountDto> Taxa { get; set; }
}
