﻿using SOS.Lib.Models.Gis;
using System.Collections.Generic;

namespace SOS.Lib.Models.Search.Result
{
    public class GeoGridResult
    {
        public LatLonBoundingBox BoundingBox { get; set; }
        public int Precision { get; set; }
        public int GridCellCount { get; set; }
        public IEnumerable<GridCellGeohash> GridCells { get; set; }
    }
}