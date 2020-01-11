using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GeoAPI.CoordinateSystems;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Geometries;
using NetTopologySuite.CoordinateSystems.Transformations;
using NetTopologySuite.Geometries;
using NetTopologySuite.Utilities;
using ProjNet.Converters.WellKnownText;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;
using SOS.Core.Enums;
using ICoordinateSequenceFilter = GeoAPI.Geometries.ICoordinateSequenceFilter;

namespace ProcessService.Application.GIS
{
    public static class CoordinateConversionManager
    {
        private static readonly ConcurrentDictionary<long, ICoordinateTransformation> TransformationsDictionary = new ConcurrentDictionary<long, ICoordinateTransformation>();

        public static IPoint GetConvertedPoint(
            IPoint point,
            CoordinateSystemId fromCoordinateSystemId,
            CoordinateSystemId toCoordinateSystemId)
        {
            var transformation = GetTransformation(fromCoordinateSystemId, toCoordinateSystemId);
            var transformedPoint = GeometryTransform.TransformPoint(point.Factory, point, transformation.MathTransform);
            return transformedPoint;
        }       

        /// <summary>
        /// Get coordinate converter transformation.
        /// </summary>
        /// <param name="fromCoordinateSystem">From coordinate system.</param>
        /// <param name="toCoordinateSystem">To coordinate system.</param>
        /// <returns>A Coordinate transformation.</returns>
        public static ICoordinateTransformation GetTransformation(
            CoordinateSystemId fromCoordinateSystem,
            CoordinateSystemId toCoordinateSystem)
        {
            ICoordinateTransformation transformation;
            long hashCode = GetTransformationUniqueHashCode(fromCoordinateSystem, toCoordinateSystem);

            transformation = TransformationsDictionary.GetOrAdd(hashCode, x =>
            {                
                var coordinateSystemFactory = new CoordinateSystemFactory();            
                var factory = new CoordinateTransformationFactory();
                var fromSystem = coordinateSystemFactory.CreateFromWkt(GetWkt(fromCoordinateSystem));
                var toSystem = coordinateSystemFactory.CreateFromWkt(GetWkt(toCoordinateSystem));
                return transformation = factory.CreateFromCoordinateSystems(fromSystem, toSystem);
            });        
            
            return transformation;
        }

        /// <summary>
        /// Gets a unique hashcode for two CoordinateSystemId enum values.
        /// </summary>
        private static long GetTransformationUniqueHashCode(
            CoordinateSystemId fromCoordinateSystem,
            CoordinateSystemId toCoordinateSystem)
        {
            return GetTransformationUniqueHashCode((int) fromCoordinateSystem, (int) toCoordinateSystem);
        }

        private static long GetTransformationUniqueHashCode(int x, int y) 
        {
            var a = (ulong)(x >= 0 ? 2 * (long)x : -2 * (long)x - 1);
            var b = (ulong)(y >= 0 ? 2 * (long)y : -2 * (long)y - 1);
            var c = (long)((a >= b ? a * a + a + b : a + b * b) / 2);
            return x < 0 && y < 0 || x >= 0 && y >= 0 ? c : -c - 1;
        }


        public static string GetWkt(CoordinateSystemId coordinateSystemId)
        {
            switch (coordinateSystemId)
            {                
                case CoordinateSystemId.WebMercator:
                    return @"PROJCS[""Google Mercator"",GEOGCS[""WGS 84"",DATUM[""World Geodetic System 1984"",SPHEROID[""WGS 84"", 6378137.0, 298.257223563, AUTHORITY[""EPSG"",""7030""]],AUTHORITY[""EPSG"",""6326""]],PRIMEM[""Greenwich"", 0.0, AUTHORITY[""EPSG"",""8901""]],UNIT[""degree"", 0.017453292519943295], AXIS[""Geodetic latitude"", NORTH], AXIS[""Geodetic longitude"", EAST], AUTHORITY[""EPSG"",""4326""]], PROJECTION[""Mercator_1SP""], PARAMETER[""semi_minor"", 6378137.0], PARAMETER[""latitude_of_origin"", 0.0], PARAMETER[""central_meridian"", 0.0], PARAMETER[""scale_factor"", 1.0], PARAMETER[""false_easting"", 0.0], PARAMETER[""false_northing"", 0.0], UNIT[""m"", 1.0], AXIS[""Easting"", EAST], AXIS[""Northing"", NORTH], AUTHORITY[""EPSG"",""900913""]]";
                case CoordinateSystemId.Rt90_25_gon_v:
                    return @"PROJCS[""SWEREF99 / RT90 2.5 gon V emulation"", GEOGCS[""SWEREF99"", DATUM[""SWEREF99"", SPHEROID[""GRS 1980"",6378137.0,298.257222101, AUTHORITY[""EPSG"",""7019""]], TOWGS84[0.0,0.0,0.0,0.0,0.0,0.0,0.0], AUTHORITY[""EPSG"",""6619""]], PRIMEM[""Greenwich"",0.0, AUTHORITY[""EPSG"",""8901""]], UNIT[""degree"",0.017453292519943295], AXIS[""Geodetic latitude"",NORTH], AXIS[""Geodetic longitude"",EAST], AUTHORITY[""EPSG"",""4619""]], PROJECTION[""Transverse Mercator""], PARAMETER[""central_meridian"",15.806284529444449], PARAMETER[""latitude_of_origin"",0.0], PARAMETER[""scale_factor"",1.00000561024], PARAMETER[""false_easting"",1500064.274], PARAMETER[""false_northing"",-667.711], UNIT[""m"",1.0], AXIS[""Northing"",NORTH], AXIS[""Easting"",EAST], AUTHORITY[""EPSG"",""3847""]]";
                case CoordinateSystemId.SWEREF99:                
                case CoordinateSystemId.SWEREF99_TM:
                    return @"PROJCS[""SWEREF99 TM"", GEOGCS[""SWEREF99"", DATUM[""D_SWEREF99"", SPHEROID[""GRS_1980"",6378137,298.257222101]], PRIMEM[""Greenwich"",0], UNIT[""Degree"",0.017453292519943295]], PROJECTION[""Transverse_Mercator""], PARAMETER[""latitude_of_origin"",0], PARAMETER[""central_meridian"",15], PARAMETER[""scale_factor"",0.9996], PARAMETER[""false_easting"",500000], PARAMETER[""false_northing"",0], UNIT[""Meter"",1]]";
                case CoordinateSystemId.WGS84:
                    return @"GEOGCS[""GCS_WGS_1984"", DATUM[""WGS_1984"", SPHEROID[""WGS_1984"",6378137,298.257223563]], PRIMEM[""Greenwich"",0], UNIT[""Degree"",0.017453292519943295]]";
                default:
                    throw new ArgumentException("Not handled coordinate system id " + coordinateSystemId);
            }
        }

    }  
}               