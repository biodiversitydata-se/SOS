using Elastic.Clients.Elasticsearch;
using SOS.Lib.Models.Gis;

namespace SOS.Lib.Extensions
{
    public static class LatLonCoordinateExtensions
    {
        public static GeoLocation ToGeoLocation(this LatLonCoordinate coordinate)
        {
            return GeoLocation.LatitudeLongitude(new LatLonGeoLocation
            {
                Lat = coordinate.Latitude,
                Lon = coordinate.Longitude
            });
        }
        public static GeoBounds ToGeoBounds(this LatLonBoundingBox bbox)
        {
            return new TopLeftBottomRightGeoBounds
            {
                BottomRight = bbox.BottomRight.ToGeoLocation(),
                TopLeft = bbox.TopLeft.ToGeoLocation()
            };
        }
    }
}