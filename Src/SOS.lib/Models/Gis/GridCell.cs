namespace SOS.Lib.Models.Gis
{
    public class GridCell
    {
        public long? ObservationsCount { get; set; }
        public long? TaxaCount { get; set; }
        public LatLonBoundingBox BoundingBox { get; set; }
    }
}