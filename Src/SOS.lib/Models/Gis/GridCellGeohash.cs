namespace SOS.Lib.Models.Gis
{
    public class GridCellGeohash
    {
        public long? ObservationsCount { get; set; }
        public long? TaxaCount { get; set; }
        public LatLonGeohashBoundingBox BoundingBox { get; set; }
    }
}