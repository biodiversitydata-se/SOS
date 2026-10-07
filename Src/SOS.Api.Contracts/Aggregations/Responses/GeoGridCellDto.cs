using SOS.Api.Contracts.Common.Geo;

namespace SOS.Api.Contracts.Aggregations.Responses;
/// <summary>An aggregated cell in a geographic grid.</summary>
public class GeoGridCellDto
{

    /// <summary>The number of observations in the cell.</summary>
    public long? ObservationsCount { get; set; }

    /// <summary>The number of distinct taxa in the cell.</summary>
    public long? TaxaCount { get; set; }

    /// <summary>The tile's horizontal index at the specified zoom level.</summary>
    public int X { get; set; }

    /// <summary>The tile's vertical index at the specified zoom level.</summary>
    public int Y { get; set; }

    /// <summary>The zoom level used to define the geographic grid.</summary>
    public int Zoom { get; set; }

    /// <summary>The cell bounds in latitude and longitude.</summary>
    public LatLonBoundingBoxDto BoundingBox { get; set; }

}


