using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.SqlServer.Types;
using MongoDB.Driver.GeoJsonObjectModel;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;
using NetTopologySuite.IO;
using NetTopologySuite.Geometries;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;

namespace SOS.Lib.Extensions
{
    /// <summary>
    /// NetTopologySuite extensions
    /// </summary>
    public static class GISExtensions
    {
        #region Private
        private static readonly ConcurrentDictionary<long, ICoordinateTransformation> TransformationsDictionary = new ConcurrentDictionary<long, ICoordinateTransformation>();

        /// <summary>
        /// Math transform filter
        /// </summary>
        private sealed class MathTransformFilter : ICoordinateSequenceFilter
        {
            private readonly MathTransform _mathTransform;

            public MathTransformFilter(MathTransform mathTransform)
                => _mathTransform = mathTransform;

            public bool Done => false;
            public bool GeometryChanged => true;

            public void Filter(CoordinateSequence seq, int i)
            {
                var (x, y, z) = _mathTransform.Transform(seq.GetX(i), seq.GetY(i), seq.GetZ(i));
                seq.SetX(i, x);
                seq.SetY(i, y);
                seq.SetZ(i, z);
            }
        }

        /// <summary>
        /// Calculate degrees from meters
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="meters"></param>
        /// <returns></returns>
        private static double CalculateDegreesFromMeters(double latitude, double meters)
        {
            return meters / (111.32 * 1000 * Math.Cos(latitude * (Math.PI / 180)));
        }

        /// <summary>
        /// Get coordinate system wkt
        /// </summary>
        /// <param name="coordinateSystem"></param>
        /// <returns></returns>
        private static string GetCoordinateSystemWkt(CoordinateSys coordinateSystem)
        {
            switch (coordinateSystem)
            {
                case CoordinateSys.WebMercator:
                    return @"PROJCS[""Google Mercator"",GEOGCS[""WGS 84"",DATUM[""World Geodetic System 1984"",SPHEROID[""WGS 84"", 6378137.0, 298.257223563, AUTHORITY[""EPSG"",""7030""]],AUTHORITY[""EPSG"",""6326""]],PRIMEM[""Greenwich"", 0.0, AUTHORITY[""EPSG"",""8901""]],UNIT[""degree"", 0.017453292519943295], AXIS[""Geodetic latitude"", NORTH], AXIS[""Geodetic longitude"", EAST], AUTHORITY[""EPSG"",""4326""]], PROJECTION[""Mercator_1SP""], PARAMETER[""semi_minor"", 6378137.0], PARAMETER[""latitude_of_origin"", 0.0], PARAMETER[""central_meridian"", 0.0], PARAMETER[""scale_factor"", 1.0], PARAMETER[""false_easting"", 0.0], PARAMETER[""false_northing"", 0.0], UNIT[""m"", 1.0], AXIS[""Easting"", EAST], AXIS[""Northing"", NORTH], AUTHORITY[""EPSG"",""900913""]]";
                case CoordinateSys.Rt90_25_gon_v:
                    return @"PROJCS[""SWEREF99 / RT90 2.5 gon V emulation"", GEOGCS[""SWEREF99"", DATUM[""SWEREF99"", SPHEROID[""GRS 1980"",6378137.0,298.257222101, AUTHORITY[""EPSG"",""7019""]], TOWGS84[0.0,0.0,0.0,0.0,0.0,0.0,0.0], AUTHORITY[""EPSG"",""6619""]], PRIMEM[""Greenwich"",0.0, AUTHORITY[""EPSG"",""8901""]], UNIT[""degree"",0.017453292519943295], AXIS[""Geodetic latitude"",NORTH], AXIS[""Geodetic longitude"",EAST], AUTHORITY[""EPSG"",""4619""]], PROJECTION[""Transverse Mercator""], PARAMETER[""central_meridian"",15.806284529444449], PARAMETER[""latitude_of_origin"",0.0], PARAMETER[""scale_factor"",1.00000561024], PARAMETER[""false_easting"",1500064.274], PARAMETER[""false_northing"",-667.711], UNIT[""m"",1.0], AXIS[""Northing"",NORTH], AXIS[""Easting"",EAST], AUTHORITY[""EPSG"",""3847""]]";
                case CoordinateSys.SWEREF99:
                    return @"GEOGCS[""SWEREF99"", DATUM[""SWEREF99"", SPHEROID[""GRS 1980"", 6378137, 298.257222101, AUTHORITY[""EPSG"", ""7019""]], TOWGS84[0, 0, 0, 0, 0, 0, 0], AUTHORITY[""EPSG"", ""6619""]], PRIMEM[""Greenwich"", 0, AUTHORITY[""EPSG"", ""8901""]], UNIT[""degree"", 0.0174532925199433, AUTHORITY[""EPSG"", ""9122""]], AUTHORITY[""EPSG"", ""4619""]]";
                case CoordinateSys.SWEREF99_TM:
                    return @"PROJCS[""SWEREF99 TM"", GEOGCS[""SWEREF99"", DATUM[""D_SWEREF99"", SPHEROID[""GRS_1980"",6378137,298.257222101]], PRIMEM[""Greenwich"",0], UNIT[""Degree"",0.017453292519943295]], PROJECTION[""Transverse_Mercator""], PARAMETER[""latitude_of_origin"",0], PARAMETER[""central_meridian"",15], PARAMETER[""scale_factor"",0.9996], PARAMETER[""false_easting"",500000], PARAMETER[""false_northing"",0], UNIT[""Meter"",1]]";
                case CoordinateSys.WGS84:
                    return @"GEOGCS[""GCS_WGS_1984"", DATUM[""WGS_1984"", SPHEROID[""WGS_1984"",6378137,298.257223563]], PRIMEM[""Greenwich"",0], UNIT[""Degree"",0.017453292519943295]]";
                default:
                    throw new ArgumentException("Not handled coordinate system id " + coordinateSystem);
            }
        }

