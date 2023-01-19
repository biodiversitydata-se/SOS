using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using AgileObjects.AgileMapper.Extensions;
using Nest;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Utilities;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;
using SOS.Lib.Enums;
using SOS.Lib.Models.Gis;
using SOS.Lib.Models.Shared;

namespace SOS.Lib.Extensions
{
    /// <summary>
    ///     NetTopologySuite extensions
    /// </summary>
    public static class GISExtensions
    {
        private readonly static IDictionary<string, Point> _transformPointCache = new ConcurrentDictionary<string, Point>();
        private readonly static WKTReader _wktReader;
        /// <summary>
        ///     Constructor
        /// </summary>
        static GISExtensions()
        {
            var coordinateSystemFactory = new CoordinateSystemFactory();
            var coordinateSystemsById = new Dictionary<CoordinateSys, CoordinateSystem>();

            foreach (var coordinateSys in Enum.GetValues(typeof(CoordinateSys)).Cast<CoordinateSys>())
            {
                coordinateSystemsById.Add(coordinateSys,
                    coordinateSystemFactory.CreateFromWkt(GetCoordinateSystemWkt(coordinateSys)));
            }

            var coordinateTransformationFactory = new CoordinateTransformationFactory();
            foreach (var sourceCoordinateSys in Enum.GetValues(typeof(CoordinateSys)).Cast<CoordinateSys>())
            {
                foreach (var targetCoordinateSys in Enum.GetValues(typeof(CoordinateSys)).Cast<CoordinateSys>())
                {
                    var coordinateTransformation = coordinateTransformationFactory.CreateFromCoordinateSystems(
                        coordinateSystemsById[sourceCoordinateSys],
                        coordinateSystemsById[targetCoordinateSys]);

                    MathTransformFilterDictionary.Add(
                        new Tuple<CoordinateSys, CoordinateSys>(sourceCoordinateSys, targetCoordinateSys),
                        new MathTransformFilter(coordinateTransformation.MathTransform));
                }
            }

            _wktReader = new WKTReader(); 
    }

        #region Private

        private static readonly ConcurrentDictionary<long, ICoordinateTransformation> TransformationsDictionary =
            new ConcurrentDictionary<long, ICoordinateTransformation>();

        private static readonly Dictionary<Tuple<CoordinateSys, CoordinateSys>, MathTransformFilter>
            MathTransformFilterDictionary = new Dictionary<Tuple<CoordinateSys, CoordinateSys>, MathTransformFilter>();

        /// <summary>
        ///     Math transform filter
        /// </summary>
        private sealed class MathTransformFilter : ICoordinateSequenceFilter
        {
            private readonly MathTransform _mathTransform;

