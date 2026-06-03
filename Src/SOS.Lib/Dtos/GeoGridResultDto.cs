using SOS.Lib.Dtos.Filter;

namespace SOS.Lib.Dtos;

public class GeoGridResultDto
{
    public LatLonBoundingBoxDto BoundingBox { get; set; }
    public int Zoom { get; set; }
    public int GridCellCount { get; set; }
    public IEnumerable<GeoGridCellDto> GridCells { get; set; }
    public long TotalGridCellCount { get; set; }
}