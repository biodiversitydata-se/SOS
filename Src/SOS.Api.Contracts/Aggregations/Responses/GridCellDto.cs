using SOS.Api.Contracts.Common.Geo;

namespace SOS.Api.Contracts.Aggregations.Responses;
/// <summary>A metric grid cell and its observation counts.</summary>
public class GridCellDto
{

    /// <summary>
    /// Grid cell Id.
    /// </summary>
    public string Id { get; set; }

    /// <summary>Cell bounds in geographic coordinates.</summary>
    public LatLonBoundingBoxDto BoundingBox { get; set; }

    /// <summary>
    /// Number of observations in cell
    /// </summary>
    public long? ObservationsCount { get; set; }

    /// <summary>Cell bounds in the selected metric coordinate system.</summary>
    public XYBoundingBoxDto MetricBoundingBox { get; set; }

    /// <summary>
    /// Count of different taxa
    /// </summary>
    public long? TaxaCount { get; set; }

}


