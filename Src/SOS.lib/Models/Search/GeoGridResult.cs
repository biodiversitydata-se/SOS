using System.Collections.Generic;
using SOS.Lib.Models.Gis;

namespace SOS.Lib.Models.Search
{
    public class GeoGridResult
    {
        public LatLonBoundingBox BoundingBox { get; set; }
        public int Precision { get; set; }
        public int GridCellCount { get; set; }
        public IEnumerable<GridCellGeohash> GridCells { get; set; }
    }
}