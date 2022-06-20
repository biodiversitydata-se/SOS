using System.Collections.Generic;
using SOS.Lib.Models.Gis;

namespace SOS.Lib.Models.Search.Result
{
    /// <summary>
    /// Geo grid aggregation 
    /// </summary>
    public class GeoGridMetricResult
    {
        /// <summary>
        /// Input bounding box
        /// </summary>
        public LatLonBoundingBox BoundingBox { get; set; }

        /// <summary>
        /// Grid cell size in meters
        /// </summary>
        public int GridCellSizeInMeters { get; set; }

        /// <summary>
        /// Grid cell count
        /// </summary>
        public int GridCellCount { get; set; }

        /// <summary>
        /// Grid cells
        /// </summary>
        public IEnumerable<GridCell> GridCells { get; set; }
    }
}