            public MathTransformFilter(MathTransform mathTransform)
            {
                _mathTransform = mathTransform;
            }

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
        ///     Get coordinate system wkt
        /// </summary>
        /// <param name="coordinateSystem"></param>
        /// <returns></returns>
        private static string GetCoordinateSystemWkt(CoordinateSys coordinateSystem)
        {
            return coordinateSystem switch
            {
                CoordinateSys.ETRS89 =>
                    @"GEOGCS[""ETRS89"", DATUM[""European_Terrestrial_Reference_System_1989"",  SPHEROID[""GRS 1980"", 6378137, 298.257222101, AUTHORITY[""EPSG"", ""7019""]], AUTHORITY[""EPSG"", ""6258""]], PRIMEM[""Greenwich"",0, AUTHORITY[""EPSG"", ""8901""]], UNIT[""degree"", 0.0174532925199433,AUTHORITY[""EPSG"", ""9122""]], AUTHORITY[""EPSG"", ""4258""]]",
                CoordinateSys.Rt90_25_gon_v =>
                    @"PROJCS[""SWEREF99 / RT90 2.5 gon V emulation"", GEOGCS[""SWEREF99"", DATUM[""SWEREF99"", SPHEROID[""GRS 1980"",6378137.0,298.257222101, AUTHORITY[""EPSG"",""7019""]], TOWGS84[0.0,0.0,0.0,0.0,0.0,0.0,0.0], AUTHORITY[""EPSG"",""6619""]], PRIMEM[""Greenwich"",0.0, AUTHORITY[""EPSG"",""8901""]], UNIT[""degree"",0.017453292519943295], AXIS[""Geodetic latitude"",NORTH], AXIS[""Geodetic longitude"",EAST], AUTHORITY[""EPSG"",""4619""]], PROJECTION[""Transverse Mercator""], PARAMETER[""central_meridian"",15.806284529444449], PARAMETER[""latitude_of_origin"",0.0], PARAMETER[""scale_factor"",1.00000561024], PARAMETER[""false_easting"",1500064.274], PARAMETER[""false_northing"",-667.711], UNIT[""m"",1.0], AXIS[""Northing"",NORTH], AXIS[""Easting"",EAST], AUTHORITY[""EPSG"",""3847""]]",
                CoordinateSys.SWEREF99 =>
                    @"GEOGCS[""SWEREF99"", DATUM[""SWEREF99"", SPHEROID[""GRS 1980"", 6378137, 298.257222101, AUTHORITY[""EPSG"", ""7019""]], TOWGS84[0, 0, 0, 0, 0, 0, 0], AUTHORITY[""EPSG"", ""6619""]], PRIMEM[""Greenwich"", 0, AUTHORITY[""EPSG"", ""8901""]], UNIT[""degree"", 0.0174532925199433, AUTHORITY[""EPSG"", ""9122""]], AUTHORITY[""EPSG"", ""4619""]]",
                CoordinateSys.SWEREF99_TM =>
                    @"PROJCS[""SWEREF99 TM"",GEOGCS[""SWEREF99"", DATUM[""SWEREF99"", SPHEROID[""GRS 1980"", 6378137, 298.257222101, AUTHORITY[""EPSG"", ""7019""]], TOWGS84[0, 0, 0, 0, 0, 0, 0], AUTHORITY[""EPSG"", ""6619""]], PRIMEM[""Greenwich"", 0, AUTHORITY[""EPSG"", ""8901""]], UNIT[""degree"", 0.0174532925199433, AUTHORITY[""EPSG"", ""9122""]], AUTHORITY[""EPSG"", ""4619""]], PROJECTION[""Transverse_Mercator""], PARAMETER[""latitude_of_origin"", 0], PARAMETER[""central_meridian"", 15], PARAMETER[""scale_factor"", 0.9996], PARAMETER[""false_easting"", 500000], PARAMETER[""false_northing"", 0], UNIT[""metre"", 1, AUTHORITY[""EPSG"", ""9001""]], AUTHORITY[""EPSG"", ""3006""]]",
                CoordinateSys.WebMercator =>
                    @"PROJCS[""Google Mercator"",GEOGCS[""WGS 84"",DATUM[""World Geodetic System 1984"",SPHEROID[""WGS 84"", 6378137.0, 298.257223563, AUTHORITY[""EPSG"",""7030""]],AUTHORITY[""EPSG"",""6326""]],PRIMEM[""Greenwich"", 0.0, AUTHORITY[""EPSG"",""8901""]],UNIT[""degree"", 0.017453292519943295], AXIS[""Geodetic latitude"", NORTH], AXIS[""Geodetic longitude"", EAST], AUTHORITY[""EPSG"",""4326""]], PROJECTION[""Mercator_1SP""], PARAMETER[""semi_minor"", 6378137.0], PARAMETER[""latitude_of_origin"", 0.0], PARAMETER[""central_meridian"", 0.0], PARAMETER[""scale_factor"", 1.0], PARAMETER[""false_easting"", 0.0], PARAMETER[""false_northing"", 0.0], UNIT[""m"", 1.0], AXIS[""Easting"", EAST], AXIS[""Northing"", NORTH], AUTHORITY[""EPSG"",""900913""]]",
                CoordinateSys.WGS84 =>
                    @"GEOGCS[""GCS_WGS_1984"", DATUM[""WGS_1984"", SPHEROID[""WGS_1984"",6378137,298.257223563]], PRIMEM[""Greenwich"",0], UNIT[""Degree"",0.017453292519943295]]",
                _ => throw new ArgumentException("Not handled coordinate system id " + coordinateSystem)
            };
        }

        /// <summary>
        ///     Cast polygon to shape coordinates
        /// </summary>
        /// <param name="polygon"></param>
        /// <returns></returns>
        private static GeoCoordinate[][] ToGeoShapePolygonCoordinates(this Polygon polygon)
        {
            if ((polygon?.Coordinates?.Length ?? 0) == 0)
            {
                return null!;
            }

            var coordinates = new List<GeoCoordinate[]>();
            var exteriorRing = polygon.ExteriorRing.Coordinates.Select(p => new GeoCoordinate(p.Y, p.X)).ToArray();
            var holes = polygon.Holes.Select(h => h.Coordinates.Select(p => new GeoCoordinate(p.Y, p.X)).ToArray()).ToArray();

            coordinates.Add(exteriorRing);
            coordinates.AddRange(holes);

            return coordinates.ToArray();
        }

        /// <summary>
        ///     Cast geo josn polygon coordinates to geo shape polygon coordinates
        /// </summary>
        /// <param name="coordinates"></param>
        /// <returns></returns>
        private static GeoCoordinate[][] ToGeoShapePolygonCoordinates(this ArrayList coordinates)
        {
            var rings = coordinates.ToArray()
                .Select(lr => (lr as double[][]).Select(c => new GeoCoordinate(c[1], c[0])).ToArray()).ToArray();
            var exteriorRing = rings.First();
            var holes = rings.Skip(1)?.ToArray();

            var newCoordinates = new List<GeoCoordinate[]>();

            newCoordinates.Add(exteriorRing);
            newCoordinates.AddRange(holes);

            return newCoordinates.ToArray();
        }

