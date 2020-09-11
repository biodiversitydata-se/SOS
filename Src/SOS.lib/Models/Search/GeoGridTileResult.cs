using System.Collections.Generic;
using SOS.Lib.Models.Gis;

namespace SOS.Lib.Models.Search
{
    public class GeoGridTileResult
    {
        public LatLonBoundingBox BoundingBox { get; set; }
        public int Zoom { get; set; }
        public int GridCellTileCount { get; set; }
        public IEnumerable<GridCellTile> GridCellTiles { get; set; }
    }
}