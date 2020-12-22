
using NetTopologySuite.Geometries;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;

namespace SOS.Lib.Models.Gis
{
    public class LatLonBoundingBox
    {
        public LatLonCoordinate BottomRight { get; set; }
        public LatLonCoordinate TopLeft { get; set; }
        private static readonly GeometryFactory _geometryFactory = new GeometryFactory();
        public Polygon GetPolygon(CoordinateSys coordinateSystem = CoordinateSys.WGS84, bool counterClockwiseOrder = true)
        {
            Polygon polygon;
            if (counterClockwiseOrder)
            {
                polygon = _geometryFactory.CreatePolygon(new[]
                {
                    new Coordinate(TopLeft.Longitude, TopLeft.Latitude),
                    new Coordinate(TopLeft.Longitude, BottomRight.Latitude),
                    new Coordinate(BottomRight.Longitude, BottomRight.Latitude),
                    new Coordinate(BottomRight.Longitude, TopLeft.Latitude),
                    new Coordinate(TopLeft.Longitude, TopLeft.Latitude)
                });
            }
            else
            {
                polygon = _geometryFactory.CreatePolygon(new[]
                {
                    new Coordinate(TopLeft.Longitude, TopLeft.Latitude),
                    new Coordinate(BottomRight.Longitude, TopLeft.Latitude),
                    new Coordinate(BottomRight.Longitude, BottomRight.Latitude),
                    new Coordinate(TopLeft.Longitude, BottomRight.Latitude),
                    new Coordinate(TopLeft.Longitude, TopLeft.Latitude)
                });
            }

            var webMercatorPolygon = (Polygon)polygon.Transform(CoordinateSys.WGS84, coordinateSystem);
            return webMercatorPolygon;
        }

        public static LatLonBoundingBox Create(Envelope envelope)
        {
            return Create(envelope?.MinX, envelope?.MaxY, envelope?.MaxX, envelope?.MinY);
        }

        public static LatLonBoundingBox Create(
            double? left,
            double? top,
            double? right,
            double? bottom)
        {
            LatLonBoundingBox bbox;
            if (left.HasValue && top.HasValue && right.HasValue && bottom.HasValue)
            {
                bbox = new LatLonBoundingBox
                {
                    TopLeft = new LatLonCoordinate(top.Value, left.Value),
                    BottomRight = new LatLonCoordinate(bottom.Value, right.Value)
                };
            }
            else
            {
                bbox = new LatLonBoundingBox
                {
                    TopLeft = new LatLonCoordinate(90, -180),
                    BottomRight = new LatLonCoordinate(-90, 180)
                };
            }

            return bbox;
        }
    }
}