        private static (LinearRing shell, LinearRing[] holes) ToGeometryPolygonCoordinates(this ArrayList coordinates)
        {
            var rings = coordinates.ToArray()
                .Select(lr => (lr as double[][]).Select(c => new Coordinate(c[0], c[1])).ToArray()).ToArray();
            var shell = new LinearRing(rings.First());
            var holes = rings.Skip(1)?.Select(c => new LinearRing(c))?.ToArray();

            return (shell, holes);
        }

        /// <summary>
        ///     Cast polygon coordinates to geo json polygon coordinates
        /// </summary>
        /// <param name="polygon"></param>
        /// <returns></returns>
        private static ArrayList ToGeoJsonPolygonCoordinates(this Polygon polygon)
        {
            var coordinates = new List<double[][]>();
            var exteriorRing = polygon.ExteriorRing.Coordinates.Select(p => new[] {p.X, p.Y}).ToArray();
            var holes = polygon.Holes.Select(h => h.Coordinates.Select(p => new[] {p.X, p.Y}).ToArray()).ToArray();

            coordinates.Add(exteriorRing);
            coordinates.AddRange(holes);

            return new ArrayList(coordinates.ToArray());
        }

        /// <summary>
        /// Try to make LinearRing valid
        /// </summary>
        /// <param name="linearRing"></param>
        /// <returns></returns>
        private static LinearRing TryMakeRingValid(this LinearRing linearRing)
        {
            var coordinates = TryMakeRingValid<Coordinate>(linearRing?.Coordinates);

            if (coordinates?.Any() ?? false)
            {
                return Geometry.DefaultFactory.CreateLinearRing(coordinates as Coordinate[]);
            }

            return null;
        }

        private static IEnumerable<GeoCoordinate> TryMakeRingValid(this IEnumerable<GeoCoordinate> linearRing)
        {
            return TryMakeRingValid<GeoCoordinate>(linearRing);
        }

        private static IEnumerable<T> TryMakeRingValid<T>(this IEnumerable<T> linearRing)
        {
            // Use hash set, no duplicates will be added
            var validatedCoordinates = new HashSet<T>(linearRing);
            var coordinateCount = validatedCoordinates.Count();
            if (coordinateCount < 3)
            {
                return null;
            }

            // Make sure last coordinate equals first
            var newRingCoordinates = new T[validatedCoordinates.Count + 1];
            validatedCoordinates.CopyTo(newRingCoordinates, 0);
            new[] { newRingCoordinates[0] }.CopyTo(newRingCoordinates, coordinateCount);

            return newRingCoordinates;
        }
        #endregion Private

        #region Public

        /// <summary>
        /// Make envelope as small as possible
        /// </summary>
        /// <param name="geometry"></param>
        /// <param name="boundingBox"></param>
        /// <param name="maxDistanceFromPoint"></param>
        /// <returns></returns>
        public static Envelope AdjustByGeometry(
           this Envelope boundingBox,
           Geometry geometry,
           double? maxDistanceFromPoint)
        {
            if (geometry == null)
            {
                return boundingBox;
            }

            Envelope envelope;
            if (geometry is Point point)
            {
                if (maxDistanceFromPoint.HasValue)
                {
                    var sweref99TmGeom = point.Transform(CoordinateSys.WGS84, CoordinateSys.SWEREF99_TM);
                    var bufferedGeomSweref99Tm = sweref99TmGeom.Buffer(maxDistanceFromPoint.Value);
                    var bufferedGeomWgs84 = bufferedGeomSweref99Tm.Transform(CoordinateSys.SWEREF99_TM, CoordinateSys.WGS84);
                    envelope = bufferedGeomWgs84.EnvelopeInternal;
                }
                else
                {
                    return boundingBox;
                }
            }
            else
            {
                envelope = geometry.EnvelopeInternal;
            }

            if (envelope.IsNull)
            {
                return boundingBox;
            }

            return boundingBox.Intersection(envelope);
        }

        public static long CalculateNumberOfTiles(this Envelope envelope,
            int zoom)
        {
            if (envelope == null)
            {
                return 0;
            }
    
            var tileWidthInDegrees = 360.0 / Math.Pow(2, zoom + 1);
            var latCentre = (envelope.MaxY + envelope.MinY) / 2;
            var tileHeightInDegrees = tileWidthInDegrees * Math.Cos(latCentre.ToRadians());

            var lonDiff = Math.Abs(envelope.MaxX - envelope.MinX);
            var latDiff = Math.Abs(envelope.MaxY - envelope.MinY);
            var maxLonTiles = Math.Ceiling(lonDiff / tileWidthInDegrees);
            var maxLatTiles = Math.Ceiling(latDiff / tileHeightInDegrees);
            var maxTilesTot = (long)(maxLonTiles * maxLatTiles);

            return maxTilesTot;
        }

