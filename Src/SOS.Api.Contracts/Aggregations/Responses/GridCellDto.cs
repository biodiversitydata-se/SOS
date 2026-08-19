using SOS.Api.Contracts.Common.Geo;

namespace SOS.Api.Contracts.Aggregations.Responses;
public class GridCellDto
{

    /// <summary>
    /// Grid cell Id.
    /// </summary>
    public string Id { get; set; }

    public LatLonBoundingBoxDto BoundingBox { get; set; }

    /// <summary>
    /// Number of observations in cell
    /// </summary>
    public long? ObservationsCount { get; set; }

    public XYBoundingBoxDto MetricBoundingBox { get; set; }

    /// <summary>
    /// Count of different taxa
    /// </summary>
    public long? TaxaCount { get; set; }

}