        /// <summary>
        /// Get the crs urn string
        /// </summary>
        /// <param name="srid"></param>
        /// <returns></returns>
        private static string GetCrsUrn(int srid)
        {
            return $"urn:ogc:def:crs:EPSG::{srid}";
        }

        /// <summary>
        /// Cast array of array of array of double to polygon coordinates
        /// </summary>
        /// <param name="polygon"></param>
        /// <returns></returns>
        private static GeoJsonPolygonCoordinates<GeoJson2DGeographicCoordinates> ToGeoJsonPolygonCoordinates(this
            IEnumerable<IEnumerable<IEnumerable<double>>> polygon)
        {
            var rings = polygon.Select(r =>
                new GeoJsonLinearRingCoordinates<GeoJson2DGeographicCoordinates>(r.Select(p => new GeoJson2DGeographicCoordinates(p.ElementAt(0), p.ElementAt(1)))));

            return new GeoJsonPolygonCoordinates<GeoJson2DGeographicCoordinates>(
                rings.FirstOrDefault(),
                rings.Skip(1));
        }

        /// <summary>
        /// Create a linear ring out of coordinates
        /// </summary>
        /// <param name="coordinates"></param>
        /// <returns></returns>
        private static LinearRing ToLineRing(this Coordinate[] coordinates)
        {
            // Make sure we don't have any dublicates
            coordinates = coordinates.Select(c => c).Distinct().ToArray();

            // Make sure first and last coordinate is the same
            var newCoordinates = coordinates.Append(coordinates.First());

            // Create a new linear ring
            var geomFactory = new GeometryFactory();
            return geomFactory.CreateLinearRing(newCoordinates.ToArray());
        }

        /// <summary>
        /// Make polygon valid
        /// </summary>
        /// <param name="polygon"></param>
        /// <returns></returns>
        private static Polygon MakeValid(this Polygon polygon)
        {
            if (polygon.IsValid)
            {
                return polygon;
            }

            return (Polygon)polygon.Buffer(0);
           /* if (polygon.IsValid)
            {
                return polygon;
            }
            var geomFactory = new GeometryFactory();

            var exteriorRing = polygon.ExteriorRing.Coordinates.ToLineRing();
            var holes = polygon.Holes.Select(h => h.Coordinates.ToLineRing()).ToArray();

            polygon = geomFactory.CreatePolygon(exteriorRing, holes);
            
            if (!polygon.IsValid)
            {
                return (Polygon)polygon.Buffer(0);
            }

            return polygon;*/
        }

