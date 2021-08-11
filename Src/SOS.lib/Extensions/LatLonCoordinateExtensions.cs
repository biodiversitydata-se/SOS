using Nest;
using SOS.Lib.Models.Gis;

namespace SOS.Lib.Extensions
{
    public static class LatLonCoordinateExtensions
    {
        public static GeoLocation ToGeoLocation(this LatLonCoordinate coordinate)
        {
            return new GeoLocation(coordinate.Latitude, coordinate.Longitude);
        }
    }
}