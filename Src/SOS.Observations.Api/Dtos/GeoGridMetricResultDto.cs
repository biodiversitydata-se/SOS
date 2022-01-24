using System.Collections.Generic;

namespace SOS.Observations.Api.Dtos
{
    public class GeoGridMetricResultDto
    {
        public LatLonBoundingBoxDto BoundingBox { get; set; }
        public int GridCellSizeInMeters { get; set; }
        public int GridCellCount { get; set; }
        public IEnumerable<GridCellDto> GridCells { get; set; }
    }
}