        /// <summary>
        /// Calculte concave hull for a list of polygons
        /// </summary>
        /// <param name="points">Points used for calculation</param>
        /// <param name="edgeLength">The target maximum edge length or the target edge length ratio if useEdgeLengthRatio = true</param>
        /// <param name="useEdgeLengthRatio">Use edge length ratio instead of edge length. The edge length ratio is a fraction of the length difference between the 
        /// longest and shortest edges in the Delaunay Triangulation of the input points</param>
        /// <param name="allowHoles"></param>
        /// <returns></returns>
        public static Geometry ConcaveHull(this Point[] points, double edgeLength = 0, bool useEdgeLengthRatio = false, bool allowHoles = false)
        {
            if (!points?.Any() ?? true)
            {
                return null;
            }

            return useEdgeLengthRatio ?
               NetTopologySuite.Algorithm.Hull.ConcaveHull.ConcaveHullByLengthRatio(new MultiPoint(points), edgeLength, allowHoles)
               :
               NetTopologySuite.Algorithm.Hull.ConcaveHull.ConcaveHullByLength(new MultiPoint(points), edgeLength, allowHoles);
        }

        /// <summary>
        /// Calculte concave hull for a list of polygons
        /// </summary>
        /// <param name="polygons">Polygons used for calculation</param>
        /// <param name="useCenterPoint">Use polygon center point amd not polygon envelope edges, less points to use in calculation makes this faster</param>
        /// <param name="edgeLength">The target maximum edge length or the target edge length ratio if useEdgeLengthRatio = true</param>
        /// <param name="useEdgeLengthRatio">Use edge length ratio instead of edge length. The edge length ratio is a fraction of the length difference between the 
        /// longest and shortest edges in the Delaunay Triangulation of the input points</param>
        /// <param name="allowHoles"></param>
        /// <returns></returns>
        public static Geometry ConcaveHull(this Polygon[] polygons, bool useCenterPoint = true, double edgeLength = 0, bool useEdgeLengthRatio = false, bool allowHoles = false)
        {
            var points = new HashSet<Point>();
            if (useCenterPoint)
            {
                //Create a geometry with all grid cell points, this is faster than using the gridcells because it's less coordinates
                polygons.ForEach(p => points.Add(p.Centroid));
            }
            else
            {
                foreach(var polygon in polygons)
                {
                    polygon.Coordinates.ForEach(c => points.Add(new Point(c)));
                }
            }

            return points.ToArray().ConcaveHull(edgeLength, useEdgeLengthRatio, allowHoles);
        }

        public static Geometry ConcaveHull(this MultiPolygon multiPolygon, bool useCenterPoint = true, double edgeLength = 0, bool useEdgeLengthRatio = false, bool allowHoles = false)
        {
            return ConcaveHull(multiPolygon?.Geometries.Select(g => g as Polygon)?.ToArray(), useCenterPoint, edgeLength, useEdgeLengthRatio, allowHoles);
        }

        /// <summary>
        /// Get the convex hull for a list of polygons
        /// </summary>
        /// <param name="polygons">Boundig box as polygon</param>
        /// <returns></returns>
        public static Geometry ConvexHull(this Polygon[] polygons)
        {
            if (!polygons?.Any() ?? true)
            {
                return null;
            }

            return new MultiPolygon(polygons).ConvexHull();
        }

        /// <summary>
        ///     Get the EPSG code for the specified coordinate system.
        /// </summary>
        /// <param name="coordinateSystem">The coordinate system.</param>
        /// <returns>The EPSG code.</returns>
        public static string EpsgCode(this CoordinateSys coordinateSystem)
        {
            return $"EPSG:{coordinateSystem.Srid()}";
        }

        /// <summary>
        /// Make geometry valid
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        public static bool IsValid(this Geometry geometry)
        {
            if (geometry == null)
            {
                return false;
            }

            if (geometry is LinearRing linearRing)
            {
                return linearRing.IsValid && linearRing.IsSimple && linearRing.IsClosed;
            }

            if (geometry is Polygon polygon)
            {
                return polygon.Shell.IsValid() && (polygon.Holes?.Count(h => !h.IsValid()) ?? 0) == 0;
            }

            if (geometry is MultiPolygon multiPolygon)
            {
                return multiPolygon.Geometries.Count(g => !((Polygon)g).IsValid()) == 0;
            }

            return geometry.IsValid;
        }

