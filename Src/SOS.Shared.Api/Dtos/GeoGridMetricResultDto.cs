using SOS.Shared.Api.Dtos.Filter;

namespace SOS.Shared.Api.Dtos
{
    public class GeoGridMetricResultDto
    {
        public LatLonBoundingBoxDto BoundingBox { get; set; }

        public IEnumerable<GridCellDto> GridCells { get; set; }

        public int GridCellCount { get; set; }

        public int GridCellSizeInMeters { get; set; }
        
        public XYBoundingBoxDto Sweref99TmBoundingBox { get; set; }
    }
}