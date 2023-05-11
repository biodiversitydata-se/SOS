using Nest;
using NetTopologySuite.Geometries;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;

namespace SOS.Observations.Api.Dtos.DataStewardship.Extensions
{
    public static class GeoShapeExtensions
    {
        public static IGeoShape ConvertCoordinateSystem(this PointGeoShape point, CoordinateSys responseCoordinateSystem)
        {
            if (point == null) return null;

            var pointToTransform = new Point(point.Coordinates.Longitude, point.Coordinates.Latitude);
          
            var transformedPoint = pointToTransform.Transform(CoordinateSys.WGS84, responseCoordinateSystem);
            return transformedPoint.ToGeoShape();
        }
    }
}
