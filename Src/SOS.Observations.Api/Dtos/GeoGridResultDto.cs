using System.Collections.Generic;

namespace SOS.Observations.Api.Dtos
{
    public class GeoGridResultDto
    {
        public LatLonBoundingBoxDto BoundingBox { get; set; }
        public int Zoom { get; set; }
        public int GridCellCount { get; set; }
        public IEnumerable<GeoGridCellDto> GridCells { get; set; }

        public long TotalGridCellCount { get; set; }
    }
}