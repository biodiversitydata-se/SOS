namespace SOS.Lib.Models.Gis
{
    public class LatLonCoordinate
    {
        public LatLonCoordinate(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}