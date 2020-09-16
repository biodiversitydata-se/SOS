using System;
using CSharpFunctionalExtensions;
using NetTopologySuite.Geometries;
using NGeoHash;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using Result = CSharpFunctionalExtensions.Result;

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

        public static Result<LatLonBoundingBox> Create(
            string geohash,
            double? left,
            double? top,
            double? right,
            double? bottom)
        {
            // First try to create from geohash, then from bounds.
            if (!string.IsNullOrWhiteSpace(geohash))
            {
                return CreateFromGeohash(geohash);
            }

            return Create(left, top, right, bottom);
        }

        public static Result<LatLonBoundingBox> Create(
            double? left,
            double? top,
            double? right,
            double? bottom)
        {
            LatLonBoundingBox bbox;
            if (left.HasValue && top.HasValue && right.HasValue && bottom.HasValue)
            {
                if (left >= right)
                {
                    return Result.Failure<LatLonBoundingBox>("Bbox left value is >= right value.");
                }

                if (bottom >= top)
                {
                    return Result.Failure<LatLonBoundingBox>("Bbox bottom value is >= top value.");
                }

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

            return Result.Success(bbox);
        }


        public static Result<LatLonBoundingBox> CreateFromGeohash(string geohash)
        {
            try
            {
                var geohashBbox = GeoHash.DecodeBbox(geohash);
                var bbox = new LatLonBoundingBox
                {
                    TopLeft = new LatLonCoordinate(geohashBbox.Maximum.Lat, geohashBbox.Minimum.Lon),
                    BottomRight = new LatLonCoordinate(geohashBbox.Minimum.Lat, geohashBbox.Maximum.Lon)
                };

                return Result.Success(bbox);
            }
            catch (Exception)
            {
                return Result.Failure<LatLonBoundingBox>("The geohash is invalid.");
            }
        }
    }
}