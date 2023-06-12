using System;
using CSharpFunctionalExtensions;
using NGeoHash;

namespace SOS.Lib.Models.Gis
{
    public class LatLonGeohashBoundingBox : LatLonBoundingBox
    {
        public string Geohash { get; set; }

        public static Result<LatLonGeohashBoundingBox> Create(
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

        public new static Result<LatLonGeohashBoundingBox> Create(
            double? left,
            double? top,
            double? right,
            double? bottom)
        {
            LatLonGeohashBoundingBox bbox;
            if (left.HasValue && top.HasValue && right.HasValue && bottom.HasValue)
            {
                if (left >= right)
                {
                    return Result.Failure<LatLonGeohashBoundingBox>("Bbox left value is >= right value.");
                }

                if (bottom >= top)
                {
                    return Result.Failure<LatLonGeohashBoundingBox>("Bbox bottom value is >= top value.");
                }

                bbox = new LatLonGeohashBoundingBox
                {
                    TopLeft = new LatLonCoordinate(top.Value, left.Value),
                    BottomRight = new LatLonCoordinate(bottom.Value, right.Value)
                };
            }
            else
            {
                bbox = new LatLonGeohashBoundingBox
                {
                    TopLeft = new LatLonCoordinate(90, -180),
                    BottomRight = new LatLonCoordinate(-90, 180)
                };
            }

            return Result.Success(bbox);
        }

        public static Result<LatLonGeohashBoundingBox> CreateFromGeohash(string geohash)
        {
            try
            {
                var geohashBbox = GeoHash.DecodeBbox(geohash);
                var bbox = new LatLonGeohashBoundingBox
                {
                    Geohash = geohash,
                    TopLeft = new LatLonCoordinate(geohashBbox.Maximum.Lat, geohashBbox.Minimum.Lon),
                    BottomRight = new LatLonCoordinate(geohashBbox.Minimum.Lat, geohashBbox.Maximum.Lon)
                };

                return Result.Success(bbox);
            }
            catch (Exception)
            {
                return Result.Failure<LatLonGeohashBoundingBox>("The geohash is invalid.");
            }
        }
    }
}