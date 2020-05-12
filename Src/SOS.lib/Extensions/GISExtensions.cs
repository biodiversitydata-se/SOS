using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.SqlServer.Types;
using MongoDB.Driver.GeoJsonObjectModel;
using Nest;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;
using NetTopologySuite.IO;
using NetTopologySuite.Geometries;
using NetTopologySuite.Utilities;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using GeoJsonObjectType = MongoDB.Driver.GeoJsonObjectModel.GeoJsonObjectType;

namespace SOS.Lib.Extensions
{
    /// <summary>
    /// NetTopologySuite extensions
    /// </summary>
    public static class GISExtensions
    {
        #region Private
        private static readonly ConcurrentDictionary<long, ICoordinateTransformation> TransformationsDictionary = new ConcurrentDictionary<long, ICoordinateTransformation>();
        private static readonly Dictionary<Tuple<CoordinateSys, CoordinateSys>, MathTransformFilter> MathTransformFilterDictionary = new Dictionary<Tuple<CoordinateSys, CoordinateSys>, MathTransformFilter>();

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
                    //return @"PROJCS[""WGS 84 / Pseudo - Mercator"",GEOGCS[""WGS 84"",DATUM[""WGS_1984"",SPHEROID[""WGS 84"",6378137,298.257223563,AUTHORITY[""EPSG"",""7030""]],AUTHORITY[""EPSG"",""6326""]],PRIMEM[""Greenwich"",0,AUTHORITY[""EPSG"",""8901""]],UNIT[""degree"",0.0174532925199433,AUTHORITY[""EPSG"",""9122""]],AUTHORITY[""EPSG"",""4326""]],PROJECTION[""Mercator_1SP""],PARAMETER[""central_meridian"",0],PARAMETER[""scale_factor"",1],PARAMETER[""false_easting"",0],PARAMETER[""false_northing"",0],UNIT[""metre"",1,AUTHORITY[""EPSG"",""9001""]],AXIS[""X"",EAST],AXIS[""Y"",NORTH],EXTENSION[""PROJ4"","" + proj = merc + a = 6378137 + b = 6378137 + lat_ts = 0.0 + lon_0 = 0.0 + x_0 = 0.0 + y_0 = 0 + k = 1.0 + units = m + nadgrids = @null + wktext + no_defs""],AUTHORITY[""EPSG"",""3857""]]";
                case CoordinateSys.Rt90_25_gon_v:
                    return @"PROJCS[""SWEREF99 / RT90 2.5 gon V emulation"", GEOGCS[""SWEREF99"", DATUM[""SWEREF99"", SPHEROID[""GRS 1980"",6378137.0,298.257222101, AUTHORITY[""EPSG"",""7019""]], TOWGS84[0.0,0.0,0.0,0.0,0.0,0.0,0.0], AUTHORITY[""EPSG"",""6619""]], PRIMEM[""Greenwich"",0.0, AUTHORITY[""EPSG"",""8901""]], UNIT[""degree"",0.017453292519943295], AXIS[""Geodetic latitude"",NORTH], AXIS[""Geodetic longitude"",EAST], AUTHORITY[""EPSG"",""4619""]], PROJECTION[""Transverse Mercator""], PARAMETER[""central_meridian"",15.806284529444449], PARAMETER[""latitude_of_origin"",0.0], PARAMETER[""scale_factor"",1.00000561024], PARAMETER[""false_easting"",1500064.274], PARAMETER[""false_northing"",-667.711], UNIT[""m"",1.0], AXIS[""Northing"",NORTH], AXIS[""Easting"",EAST], AUTHORITY[""EPSG"",""3847""]]";
                case CoordinateSys.SWEREF99:
                    return @"GEOGCS[""SWEREF99"", DATUM[""SWEREF99"", SPHEROID[""GRS 1980"", 6378137, 298.257222101, AUTHORITY[""EPSG"", ""7019""]], TOWGS84[0, 0, 0, 0, 0, 0, 0], AUTHORITY[""EPSG"", ""6619""]], PRIMEM[""Greenwich"", 0, AUTHORITY[""EPSG"", ""8901""]], UNIT[""degree"", 0.0174532925199433, AUTHORITY[""EPSG"", ""9122""]], AUTHORITY[""EPSG"", ""4619""]]";
                case CoordinateSys.SWEREF99_TM:
                    return @"PROJCS[""SWEREF99 TM"", GEOGCS[""SWEREF99"", DATUM[""D_SWEREF99"", SPHEROID[""GRS_1980"",6378137,298.257222101]], PRIMEM[""Greenwich"",0], UNIT[""Degree"",0.017453292519943295]], PROJECTION[""Transverse_Mercator""], PARAMETER[""latitude_of_origin"",0], PARAMETER[""central_meridian"",15], PARAMETER[""scale_factor"",0.9996], PARAMETER[""false_easting"",500000], PARAMETER[""false_northing"",0], UNIT[""Meter"",1]]";
                case CoordinateSys.WGS84:
                    return @"GEOGCS[""GCS_WGS_1984"", DATUM[""WGS_1984"", SPHEROID[""WGS_1984"",6378137,298.257223563]], PRIMEM[""Greenwich"",0], UNIT[""Degree"",0.017453292519943295]]";
                case CoordinateSys.ETRS89:
                    return @"PROJCS[""ETRS89 / LAEA Europe"",GEOGCS[""ETRS89"",DATUM[""European_Terrestrial_Reference_System_1989"",SPHEROID[""GRS 1980"",6378137,298.257222101,AUTHORITY[""EPSG"",""7019""]],TOWGS84[0,0,0,0,0,0,0],AUTHORITY[""EPSG"",""6258""]],PRIMEM[""Greenwich"",0,AUTHORITY[""EPSG"",""8901""]],UNIT[""degree"",0.0174532925199433,AUTHORITY[""EPSG"",""9122""]],AUTHORITY[""EPSG"",""4258""]],PROJECTION[""Lambert_Azimuthal_Equal_Area""],PARAMETER[""latitude_of_center"",52],PARAMETER[""longitude_of_center"",10],PARAMETER[""false_easting"",4321000],PARAMETER[""false_northing"",3210000],UNIT[""metre"",1,AUTHORITY[""EPSG"",""9001""]],AUTHORITY[""EPSG"",""3035""]]";
                default:
                    throw new ArgumentException("Not handled coordinate system id " + coordinateSystem);
            }
        }

        /// <summary>
        /// Get the EPSG code for the specified coordinate system.
        /// </summary>
        /// <param name="coordinateSystem">The coordinate system.</param>
        /// <returns>The EPSG code.</returns>
        public static string EpsgCode(this CoordinateSys coordinateSystem)
        {
            return $"EPSG:{coordinateSystem.Srid()}";
        }

        /// <summary>
        /// Gets the Srid for the specified coordinate system.
        /// </summary>
        /// <param name="coordinateSystem">The coordinate system.</param>
        /// <returns>The Srid.</returns>
        public static int Srid(this CoordinateSys coordinateSystem)
        {
            return (int)coordinateSystem; // the enum value is the same as SRID.
        }

        /// <summary>
        /// Parse coordinate system.
        /// </summary>
        /// <param name="strCoordinateSystem"></param>
        /// <returns></returns>
        public static CoordinateSys? ParseCoordinateSystem(string strCoordinateSystem)
        {
            var result = TryParseCoordinateSystem(strCoordinateSystem, out CoordinateSys coordinateSystem);
            if (result) return coordinateSystem;
            return null;
        }

        /// <summary>
        /// Try parse coordinate system.
        /// </summary>
        /// <param name="strCoordinateSystem"></param>
        /// <param name="coordinateSystem"></param>
        /// <returns></returns>
        public static bool TryParseCoordinateSystem(string strCoordinateSystem, out CoordinateSys coordinateSystem)
        {
            strCoordinateSystem = strCoordinateSystem.Trim();

            if (strCoordinateSystem.Equals("epsg:3006", StringComparison.OrdinalIgnoreCase))
            {
                coordinateSystem = CoordinateSys.SWEREF99_TM;
                return true;
            }

            if (strCoordinateSystem.Equals("epsg:3021", StringComparison.OrdinalIgnoreCase))
            {
                coordinateSystem = CoordinateSys.Rt90_25_gon_v;
                return true;
            }

            if (strCoordinateSystem.Equals("epsg:3857", StringComparison.OrdinalIgnoreCase))
            {
                coordinateSystem = CoordinateSys.WebMercator;
                return true;
            }

            if (strCoordinateSystem.Equals("epsg:900913", StringComparison.OrdinalIgnoreCase))
            {
                coordinateSystem = CoordinateSys.WebMercator;
                return true;
            }

            if (strCoordinateSystem.Equals("epsg:4326", StringComparison.OrdinalIgnoreCase))
            {
                coordinateSystem = CoordinateSys.WGS84;
                return true;
            }

            if (strCoordinateSystem.Equals("wgs84", StringComparison.OrdinalIgnoreCase))
            {
                coordinateSystem = CoordinateSys.WGS84;
                return true;
            }

            if (strCoordinateSystem.Equals("wgs 84", StringComparison.OrdinalIgnoreCase))
            {
                coordinateSystem = CoordinateSys.WGS84;
                return true;
            }

            if (strCoordinateSystem.Equals("epsg:4619", StringComparison.OrdinalIgnoreCase))
            {
                coordinateSystem = CoordinateSys.SWEREF99;
                return true;
            }

            if (strCoordinateSystem.Equals("epsg:3035", StringComparison.OrdinalIgnoreCase))
            {
                coordinateSystem = CoordinateSys.ETRS89;
                return true;
            }

            coordinateSystem = default;
            return false;
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
        /// Cast GeoJson polygon coordinates to array list
        /// </summary>
        /// <param name="polygonCoordinates"></param>
        /// <returns></returns>
        private static ArrayList ToArrayList(this GeoJsonPolygonCoordinates<GeoJson2DGeographicCoordinates> polygonCoordinates)
        {
            var coordinates = new ArrayList();
            var exteriorRing = polygonCoordinates.Exterior.Positions.Select(p => new[] { p.Longitude, p.Latitude });
            var holes = polygonCoordinates.Holes.Select(h => h.Positions.Select(p => new[] { p.Longitude, p.Latitude }))?.ToArray();

            coordinates.Add(exteriorRing);
            if (holes?.Any() ?? false)
            {
                coordinates.AddRange(holes);
            }

            return coordinates;
        }

        /// <summary>
        /// Cast array of array of array of double to polygon coordinates
        /// </summary>
        /// <param name="polygon"></param>
        /// <returns></returns>
        private static IEnumerable<IEnumerable<GeoCoordinate>> ToGeoShapePolygonCoordinates(this
            IEnumerable<IEnumerable<IEnumerable<double>>> polygon)
        {
            return polygon.Select(r =>
                  r.Select(p => new GeoCoordinate(p.ElementAt(1), p.ElementAt(0))));
        }        
        /// <summary>
        /// Cast polygon to shape coordinates
        /// </summary>
        /// <param name="polygon"></param>
        /// <returns></returns>
        private static IEnumerable<IEnumerable<GeoCoordinate>> ToGeoShapePolygonCoordinates(this Polygon polygon)
        {
            var coordinates = new List<IEnumerable<GeoCoordinate>>();
            var exteriorRing = polygon.ExteriorRing.Coordinates.Select(p => new GeoCoordinate(p.Y, p.X));
            var holes = polygon.Holes.Select(h => h.Coordinates.Select(p => new GeoCoordinate(p.Y, p.X)));

            coordinates.Add(exteriorRing);
            coordinates.AddRange(holes);

            return coordinates;
        }

        /// <summary>
        /// Cast GeoJson Ploygon coordinates to geo shape polygon coordinates
        /// </summary>
        /// <param name="polygon"></param>
        /// <returns></returns>
        private static IEnumerable<IEnumerable<GeoCoordinate>> ToGeoShapePolygonCoordinates(this GeoJsonPolygonCoordinates<GeoJson2DGeographicCoordinates> polygon)
        {
            var coordinates = new List<IEnumerable<GeoCoordinate>>();
            var exteriorRing = polygon.Exterior.Positions.Select(p => new GeoCoordinate(p.Latitude, p.Longitude));
            var holes = polygon.Holes.Select(h => h.Positions.Select(p => new GeoCoordinate(p.Latitude, p.Longitude)));

            coordinates.Add(exteriorRing);
            coordinates.AddRange(holes);

            return coordinates;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        static GISExtensions()
        {
            CoordinateSystemFactory coordinateSystemFactory = new CoordinateSystemFactory();
            Dictionary<CoordinateSys, CoordinateSystem> coordinateSystemsById = new Dictionary<CoordinateSys, CoordinateSystem>();

            foreach (var coordinateSys in Enum.GetValues(typeof(CoordinateSys)).Cast<CoordinateSys>())
            {
                coordinateSystemsById.Add(coordinateSys, coordinateSystemFactory.CreateFromWkt(GetCoordinateSystemWkt(coordinateSys)));
            }

            var coordinateTransformationFactory = new CoordinateTransformationFactory();
            foreach (var sourceCoordinateSys in Enum.GetValues(typeof(CoordinateSys)).Cast<CoordinateSys>())
            {
                foreach (var targetCoordinateSys in Enum.GetValues(typeof(CoordinateSys)).Cast<CoordinateSys>())
                {
                    ICoordinateTransformation coordinateTransformation = coordinateTransformationFactory.CreateFromCoordinateSystems(
                        coordinateSystemsById[sourceCoordinateSys],
                        coordinateSystemsById[targetCoordinateSys]);

                    MathTransformFilterDictionary.Add(new Tuple<CoordinateSys, CoordinateSys>(sourceCoordinateSys, targetCoordinateSys), new MathTransformFilter(coordinateTransformation.MathTransform));
                }
            }
        }
        /// <summary>
        /// Convert angle to radians
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        private static double ToRadians(this double val)
        {
            return (Math.PI / 180) * val;
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

        #endregion Private

        #region Public

        /// <summary>
        ///  Transform WGS 84 point to circle by adding a buffer to it
        /// </summary>
        /// <param name="point"></param>
        /// <param name="accuracy"></param>
        /// <returns></returns>
        public static Geometry ToCircle(this Point point, int? accuracy)
        {
            if (point?.Coordinate == null || point.Coordinate.X.Equals(0) || point.Coordinate.Y.Equals(0) || accuracy == null || accuracy < 0.0)
            {
                return null;
            }

            Geometry circle = null;
            switch ((CoordinateSys)point.SRID)
            {
                // Metric systems, add buffer to create a circle
                case CoordinateSys.SWEREF99:
                case CoordinateSys.SWEREF99_TM:
                case CoordinateSys.Rt90_25_gon_v:
                    circle = point.Buffer((double)(accuracy == 0 ? 1 : accuracy));
                    break;
                case CoordinateSys.WebMercator:
                case CoordinateSys.WGS84:
                    // Degree systems
                    var diameterInMeters = (double)(accuracy == 0 ? 1 : accuracy * 2);

                    var shapeFactory = new GeometricShapeFactory();
                    shapeFactory.NumPoints = accuracy < 1000 ? 32 : accuracy < 10000 ? 64 : 128;
                    shapeFactory.Centre = point.Coordinate;

                    // Length in meters of 1° of latitude = always 111.32 km
                    shapeFactory.Height = diameterInMeters / 111320d;

                    // Length in meters of 1° of longitude = 40075 km * cos( latitude radian ) / 360
                    shapeFactory.Width = diameterInMeters / (40075000 * Math.Cos(point.Y.ToRadians()) / 360);

                    circle = shapeFactory.CreateCircle();
                    break;
            }

            return circle;
        }

        /// <summary>
        /// Cast point to geo location
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static GeoLocation ToGeoLocation(this Point point)
        {
            if (point?.Coordinate == null || point.Coordinate.X.Equals(0) || point.Coordinate.Y.Equals(0))
            {
                return null;
            }

            return new GeoLocation(point.Y, point.X);
        }

        /// <summary>
        /// Cast GeoJson point geometry to geo location
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        public static GeoLocation ToGeoLocation(this GeoJsonGeometry<GeoJson2DGeographicCoordinates> geometry)
        {
            if (geometry == null || geometry.Type != GeoJsonObjectType.Point)
            {
                return null;
            }

            var point = (GeoJsonPoint<GeoJson2DGeographicCoordinates>)geometry;

            if (point.Coordinates == null || point.Coordinates.Latitude.Equals(0) || point.Coordinates.Longitude.Equals(0))
            {
                return null;
            }

            return new GeoLocation(point.Coordinates.Latitude, point.Coordinates.Longitude);
        }

        public static GeometryGeoJson ToGeoJson(this GeoJsonGeometry<GeoJson2DGeographicCoordinates> geometry)
        {
            if (geometry == null)
            {
                return null;
            }

            switch (geometry.Type)
            {
                case GeoJsonObjectType.Point:
                    var point = (GeoJsonPoint<GeoJson2DGeographicCoordinates>)geometry;
                   
                    return new GeometryGeoJson
                    {
                        Coordinates = new ArrayList { new[] { point.Coordinates.Longitude, point.Coordinates.Latitude } },
                        Type = "Point"
                    };
                case GeoJsonObjectType.Polygon:
                    var polygon = (GeoJsonPolygon<GeoJson2DGeographicCoordinates>)geometry;

                    return new GeometryGeoJson
                    {
                        Coordinates = polygon.Coordinates.ToArrayList(),
                        Type = "Polygon"
                    };
                case GeoJsonObjectType.MultiPolygon:
                    var multipolygon = (GeoJsonMultiPolygon<GeoJson2DGeographicCoordinates>)geometry;

                    var coordinates = new ArrayList();
                    coordinates.AddRange(multipolygon.Coordinates.Polygons.Select(p => p.ToArrayList()).ToArray());

                    return new GeometryGeoJson
                        {
                            Coordinates = coordinates,
                            Type = "MultiPolygon"
                    };
                default:
                    return null;
            }
        }

        /// <summary>
        /// Cast geojson geometry (point) to geo location
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        public static GeoLocation ToGeoLocation(this GeometryGeoJson geometry)
        {
            if (!geometry.IsValid || geometry.Type?.ToLower() != "point")
            {
                return null;
            }

            var coordinates = geometry.Coordinates.ToArray().Select(p => ((JsonElement)p).GetDouble())
                .ToArray();

            if (coordinates?.Length != 2 || coordinates[0].Equals(0) || coordinates[1].Equals(0))
            {
                return null;
            }

            return new GeoLocation(coordinates[1], coordinates[0]);
        }

        /// <summary>
        /// Cast geojson geometry to Geo shape
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        public static IGeoShape ToGeoShape(this GeometryGeoJson geometry)
        {
            if (!geometry.IsValid)
            {
                return null;
            }

            switch (geometry.Type?.ToLower())
            {
                case "point":
                    var coordinates = geometry.Coordinates.ToArray().Select(p => ((JsonElement)p).GetDouble())
                        .ToArray();

                    return new PointGeoShape(new GeoCoordinate(coordinates[1], coordinates[0]));
                case "polygon":
                case "holepolygon":
                    var listPolygons = new List<List<GeoCoordinate>>();                    
                    foreach(JsonElement polygon in geometry.Coordinates)
                    {
                        var listPolygon = new List<GeoCoordinate>();                        
                        foreach(var coord in polygon.EnumerateArray())
                        {                            
                            var coordArray = coord.EnumerateArray();                                                        
                            listPolygon.Add(new GeoCoordinate(coordArray.ElementAt(1).GetDouble(), coordArray.ElementAt(0).GetDouble()));                            
                        }
                        listPolygons.Add(listPolygon);
                    }                    
                    return  new PolygonGeoShape(listPolygons);                    
                case "multipolygon":
                    var listMultiPolygons = new List<List<List<GeoCoordinate>>>();
                    foreach (JsonElement multiPolygon in geometry.Coordinates)
                    {
                        var listMultiPolygon = new List<List<GeoCoordinate>>();
                        foreach (var polygon in multiPolygon.EnumerateArray())
                        {
                            var listPolygon = new List<GeoCoordinate>();
                            foreach (var coord in polygon.EnumerateArray())
                            {
                                var coordArray = coord.EnumerateArray();
                                listPolygon.Add(new GeoCoordinate(coordArray.ElementAt(1).GetDouble(), coordArray.ElementAt(0).GetDouble()));
                            }
                            listMultiPolygon.Add(listPolygon);
                        }
                        listMultiPolygons.Add(listMultiPolygon);
                    }                    
                    return new MultiPolygonGeoShape(listMultiPolygons);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Cast geometry to geo shape
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        public static IGeoShape ToGeoShape(this Geometry geometry)
        {
            if (geometry?.Coordinates == null)
            {
                return null;
            }

            switch (geometry.OgcGeometryType)
            {
                case OgcGeometryType.Point:
                    var point = (Point)geometry;

                    return point.X.Equals(0) || point.Y.Equals(0) ? null : new PointGeoShape(new GeoCoordinate(point.Y, point.X));
                case OgcGeometryType.LineString:
                    var lineString = (LineString)geometry;

                    return new LineStringGeoShape(lineString.Coordinates.Select(p => new GeoCoordinate(p.Y, p.X)));
                case OgcGeometryType.MultiLineString:
                    var multiLineString = (MultiLineString)geometry;

                    return new MultiLineStringGeoShape(multiLineString.Geometries.Select(mls => mls.Coordinates.Select(p => new GeoCoordinate(p.Y, p.X))));
                case OgcGeometryType.MultiPoint:
                    var multiPoint = (MultiPoint)geometry;

                    return new MultiPointGeoShape(multiPoint.Coordinates.Select(p => new GeoCoordinate(p.Y, p.X)));
                case OgcGeometryType.Polygon:
                    var polygon = (Polygon)geometry;

                    return new PolygonGeoShape(polygon.MakeValid().ToGeoShapePolygonCoordinates());
                case OgcGeometryType.MultiPolygon:
                    var multiPolygon = (MultiPolygon)geometry;

                    return new MultiPolygonGeoShape(multiPolygon.Geometries.Select(p => ((Polygon)p).MakeValid().ToGeoShapePolygonCoordinates()));
                default:
                    throw new ArgumentException($"Not handled geometry type: {geometry.GeometryType}");
            }
        }

        public static IGeoShape ToGeoShape(this GeoJsonGeometry<GeoJson2DGeographicCoordinates> geometry)
        {
            if (geometry == null)
            {
                return null;
            }

            switch (geometry.Type)
            {
                case GeoJsonObjectType.Point:
                    var point = (GeoJsonPoint<GeoJson2DGeographicCoordinates>)geometry;

                    return (point.Coordinates?.Latitude.Equals(0) ?? true) || (point.Coordinates?.Longitude.Equals(0) ?? true) ? null : new PointGeoShape(new GeoCoordinate(point.Coordinates.Latitude, point.Coordinates.Longitude));
                case GeoJsonObjectType.LineString:
                    var lineString = (GeoJsonLineString<GeoJson2DGeographicCoordinates>)geometry;

                    return new LineStringGeoShape(lineString.Coordinates.Positions.Select(p => new GeoCoordinate(p.Latitude, p.Longitude)));
                case GeoJsonObjectType.MultiLineString:
                    var multiLineString = (GeoJsonMultiLineString<GeoJson2DGeographicCoordinates>)geometry;

                    return new MultiLineStringGeoShape(multiLineString.Coordinates.LineStrings.Select(mls => mls.Positions.Select(p => new GeoCoordinate(p.Latitude, p.Longitude))));
                case GeoJsonObjectType.MultiPoint:
                    var multiPoint = (GeoJsonMultiPoint<GeoJson2DGeographicCoordinates>)geometry;

                    return new MultiPointGeoShape(multiPoint.Coordinates.Positions.Select(p => new GeoCoordinate(p.Latitude, p.Longitude)));
                case GeoJsonObjectType.Polygon:
                    var polygon = (GeoJsonPolygon<GeoJson2DGeographicCoordinates>)geometry;

                    return new PolygonGeoShape(polygon.Coordinates.ToGeoShapePolygonCoordinates());
                case GeoJsonObjectType.MultiPolygon:
                    var multiPolygon = (GeoJsonMultiPolygon<GeoJson2DGeographicCoordinates>)geometry;

                    return new MultiPolygonGeoShape(multiPolygon.Coordinates.Polygons.Select(p => p.ToGeoShapePolygonCoordinates()));
                default:
                    throw new ArgumentException($"Not handled geometry type: {geometry.Type}");
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
            if (fromCoordinateSystem == toCoordinateSystem)
            {
                return geometry;
            }

            var mathTransformFilter = MathTransformFilterDictionary[new Tuple<CoordinateSys, CoordinateSys>(fromCoordinateSystem, toCoordinateSystem)];
            var transformedGeometry = geometry.Copy();
            transformedGeometry.Apply(mathTransformFilter);
            transformedGeometry.SRID = (int)toCoordinateSystem;
            return transformedGeometry;
        }
        #endregion Public
    }
}