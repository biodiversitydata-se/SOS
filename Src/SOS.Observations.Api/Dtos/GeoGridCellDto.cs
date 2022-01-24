namespace SOS.Observations.Api.Dtos
{
    public class GeoGridCellDto
    {
        public long? ObservationsCount { get; set; }
        public long? TaxaCount { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Zoom { get; set; }
        public LatLonBoundingBoxDto BoundingBox { get; set; }
    }
}
