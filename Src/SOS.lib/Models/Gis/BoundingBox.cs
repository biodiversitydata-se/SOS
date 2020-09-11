namespace SOS.Lib.Models.Gis
{
    public class LatLonBoundingBox
    {
        public string GeoHash { get; set; }
        public LatLonCoordinate BottomRight { get; set; }
        public LatLonCoordinate TopLeft { get; set; }
    }
}