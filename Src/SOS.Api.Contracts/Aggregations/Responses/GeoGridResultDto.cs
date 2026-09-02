using SOS.Api.Contracts.Common.Geo;

namespace SOS.Api.Contracts.Aggregations.Responses;
public class GeoGridResultDto
{

    public LatLonBoundingBoxDto BoundingBox { get; set; }

    public int Zoom { get; set; }

    public int GridCellCount { get; set; }

    public ICollection<GeoGridCellDto> GridCells { get; set; }

    public long TotalGridCellCount { get; set; }

}