        /// <summary>
        /// Cast polygon to coordinates
        /// </summary>
        /// <param name="polygon"></param>
        /// <returns></returns>
        private static GeoJsonPolygonCoordinates<GeoJson2DGeographicCoordinates> ToGeoJsonPolygonCoordinates(this Polygon polygon)
        {
            var exteriorRing = GeoJson.LinearRingCoordinates(polygon.ExteriorRing.Coordinates.Select(er => GeoJson.Geographic(er.X, er.Y)).ToArray());
            var holes = polygon.Holes.Select(h => GeoJson.LinearRingCoordinates(h.Coordinates.Select(er => GeoJson.Geographic(er.X, er.Y)).ToArray())).ToArray();
            var coordinates = GeoJson.PolygonCoordinates(exteriorRing, holes);

            return coordinates;
        }


        /// <summary>
        /// Transform coordinates
        /// </summary>
        /// <param name="geometry"></param>
        /// <param name="mathTransform"></param>
        /// <returns></returns>
        private static Geometry Transform(this Geometry geometry, MathTransform mathTransform)
        {
            geometry = geometry.Copy();
            geometry.Apply(new MathTransformFilter(mathTransform));
            return geometry;
        }

        #endregion Private

        #region Public
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pointCoordinates"></param>
        /// <param name="accuracy"></param>
        /// <returns></returns>
        public static Geometry ToCircle(this double[] pointCoordinates, int? accuracy)
        {
            if ((pointCoordinates?.Length ?? 0) != 2)
            {
                return null;
            }

            var longitude = pointCoordinates[0];
            var latitude = pointCoordinates[1];

            var wgs84Point = new Point(longitude, latitude);

            return wgs84Point.ToCircle(accuracy);
        }

        /// <summary>
        /// Transform WGS 84 point to circle by adding a buffer to it
        /// </summary>
        /// <param name="wgs84Point"></param>
        /// <param name="accuracy"></param>
        /// <returns></returns>
        public static Geometry ToCircle(this Point wgs84Point, int? accuracy)
        {
            if (accuracy == null || accuracy < 0.0)
            {
                return null;
            }

            // Add buffer to point to create a circle. If accuracy equals 0, add one meter in order to make a polygon. 
            var circle =
                wgs84Point.Buffer(CalculateDegreesFromMeters(wgs84Point.Y, (double) (accuracy == 0 ? 1 : accuracy)));
            return circle;

            // Transform to SWEREF99 TM since it's in meters
            //  var sweRef99TMPoint = Transform(wgs84Point, CoordinateSys.WGS84, CoordinateSys.SWEREF99_TM);

            // Add buffer to point to create a circle. If accuracy equals 0, add one meter in order to make a polygon. 
            //   var circle = sweRef99TMPoint.Buffer((double)(accuracy == 0 ? 1 : accuracy));

            // Transform back to WGS84
            // return Transform(circle, CoordinateSys.SWEREF99_TM, CoordinateSys.WGS84);
        }

        /// <summary>
        /// Cast input geometry to geojson geometry
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        public static GeoJsonGeometry<GeoJson2DGeographicCoordinates> ToGeoJsonGeometry(this InputGeometry geometry)
        {
            if (!geometry.IsValid)
            {
                return null;
            }
            
            switch (geometry.Type?.ToLower())
            {
                case "point":
                    var coordinates = geometry.Coordinates.ToArray().Select(p => ((JsonElement) p).GetDouble()).ToArray();
                    return GeoJson.Point(GeoJson.Geographic(coordinates[0], coordinates[1]));
                case "polygon":
                    var polygon = geometry.Coordinates.ToArray()
                        .Select(ring =>
                            ((JsonElement)ring).EnumerateArray().Select(point => point.EnumerateArray().Select(nmr => nmr.GetDouble())));

                    return GeoJson.Polygon(polygon.ToGeoJsonPolygonCoordinates());
                    
                case "multipolygon":
                   var multipolygon = geometry.Coordinates.ToArray()
                        .Select(polygon => ((JsonElement)polygon).EnumerateArray()
                            .Select(ring =>
                                ring.EnumerateArray().Select(point => point.EnumerateArray().Select(nmr => nmr.GetDouble()))));

                   return GeoJson.MultiPolygon(multipolygon.Select(p => p.ToGeoJsonPolygonCoordinates()).ToArray());

                default:
                    return null;
            }
        }

