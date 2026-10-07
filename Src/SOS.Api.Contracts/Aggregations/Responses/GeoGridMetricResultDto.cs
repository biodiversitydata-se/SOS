using SOS.Api.Contracts.Common.Geo;

namespace SOS.Api.Contracts.Aggregations.Responses;
/// <summary>Observation aggregation over a metric grid.</summary>
public class GeoGridMetricResultDto
{

    /// <summary>The grid bounds in latitude and longitude.</summary>
    public LatLonBoundingBoxDto BoundingBox { get; set; }

    /// <summary>The returned grid cells and their observation counts.</summary>
    public ICollection<GridCellDto> GridCells { get; set; }

    /// <summary>The number of returned grid cells.</summary>
    public int GridCellCount { get; set; }

    /// <summary>The side length of a grid cell in meters.</summary>
    public int GridCellSizeInMeters { get; set; }

    /// <summary>The grid bounds in the SWEREF 99 TM coordinate system.</summary>
    public XYBoundingBoxDto Sweref99TmBoundingBox { get; set; }

}