        /// <summary>
        /// Cast XY bounding box to polygon
        /// </summary>
        /// <param name="boundinBox"></param>
        /// <returns></returns>
        public static Polygon ToPolygon(this XYBoundingBox boundinBox)
        {
            if (boundinBox == null)
            {
                return null!;
            }

            return new Polygon(
                new LinearRing(new[]
                {
                    new Coordinate(boundinBox.TopLeft.X, boundinBox.TopLeft.Y),
                    new Coordinate(boundinBox.BottomRight.X, boundinBox.TopLeft.Y),
                    new Coordinate(boundinBox.BottomRight.X, boundinBox.BottomRight.Y),
                    new Coordinate(boundinBox.TopLeft.X, boundinBox.BottomRight.Y),
                    new Coordinate(boundinBox.TopLeft.X, boundinBox.TopLeft.Y)
                })
            );
        }

        /// <summary>
        /// Create a new feature
        /// </summary>
        /// <param name="geometry"></param>
        /// <param name="attributes"></param>
        /// <returns></returns>
        public static Feature ToFeature(this Geometry geometry, IEnumerable<KeyValuePair<string, object>> attributes)
        {
            if (geometry == null)
            {
                return null!;
            }

            return new Feature(
                geometry,
                new AttributesTable(attributes)
            );
        }

        /// <summary>
        ///     Make polygon valid
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        public static Geometry TryMakeValid(this Geometry geometry)
        {
            if (geometry == null)
            {
                return null;
            }

            switch (geometry.OgcGeometryType)
            {
                case OgcGeometryType.Polygon:
                    var polygon = (Polygon) geometry;

                    var shell = polygon.Shell.TryMakeRingValid();
                    var holes = polygon.Holes?.Select(h => h.TryMakeRingValid());
                    return Geometry.DefaultFactory.CreatePolygon(shell, holes?.ToArray()).Buffer(0);
                case OgcGeometryType.MultiPolygon:
                    var multiPolygon = (MultiPolygon)geometry;

                    var polygons = multiPolygon.Geometries?.Select(g => g.TryMakeValid() as Polygon)?.ToArray();
                    return Geometry.DefaultFactory.CreateMultiPolygon(polygons);
            }

            return geometry;
        }

        public static IGeoShape TryMakeValid(this IGeoShape geoShape)
        {
            if (geoShape == null)
            {
                return null;
            }

            switch (geoShape.Type?.ToLower())
            {
                case "polygon":
                    var polygon = (PolygonGeoShape)geoShape;
                    return new PolygonGeoShape(polygon.Coordinates.Select(c => c.TryMakeRingValid()));
                case "multipolygon":
                    var multiPolygon = (MultiPolygonGeoShape)geoShape;
                    return new MultiPolygonGeoShape(multiPolygon.Coordinates.Select(p => p.Select(c => c.TryMakeRingValid())));
            }

            return geoShape;
        }

        /// <summary>
        ///     Gets the Srid for the specified coordinate system.
        /// </summary>
        /// <param name="coordinateSystem">The coordinate system.</param>
        /// <returns>The Srid.</returns>
        public static int Srid(this CoordinateSys coordinateSystem)
        {
            return (int) coordinateSystem; // the enum value is the same as SRID.
        }

        /// <summary>
        ///     Transform WGS 84 point to circle by adding a buffer to it
        /// </summary>
        /// <param name="point"></param>
        /// <param name="accuracy"></param>
        /// <returns></returns>
        public static Geometry ToCircle(this Point point, int accuracy)
        {
            if (point?.Coordinate == null || point.Coordinate.X <= 0 || point.Coordinate.Y <= 0)
            {
                return null;
            }
            if (accuracy == 0)
            {
                accuracy = 1;
            }

            var shapeFactory = new GeometricShapeFactory();
            
            shapeFactory.NumPoints = accuracy < 1000 ? 32 : accuracy < 10000 ? 64 : 128;
            shapeFactory.Centre = point.Coordinate;
            var diameterInMeters = (double)accuracy * 2;

            switch ((CoordinateSys)point.SRID)
            {
                // Metric systems, add buffer to create a circle
                case CoordinateSys.SWEREF99:
                case CoordinateSys.SWEREF99_TM:
                case CoordinateSys.Rt90_25_gon_v:
                    shapeFactory.Height = diameterInMeters;
                    shapeFactory.Width = diameterInMeters;
                    break;
                default: // Degree systems

                    // Length in meters of 1° of latitude = always 111.32 km
                    shapeFactory.Height = diameterInMeters / 111320d;

                    // Length in meters of 1° of longitude = 40075 km * cos( latitude radian ) / 360
                    shapeFactory.Width = diameterInMeters / (40075000 * Math.Cos(point.Y.ToRadians()) / 360);

                    break;
            }
            
            var circle = shapeFactory.CreateCircle();
            circle.SRID = point.SRID;
            return circle;
        }

        /// <summary>
        /// Cast IGeoShape to feature
        /// </summary>
        /// <param name="geoShape"></param>
        /// <param name="attributes"></param>
        /// <returns></returns>
        public static IFeature ToFeature(this IGeoShape geoShape, IDictionary<string, object> attributes = null)
        {
            return geoShape?.ToFeature(attributes);
        }

