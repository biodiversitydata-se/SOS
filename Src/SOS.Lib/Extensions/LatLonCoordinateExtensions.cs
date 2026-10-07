using SOS.Api.Contracts.Common.Geo;
using Elastic.Clients.Elasticsearch;
using SOS.Lib.Models.Gis;

namespace SOS.Lib.Extensions;

public static class LatLonCoordinateExtensions
{
    extension(LatLonCoordinateDto coordinate)
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

}
