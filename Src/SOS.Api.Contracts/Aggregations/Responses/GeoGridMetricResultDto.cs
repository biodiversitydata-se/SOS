using SOS.Api.Contracts.Common.Geo;

namespace SOS.Api.Contracts.Aggregations.Responses;
public class GeoGridMetricResultDto
{

    public LatLonBoundingBoxDto BoundingBox { get; set; }

    public ICollection<GridCellDto> GridCells { get; set; }

    public int GridCellCount { get; set; }

    public int GridCellSizeInMeters { get; set; }

    public XYBoundingBoxDto Sweref99TmBoundingBox { get; set; }

}