        /// <summary>
        /// Cast geometry to feature
        /// </summary>
        /// <param name="geometry"></param>
        /// <param name="attributes"></param>
        /// <returns></returns>
        public static IFeature ToFeature(this Geometry geometry, IDictionary<string, object> attributes = null)
        {
            return geometry == null ? null : new Feature { Geometry = geometry, Attributes = attributes == null ? null : new AttributesTable(attributes) };
        }

        /// <summary>
        /// Cast envelope to geoemtry
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        public static Envelope ToEnvelope(this Geometry geometry)
        {
            return (geometry?.Coordinates?.Any() ?? false) ? new Envelope(geometry.Coordinates) : null;
        }

        /// <summary>
        /// Cast LatLonBoundingBox to envelope
        /// </summary>
        /// <param name="boundingBox"></param>
        /// <returns></returns>
        public static Envelope ToEnvelope(this LatLonBoundingBox boundingBox)
        {
            return boundingBox?.BottomRight == null || boundingBox?.TopLeft == null ? 
                null : 
                new Envelope(new Coordinate(boundingBox.BottomRight.Longitude, boundingBox.BottomRight.Latitude), new Coordinate(boundingBox.TopLeft.Longitude, boundingBox.TopLeft.Latitude));
        }

        /// <summary>
        ///     Cast geometry to geo json
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        public static GeoJsonGeometry ToGeoJson(this Geometry geometry)
        {
            if (geometry?.Coordinates == null)
            {
                return null;
            }

            var coordinates = new ArrayList();
            switch (geometry.OgcGeometryType)
            {
                case OgcGeometryType.Point:
                    var point = (Point) geometry;
                    coordinates.Add(point.Coordinate.X);
                    coordinates.Add(point.Coordinate.Y);
                    break;
                case OgcGeometryType.Polygon:
                    var polygon = (Polygon) geometry;
                    coordinates = polygon.ToGeoJsonPolygonCoordinates();
                    break;
                case OgcGeometryType.MultiPolygon:
                    var multiPolygon = (MultiPolygon) geometry;

                    foreach (var geom in multiPolygon.Geometries)
                    {
                        coordinates.Add(((Polygon) geom).ToGeoJsonPolygonCoordinates());
                    }

                    break;
                default:
                    throw new ArgumentException($"Not handled geometry type: {geometry.GeometryType}");
            }

            return new GeoJsonGeometry
            {
                Type = geometry.OgcGeometryType.ToString(),
                Coordinates = coordinates
            };
        }

        /// <summary>
        ///     Cast geo shape to geo json geometry
        /// </summary>
        /// <param name="geoShape"></param>
        /// <returns></returns>
        public static Geometry ToGeometry(this IGeoShape geoShape)
        {
            if (geoShape == null)
            {
                return null;
            }

            switch (geoShape.Type?.ToLower())
            {
                case "point":
                    var point = (PointGeoShape)geoShape;
                    return Geometry.DefaultFactory.CreatePoint(new Coordinate(point.Coordinates.Longitude,
                        point.Coordinates.Latitude));
                case "linestring":
                    var linestring = (LineString)geoShape;
                    return Geometry.DefaultFactory.CreateLineString(linestring.Coordinates.Select(c => new Coordinate(c.X, c.Y))?.ToArray());
                case "linearring":
                    var linearring = (LinearRing)geoShape;
                    return Geometry.DefaultFactory.CreateLinearRing(linearring.Coordinates.Select(c => new Coordinate(c.X, c.Y))?.ToArray());
                case "polygon":
                    var polygon = (PolygonGeoShape)geoShape;
                    var linearRings = polygon.Coordinates.Select(lr =>
                            new LinearRing(lr.Select(pnt => new Coordinate(pnt.Longitude, pnt.Latitude)).ToArray()).TryMakeRingValid())
                        .ToArray();

                    return Geometry.DefaultFactory.CreatePolygon(linearRings.First(), linearRings.Skip(1)?.ToArray());
                case "multipolygon":
                    var multiPolygons = (MultiPolygonGeoShape)geoShape;
                    var polygons = new List<Polygon>();

                    foreach (var poly in multiPolygons.Coordinates)
                    {
                        var lr = poly.Select(lr =>
                                new LinearRing(lr.Select(pnt => new Coordinate(pnt.Longitude, pnt.Latitude)).ToArray()).TryMakeRingValid())
                            .ToArray();

                        polygons.Add(new Polygon(lr.First(), lr.Skip(1)?.ToArray()));
                    }

                    return Geometry.DefaultFactory.CreateMultiPolygon(polygons.ToArray());
                default:
                    return null;
            }
        }

