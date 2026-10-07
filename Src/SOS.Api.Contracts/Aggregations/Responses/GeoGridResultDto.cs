using SOS.Api.Contracts.Common.Geo;

namespace SOS.Api.Contracts.Aggregations.Responses;
/// <summary>Observation aggregation over a geographic grid.</summary>
public class GeoGridResultDto
{

    /// <summary>The grid bounds in latitude and longitude.</summary>
    public LatLonBoundingBoxDto BoundingBox { get; set; }

    /// <summary>The zoom level used to define the geographic grid.</summary>
    public int Zoom { get; set; }

    /// <summary>The number of returned grid cells.</summary>
    public int GridCellCount { get; set; }

    /// <summary>The returned grid cells and their observation counts.</summary>
    public ICollection<GeoGridCellDto> GridCells { get; set; }

    /// <summary>The total number of matching grid cells, including cells not returned in this result.</summary>
    public long TotalGridCellCount { get; set; }

}