        /// <summary>
        /// Cast geometry to a mongodb friendly object
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        public static GeoJsonGeometry<GeoJson2DGeographicCoordinates> ToGeoJsonGeometry(this Geometry geometry)
        {
            if (geometry?.Coordinates == null)
            {
                return null;
            }
            
            switch (geometry.OgcGeometryType)
            {
                case OgcGeometryType.Point:
                    var point = (Point)geometry;

                    return GeoJson.Point(GeoJson.Geographic(point.X, point.Y));
                case OgcGeometryType.LineString:
                    var lineString = (LineString)geometry;

                    return GeoJson.LineString(lineString.Coordinates.Select(p => GeoJson.Geographic(p.X, p.Y)).ToArray());
                case OgcGeometryType.MultiLineString:
                    var multiLineString = (MultiLineString)geometry;

                    return GeoJson.MultiLineString(multiLineString.Geometries.Select(mls => GeoJson.LineStringCoordinates(mls.Coordinates.Select(p => GeoJson.Geographic(p.X, p.Y)).ToArray())).ToArray());
                case OgcGeometryType.MultiPoint:
                    var multiPoint = (MultiPoint)geometry;

                    return GeoJson.MultiPoint(multiPoint.Coordinates.Select(p => GeoJson.Geographic(p.X, p.Y)).ToArray());
                case OgcGeometryType.Polygon:
                    var polygon = (Polygon)geometry;

                    return GeoJson.Polygon(polygon.MakeValid().ToGeoJsonPolygonCoordinates());
                case OgcGeometryType.MultiPolygon:
                    var multiPolygon = (MultiPolygon)geometry;

                    return GeoJson.MultiPolygon(multiPolygon.Geometries.Select(p => ((Polygon)p).MakeValid().ToGeoJsonPolygonCoordinates()).ToArray());
                default:
                    throw new ArgumentException($"Not handled geometry type: {geometry.GeometryType}");
            }
        }

        /// <summary>
        /// Cast sql geometry to IGeometry
        /// </summary>
        /// <param name="sqlGeometry"></param>
        /// <returns></returns>
        public static Geometry ToGeometry(
            this SqlGeometry sqlGeometry)
        {
            var factory = new GeometryFactory();
            var wktReader = new WKTReader(factory);
            return wktReader.Read(sqlGeometry.STAsText().ToSqlString().ToString());
        }

        /// <summary>
        /// Transform coordinates
        /// </summary>
        /// <param name="geometry"></param>
        /// <param name="fromCoordinateSystem"></param>
        /// <param name="toCoordinateSystem"></param>
        /// <returns></returns>
        public static Geometry Transform(
            this Geometry geometry,
            CoordinateSys fromCoordinateSystem,
            CoordinateSys toCoordinateSystem)
        {
            // Create coordinate systems
            var coordinateSystemFactory = new CoordinateSystemFactory();
            var sourceCoordinateSystem = coordinateSystemFactory.CreateFromWkt(GetCoordinateSystemWkt(fromCoordinateSystem));
            var targetCoordinateSystem = coordinateSystemFactory.CreateFromWkt(GetCoordinateSystemWkt(toCoordinateSystem));

            // Transform coordinates
            var coordinateTransformationFactory = new CoordinateTransformationFactory();
            var transformFactory = coordinateTransformationFactory.CreateFromCoordinateSystems(sourceCoordinateSystem, targetCoordinateSystem);

            var transformedGeometry = geometry.Transform(transformFactory.MathTransform);
            transformedGeometry.SRID = (int)toCoordinateSystem;

            return transformedGeometry;
        }
        #endregion Public
    }
}