        /// <summary>
        ///     Cast geojson geometry to geometry
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        public static Geometry ToGeometry(this GeoJsonGeometry geometry)
        {
            if (!geometry?.IsValid ?? true)
            {
                return null;
            }

            switch (geometry.Type?.ToLower())
            {
                case "point":
                    var coordinates = geometry.Coordinates.ToArray().Select(p => (double)p).ToArray();
                    return Geometry.DefaultFactory.CreatePoint(new Coordinate(coordinates[0],
                        coordinates[1]));
                case "polygon":
                case "holepolygon":
                    var (shell,  holes) = geometry.Coordinates.ToGeometryPolygonCoordinates();
                    return Geometry.DefaultFactory.CreatePolygon(shell, holes);
                case "multipolygon":
                    var polygones = new List<Polygon>();
                    foreach (ArrayList polyCoordinates in geometry.Coordinates)
                    {
                        var (polyShell, polyHoles) = polyCoordinates.ToGeometryPolygonCoordinates();
                        polygones.Add(Geometry.DefaultFactory.CreatePolygon(polyShell, polyHoles)); ;
                    }

                    return Geometry.DefaultFactory.CreateMultiPolygon(polygones.ToArray());
                default:
                    return null;
            }
        }

        /// <summary>
        ///     Cast geo shape to geo json geometry
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        public static GeoJsonGeometry ToGeoJson(this IGeoShape geoShape)
        {
            if (geoShape == null)
            {
                return null;
            }

            var coordinates = new ArrayList();
            var type = "";
            switch (geoShape.Type?.ToLower())
            {
                case "point":
                    var point = (PointGeoShape)geoShape;
                    coordinates.Add(point.Coordinates.Longitude);
                    coordinates.Add(point.Coordinates.Latitude);
                    type = "Point"; // Type in correct case
                    break;
                case "polygon":
                    var polygon = (PolygonGeoShape)geoShape;
                    coordinates.AddRange(polygon.Coordinates
                        .Select(ls => ls.Select(pnt => new[] {pnt.Longitude, pnt.Latitude})).ToArray());
                    type = "Polygon"; // Type in correct case
                    break;
                case "multipolygon":
                    var multiPolygons = (MultiPolygonGeoShape)geoShape;
                    coordinates.AddRange(multiPolygons.Coordinates
                        .Select(p => p.Select(ls => ls.Select(pnt => new[] {pnt.Longitude, pnt.Latitude}))).ToArray());
                    type = "MultiPolygon"; // Type in correct case
                    break;
                default:
                    return null;
            }

            return new GeoJsonGeometry
            {
                Type = type,
                Coordinates = coordinates
            };
        }

        /// <summary>
        ///     Cast point to geo location
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
        ///     Cast GeoJsonGeometry (point) to geo location
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        public static GeoLocation ToGeoLocation(this GeoJsonGeometry geometry)
        {
            if (geometry?.Type?.ToLower() != "point")
            {
                return null;
            }

            return new GeoLocation((double) geometry.Coordinates[1], (double) geometry.Coordinates[0]);
        }

        /// <summary>
        ///     Cast IGeoShape (point) to geo location
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        public static GeoLocation ToGeoLocation(this IGeoShape geometry)
        {
            if (geometry?.Type?.ToLower() != "point")
            {
                return null;
            }

            var point = (PointGeoShape) geometry;

            return new GeoLocation(point.Coordinates.Latitude, point.Coordinates.Longitude);
        }

        /// <summary>
        ///     Cast geojson geometry to Geo shape
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        public static IGeoShape ToGeoShape(this GeoJsonGeometry geometry)
        {
            if (!geometry.IsValid)
            {
                return null;
            }

            switch (geometry.Type?.ToLower())
            {
                case "point":
                    var coordinates = geometry.Coordinates.ToArray().Select(p => (double) p).ToArray();
                    return new PointGeoShape(new GeoCoordinate(coordinates[1], coordinates[0]));
                case "polygon":
                case "holepolygon":
                    var polygonCoordinates = geometry.Coordinates.ToGeoShapePolygonCoordinates();
                    return new PolygonGeoShape(polygonCoordinates);
                case "multipolygon":
                    var multiPolygonCoordinates = new List<GeoCoordinate[][]>();
                    foreach (var polyCoorinates in geometry.Coordinates)
                    {
                        multiPolygonCoordinates.Add(((ArrayList) polyCoorinates).ToGeoShapePolygonCoordinates());
                    }

                    return new MultiPolygonGeoShape(multiPolygonCoordinates);
                default:
                    return null;
            }
        }

        /// <summary>
        ///     Cast geometry to geo shape
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
                    var point = (Point) geometry;

                    return point.X.Equals(0) || point.Y.Equals(0)
                        ? null
                        : new PointGeoShape(new GeoCoordinate(point.Y, point.X));
                case OgcGeometryType.LineString:
                    var lineString = (LineString) geometry;

                    return new LineStringGeoShape(lineString.Coordinates.Select(p => new GeoCoordinate(p.Y, p.X)).ToArray());
                case OgcGeometryType.MultiLineString:
                    var multiLineString = (MultiLineString) geometry;

