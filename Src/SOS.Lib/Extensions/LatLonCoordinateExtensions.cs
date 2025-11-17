using Elastic.Clients.Elasticsearch;
using SOS.Lib.Models.Gis;

namespace SOS.Lib.Extensions;

public static class LatLonCoordinateExtensions
{
    extension(LatLonCoordinate coordinate)
    {
        public LatLonGeoLocation ToGeoLocation()
        {
            return new LatLonGeoLocation
            {
                Lat = coordinate.Latitude,
                Lon = coordinate.Longitude
            };
        }
    }

    extension(LatLonBoundingBox bbox)
    {
        public GeoBounds ToGeoBounds()
        {
            return new TopLeftBottomRightGeoBounds
            {
                BottomRight = bbox.BottomRight.ToGeoLocation(),
                TopLeft = bbox.TopLeft.ToGeoLocation()
            };
        }
    }
}