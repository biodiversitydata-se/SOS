using SOS.Api.Contracts.Common.Geo;
using SOS.Lib.Models.Gis;
using System.Collections.Generic;

namespace SOS.Lib.Models.Search.Result;

public class GeoGridResult
{
    public LatLonBoundingBoxDto BoundingBox { get; set; }
    public int Precision { get; set; }
    public int GridCellCount { get; set; }
    public IEnumerable<GridCellGeohash> GridCells { get; set; }
}