                    return new MultiLineStringGeoShape(multiLineString.Geometries.Select(mls =>
                        mls.Coordinates.Select(p => new GeoCoordinate(p.Y, p.X)).ToArray()));
                case OgcGeometryType.MultiPoint:
                    var multiPoint = (MultiPoint) geometry;

                    return new MultiPointGeoShape(multiPoint.Coordinates.Select(p => new GeoCoordinate(p.Y, p.X)).ToArray());
                case OgcGeometryType.Polygon:
                    var polygon = (Polygon) geometry;

                    return new PolygonGeoShape(((Polygon)polygon.TryMakeValid()).ToGeoShapePolygonCoordinates());
                case OgcGeometryType.MultiPolygon:
                    var multiPolygon = (MultiPolygon) geometry;

                    return new MultiPolygonGeoShape(multiPolygon.Geometries.Select(p =>
                        ((Polygon) p.TryMakeValid()).ToGeoShapePolygonCoordinates()));
                default:
                    throw new ArgumentException($"Not handled geometry type: {geometry.GeometryType}");
            }
        }

        /// <summary>
        ///     Cast wkt string to IGeometry
        /// </summary>
        /// <param name="wkt"></param>
        /// <returns></returns>
        public static Geometry ToGeometry(
            this string wkt)
        {
            var geometry = _wktReader.Read(wkt);
  
            return geometry;
        }

        public static Envelope Transform(
            this Envelope envelope,
            CoordinateSys fromCoordinateSystem,
            CoordinateSys toCoordinateSystem)
        {
            if (envelope == null)
            {
                return null;
            }

            if (fromCoordinateSystem == toCoordinateSystem)
            {
                return envelope;
            }

            var topLeft = new Point(envelope.MinX, envelope.MaxY).Transform(fromCoordinateSystem, toCoordinateSystem);
            var bottomRight = new Point(envelope.MaxX, envelope.MinY).Transform(fromCoordinateSystem, toCoordinateSystem);

            return new Envelope(topLeft.Coordinate.X, bottomRight.Coordinate.X, topLeft.Coordinate.Y,
                bottomRight.Coordinate.Y);
        }

        /// <summary>
        ///  Transform coordinates
        /// </summary>
        /// <param name="geometry"></param>
        /// <param name="fromCoordinateSystem"></param>
        /// <param name="toCoordinateSystem"></param>
        /// <param name="usePointCache"></param>
        /// <returns></returns>
        public static T Transform<T>(
            this T geometry,
            CoordinateSys fromCoordinateSystem,
            CoordinateSys toCoordinateSystem,
            bool usePointCache = true) where T : Geometry
        {
            if (geometry == null)
            {
                return null;
            }

            if (fromCoordinateSystem == toCoordinateSystem)
            {
                geometry.SRID = (int)toCoordinateSystem;
                return geometry;
            }

            var key = string.Empty;
            if (usePointCache && geometry is Point point)
            {
                key = $"{fromCoordinateSystem}:{toCoordinateSystem}:{point.Coordinate.X}:{point.Coordinate.Y}";

                if (_transformPointCache.TryGetValue(key, out var cachedPoint))
                {
                    return cachedPoint as T;
                }
            }

            var mathTransformFilter =
                MathTransformFilterDictionary[
                    new Tuple<CoordinateSys, CoordinateSys>(fromCoordinateSystem, toCoordinateSystem)];
            var transformedGeometry = geometry.Copy();
            transformedGeometry.Apply(mathTransformFilter);
            transformedGeometry.SRID = (int) toCoordinateSystem;

            if (!string.IsNullOrEmpty(key))
            {
                // If we got this far and key is set, try add point to cache
                lock (_transformPointCache)
                {
                    _transformPointCache.TryAdd(key, transformedGeometry as Point);
                }
            }

            return transformedGeometry as T;
        }

        /// <summary>
        ///     Convert angle to radians
        /// </summary>
        /// <param name="degrees"></param>
        /// <returns></returns>
        public static double ToRadians(this double degrees)
        {
            return degrees * Math.PI / 180;
        }

        /// <summary>
        ///     Try parse coordinate system.
        /// </summary>
        /// <param name="strCoordinateSystem"></param>
        /// <param name="coordinateSystem"></param>
        /// <returns></returns>
        public static bool TryParseCoordinateSystem(string strCoordinateSystem, out CoordinateSys coordinateSystem)
        {
            coordinateSystem = default;

            if (string.IsNullOrEmpty(strCoordinateSystem))
            {
                return false;
            }

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

            if (strCoordinateSystem.Equals("epsg:4258", StringComparison.OrdinalIgnoreCase))
            {
                coordinateSystem = CoordinateSys.ETRS89;
                return true;
            }

            return false;
        }
        #endregion Public
    }
}