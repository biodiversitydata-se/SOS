using AgileObjects.AgileMapper.Extensions;
using Elastic.Clients.Elasticsearch;
using MongoDB.Driver.GeoJsonObjectModel;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Triangulate;
using NetTopologySuite.Utilities;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;
using SOS.Lib.Enums;
using SOS.Lib.Models.Gis;
using System.Collections;
using System.Collections.Concurrent;

namespace SOS.Lib.Extensions;

/// <summary>
///     NetTopologySuite extensions
/// </summary>
public static class GISExtensions
{        
    private const int MaxCacheSize = 5_000_000;
    public static int NumberOfCacheHits = 0;
    public static int NumberOfCachedItems = 0;
    private static readonly ConcurrentDictionary<TransformCacheKey, Point> _transformPointCache = new();
    private static readonly HashSet<CoordinateSys> _roundedCoordinateSystems = new()
    {
        CoordinateSys.ETRS89_LAEA_Europe,
        CoordinateSys.Rt90_25_gon_v,
        CoordinateSys.SWEREF99_TM,
        CoordinateSys.WebMercator
    };

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

    extension(IEnumerable<Geometry>? geometries)
    {
        public Envelope? CalculateBbox()
        {            
            if (geometries == null || !geometries.Any())
                return null;

            var combinedEnvelope = new Envelope();
            foreach (var geom in geometries)
            {
                if (geom == null) continue;
                combinedEnvelope.ExpandToInclude(geom.EnvelopeInternal);
            }

            if (combinedEnvelope.IsNull)
                return null;

            return combinedEnvelope;
        }
    }

    extension(IEnumerable<Feature>? features)
    {
        public Envelope? CalculateBbox()
        {
            if (features == null) return null;
            
            var geometries = features.Select(f => f?.Geometry);
            return geometries.CalculateBbox();
        }
    }

    extension(IEnumerable<Polygon> polygons)
    {
        private IEnumerable<Point> CalculationPoints(bool useCenterPoint = true)
        {
            if (!polygons?.Any() ?? true)
            {
                return null;
            }

            var points = new HashSet<Point>();
            if (useCenterPoint)
            {
                //Create a geometry with all grid cell points, this is faster than using the gridcells because it's less coordinates
                polygons.ForEach(p => points.Add(p.Centroid));
            }
            else
            {
                foreach (var tile in polygons)
                {
                    tile.Coordinates.ForEach(c => points.Add(new Point(c)));
                }
            }

            return points;
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
            CoordinateSys.ETRS89_LAEA_Europe =>
                @"PROJCS[""ETRS89 / LAEA Europe"",GEOGCS[""ETRS89"",DATUM[""European_Terrestrial_Reference_System_1989"",SPHEROID[""GRS 1980"",6378137,298.257222101,AUTHORITY[""EPSG"",""7019""]],TOWGS84[0,0,0,0,0,0,0],AUTHORITY[""EPSG"",""6258""]],PRIMEM[""Greenwich"",0,AUTHORITY[""EPSG"",""8901""]],UNIT[""degree"",0.0174532925199433,AUTHORITY[""EPSG"",""9122""]],AUTHORITY[""EPSG"",""4258""]],PROJECTION[""Lambert_Azimuthal_Equal_Area""],PARAMETER[""latitude_of_center"",52],PARAMETER[""longitude_of_center"",10],PARAMETER[""false_easting"",4321000],PARAMETER[""false_northing"",3210000],UNIT[""metre"",1,AUTHORITY[""EPSG"",""9001""]],AUTHORITY[""EPSG"",""3035""]]",
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

    extension(ArrayList coordinates)
    {
        private (LinearRing shell, LinearRing[] holes) ToGeometryPolygonCoordinates()
        {
            var rings = coordinates.ToArray()
                .Select(lr => (lr as double[][]).Select(c => new Coordinate(c[0], c[1])).ToArray()).ToArray();
            var shell = new LinearRing(rings.First());
            var holes = rings.Skip(1)?.Select(c => new LinearRing(c))?.ToArray();

            return (shell, holes);
        }
    }

    extension(Polygon polygon)
    {
        /// <summary>
        ///     Cast polygon coordinates to geo json polygon coordinates
        /// </summary>
        /// <returns></returns>
        private ArrayList ToGeoJsonPolygonCoordinates()
        {
            var coordinates = new List<double[][]>();
            var exteriorRing = polygon.ExteriorRing.Coordinates.Select(p => new[] { p.X, p.Y }).ToArray();
            var holes = polygon.Holes.Select(h => h.Coordinates.Select(p => new[] { p.X, p.Y }).ToArray()).ToArray();

            coordinates.Add(exteriorRing);
            coordinates.AddRange(holes);

            return new ArrayList(coordinates.ToArray());
        }
    }

    extension(LinearRing linearRing)
    {
        /// <summary>
        /// Try to make LinearRing valid
        /// </summary>
        /// <returns></returns>
        private LinearRing TryMakeRingValid()
        {
            var coordinates = TryMakeRingValid<Coordinate>(linearRing?.Coordinates);

            if (coordinates?.Any() ?? false)
            {
                return Geometry.DefaultFactory.CreateLinearRing(coordinates as Coordinate[]);
            }

            return null;
        }
    }

    extension<T>(IEnumerable<T> linearRing)
    {
        private IEnumerable<T> TryMakeRingValid()
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
    }

    extension(Envelope boundingBox)
    {
        #endregion Private

        #region Public

        /// <summary>
        /// Make envelope as small as possible
        /// </summary>
        /// <param name="geometry"></param>
        /// <param name="maxDistanceFromPoint"></param>
        /// <returns></returns>
        public Envelope AdjustByGeometry(
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
    }

    extension(Envelope envelope)
    {
        public long CalculateNumberOfTiles(int zoom)
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

        public LatLonBoundingBox ToLatLonBoundingBox()
        {            
            return LatLonBoundingBox.Create(envelope);
        }

        public Envelope Transform(
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
    }

    extension(Polygon[] polygons)
    {
        /// <summary>
        /// Calculate traingels from point set
        /// </summary>
        /// <param name="useCenterPoint"></param>
        /// <returns></returns>
        public MultiPolygon CalculateTraiangels(bool useCenterPoint = true)
        {
            // If less than 3 polygons, use all polygon coordinates
            var points = polygons.CalculationPoints(useCenterPoint && (polygons?.Length ?? 0) > 2);

            // we need at least tree points to make a traingle
            if ((points?.Count() ?? 0) < 3)
            {
                return null!;
            }

            var triangulationBuilder = new ConformingDelaunayTriangulationBuilder();
            triangulationBuilder.SetSites(new MultiPoint(points.ToArray()));
            var geometryFactory = new GeometryFactory();
            var triangles = triangulationBuilder.GetTriangles(geometryFactory);

            return new MultiPolygon(triangles.Select(t => new Polygon(new LinearRing(t.Coordinates)))?.ToArray());
        }

        /// <summary>
        /// Calculte concave hull for a list of polygons
        /// </summary>
        /// <param name="useCenterPoint">Use polygon center point amd not polygon envelope edges, less points to use in calculation makes this faster</param>
        /// <param name="alphaValue">The target maximum edge length or the target edge length ratio if useEdgeLengthRatio = true</param>
        /// <param name="useEdgeLengthRatio">Use edge length ratio instead of edge length. The edge length ratio is a fraction of the length difference between the 
        /// longest and shortest edges in the Delaunay Triangulation of the input points</param>
        /// <param name="allowHoles"></param>
        /// <returns></returns>
        public Geometry ConcaveHull(bool useCenterPoint = true, double alphaValue = 0, bool useEdgeLengthRatio = false, bool allowHoles = false)
        {
            if (!polygons?.Any() ?? true)
            {
                return null;
            }
            var points = polygons.CalculationPoints(useCenterPoint);
            return points?.ToArray().ConcaveHull(alphaValue, useEdgeLengthRatio, allowHoles);
        }

        /// <summary>
        /// Get the convex hull for a list of polygons
        /// </summary>
        /// <returns></returns>
        public Geometry ConvexHull()
        {
            if (!polygons?.Any() ?? true)
            {
                return null;
            }

            return new MultiPolygon(polygons).ConvexHull();
        }
    }

    extension(Point[] points)
    {
        /// <summary>
        /// Calculte concave hull for a list of points
        /// </summary>
        /// <param name="alphaValue">The target maximum edge length or the target edge length ratio if useEdgeLengthRatio = true</param>
        /// <param name="useEdgeLengthRatio">Use edge length ratio instead of edge length. The edge length ratio is a fraction of the length difference between the 
        /// longest and shortest edges in the Delaunay Triangulation of the input points</param>
        /// <param name="allowHoles"></param>
        /// <returns></returns>
        public Geometry ConcaveHull(double alphaValue = 0, bool useEdgeLengthRatio = false, bool allowHoles = false)
        {
            if (!points?.Any() ?? true)
            {
                return null;
            }

            return useEdgeLengthRatio ?
               NetTopologySuite.Algorithm.Hull.ConcaveHull.ConcaveHullByLengthRatio(new MultiPoint(points), alphaValue, allowHoles)
               :
               NetTopologySuite.Algorithm.Hull.ConcaveHull.ConcaveHullByLength(new MultiPoint(points), alphaValue, allowHoles);
        }
    }

    extension(CoordinateSys coordinateSystem)
    {
        /// <summary>
        ///     Get the EPSG code for the specified coordinate system.
        /// </summary>
        /// <returns>The EPSG code.</returns>
        public string EpsgCode()
        {
            return $"EPSG:{coordinateSystem.Srid()}";
        }

        /// <summary>
        ///     Gets the Srid for the specified coordinate system.
        /// </summary>
        /// <returns>The Srid.</returns>
        public int Srid()
        {
            return (int)coordinateSystem; // the enum value is the same as SRID.
        }
    }

    extension(Geometry geometry)
    {
        /// <summary>
        /// Make geometry valid
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
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
        /// Create a new feature
        /// </summary>
        /// <param name="attributes"></param>
        /// <returns></returns>
        public Feature ToFeature(IEnumerable<KeyValuePair<string, object>> attributes)
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
        /// <returns></returns>
        public Geometry TryMakeValid()
        {
            if (geometry == null)
            {
                return null;
            }

            switch (geometry.OgcGeometryType)
            {
                case OgcGeometryType.Polygon:
                    var polygon = (Polygon)geometry;
                    var shell = polygon.Shell.TryMakeRingValid();
                    var holes = polygon.Holes?.Select(h => h.TryMakeRingValid());
                    geometry = Geometry.DefaultFactory.CreatePolygon(shell, holes?.ToArray()).Buffer(0);
                    break;
                case OgcGeometryType.MultiPolygon:
                    var multiPolygon = (MultiPolygon)geometry;
                    var polygons = multiPolygon.Geometries?.Select(g => g.TryMakeValid() as Polygon)?.ToArray();
                    geometry = Geometry.DefaultFactory.CreateMultiPolygon(polygons);
                    break;
            }
            if (!geometry.IsValid)
            {
                geometry = NetTopologySuite.Geometries.Utilities.GeometryFixer.Fix(geometry);
            }
            return geometry;
        }

        /// <summary>
        /// Cast geometry to feature
        /// </summary>
        /// <param name="attributes"></param>
        /// <returns></returns>
        public IFeature ToFeature(IDictionary<string, object> attributes = null)
        {
            return geometry == null ? null : new Feature { Geometry = geometry, Attributes = attributes == null ? null : new AttributesTable(attributes) };
        }

        /// <summary>
        ///     Cast geometry to geo json
        /// </summary>
        /// <returns></returns>
        public GeoJsonGeometry<GeoJson2DCoordinates> ToGeoJson()
        {
            if (geometry?.Coordinates == null)
            {
                return null;
            }

            switch (geometry.OgcGeometryType)
            {
                case OgcGeometryType.Point:
                    var point = (Point)geometry;
                    return new GeoJsonPoint<GeoJson2DCoordinates>(new GeoJson2DCoordinates(point.Coordinate.X, point.Coordinate.Y));
                case OgcGeometryType.Polygon:
                    var polygon = (Polygon)geometry;

                    return new GeoJsonPolygon<GeoJson2DCoordinates>(new GeoJsonPolygonCoordinates<GeoJson2DCoordinates>(
                            new GeoJsonLinearRingCoordinates<GeoJson2DCoordinates>(
                                polygon.ExteriorRing?.Coordinates?.Select(c => new GeoJson2DCoordinates(c.X, c.Y))
                            ),
                            polygon.Holes?.Select(h =>
                                new GeoJsonLinearRingCoordinates<GeoJson2DCoordinates>(
                                    h.Coordinates?.Select(c => new GeoJson2DCoordinates(c.X, c.Y))
                                )
                            )
                        )
                    );
                case OgcGeometryType.MultiPolygon:
                    var multiPolygon = (MultiPolygon)geometry;
                    var multiPolygonCoordinates = new List<GeoJsonPolygonCoordinates<GeoJson2DCoordinates>>();
                    foreach (Polygon poly in multiPolygon.Geometries)
                    {
                        multiPolygonCoordinates.Add(((GeoJsonPolygon<GeoJson2DCoordinates>)poly.ToGeoJson()).Coordinates);
                    }

                    return new GeoJsonMultiPolygon<GeoJson2DCoordinates>(new GeoJsonMultiPolygonCoordinates<GeoJson2DCoordinates>(multiPolygonCoordinates))
    ;
                default:
                    throw new ArgumentException($"Not handled geometry type: {geometry.GeometryType}");
            }
        }
    }

    extension(XYBoundingBox boundinBox)
    {
        /// <summary>
        /// Cast XY bounding box to polygon
        /// </summary>
        /// <returns></returns>
        public Polygon ToPolygon()
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
    }

    extension(Feature feature)
    {
        /// <summary>
        /// Cast fetaure to geo json string
        /// </summary>
        /// <returns></returns>
        public string ToGeoJsonString()
        {
            var geoJsonWriter = new GeoJsonWriter();
            return geoJsonWriter.Write(feature);
        }
    }

    extension(Point point)
    {
        /// <summary>
        ///     Transform WGS 84 point to circle by adding a buffer to it
        /// </summary>
        /// <param name="accuracy"></param>
        /// <returns></returns>
        public Geometry ToCircle(int accuracy)
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

        public LatLonGeoLocation ToGeoLocation()
        {
            if (point == null)
            {
                return null;
            }

            return new LatLonGeoLocation { Lat = point.Coordinate.Y, Lon = point.Coordinate.X };
        }
    }

    extension(LatLonBoundingBox boundingBox)
    {
        public Geometry? ToGeometry()
        {
            return boundingBox?.BottomRight == null || boundingBox?.TopLeft == null ?
                null :
                new Polygon(new LinearRing([
                    new Coordinate(boundingBox.TopLeft.Longitude, boundingBox.TopLeft.Latitude),
                new Coordinate(boundingBox.BottomRight.Longitude, boundingBox.TopLeft.Latitude),
                new Coordinate(boundingBox.BottomRight.Longitude, boundingBox.BottomRight.Latitude),
                new Coordinate(boundingBox.TopLeft.Longitude, boundingBox.BottomRight.Latitude),
                new Coordinate(boundingBox.TopLeft.Longitude, boundingBox.TopLeft.Latitude)
                ])
            );
        }
    }

    extension(GeoJsonGeometry<GeoJson2DCoordinates> geoJsonGeometry)
    {
        /// <summary>
        /// Cast GeoJson point to Geometry
        /// </summary>
        /// <returns></returns>
        public Geometry ToGeometry()
        {
            if (geoJsonGeometry == null)
            {
                return null;
            }
            Geometry geometry = null;
            switch (geoJsonGeometry.Type)
            {
                case MongoDB.Driver.GeoJsonObjectModel.GeoJsonObjectType.Point:
                    var point = (GeoJsonPoint<GeoJson2DCoordinates>)geoJsonGeometry;
                    geometry = Geometry.DefaultFactory.CreatePoint(new Coordinate(point.Coordinates.X,
                        point.Coordinates.Y));
                    break;
                case MongoDB.Driver.GeoJsonObjectModel.GeoJsonObjectType.LineString:
                    var linestring = (GeoJsonLineString<GeoJson2DCoordinates>)geoJsonGeometry;
                    geometry = Geometry.DefaultFactory.CreateLineString(linestring.Coordinates.Positions.Select(c => new Coordinate(c.X, c.Y))?.ToArray());
                    break;
                case MongoDB.Driver.GeoJsonObjectModel.GeoJsonObjectType.Polygon:
                    var polygon = (GeoJsonPolygon<GeoJson2DCoordinates>)geoJsonGeometry;
                    var exterior = new LinearRing(polygon.Coordinates.Exterior.Positions.Select(p =>
                            new Coordinate(p.X, p.Y)).TryMakeRingValid().ToArray());
                    var holes = polygon.Coordinates.Holes.Select(lr =>
                            new LinearRing(lr.Positions.Select(pnt => new Coordinate(pnt.X, pnt.Y)).ToArray()).TryMakeRingValid())
                        .ToArray();
                    geometry = Geometry.DefaultFactory.CreatePolygon(exterior, holes);
                    break;
                case MongoDB.Driver.GeoJsonObjectModel.GeoJsonObjectType.MultiPolygon:
                    var multiPolygons = (GeoJsonMultiPolygon<GeoJson2DCoordinates>)geoJsonGeometry;
                    var polygons = new List<Polygon>();

                    foreach (var polygonCoordinates in multiPolygons.Coordinates.Polygons)
                    {
                        var geoJsonPolygon = new GeoJsonPolygon<GeoJson2DCoordinates>(polygonCoordinates);
                        polygons.Add((Polygon)geoJsonPolygon.ToGeometry());
                    }

                    geometry = Geometry.DefaultFactory.CreateMultiPolygon(polygons.ToArray());
                    break;
            }

            if (!geometry.IsValid)
            {
                geometry = TryMakeValid(geometry);
            }

            return geometry;
        }
    }

    extension(string wkt)
    {
        /// <summary>
        ///     Cast wkt string to IGeometry
        /// </summary>
        /// <returns></returns>
        public Geometry ToGeometry(
    )
        {
            var geometry = _wktReader.Read(wkt);
            if (!geometry.IsValid)
            {
                geometry = TryMakeValid(geometry);
            }
            return geometry;
        }
    }

    extension<T>(T geometry) where T : Geometry
    {
        /// <summary>
        ///  Transform coordinates
        /// </summary>
        /// <param name="fromCoordinateSystem"></param>
        /// <param name="toCoordinateSystem"></param>
        /// <param name="usePointCache"></param>
        /// <returns></returns>
        public T Transform(
            CoordinateSys fromCoordinateSystem,
            CoordinateSys toCoordinateSystem,
            bool usePointCache = true)
        {
            if (geometry == null)
                return null;

            if (fromCoordinateSystem == toCoordinateSystem)
            {
                geometry.SRID = (int)toCoordinateSystem;
                return geometry;
            }

            TransformCacheKey? cacheKey = null;
            if (usePointCache && geometry is Point point)
            {
                var key = new TransformCacheKey(fromCoordinateSystem, toCoordinateSystem, point.Coordinate.X, point.Coordinate.Y);
                if (_transformPointCache.TryGetValue(key, out var cachedPoint))
                {
                    NumberOfCacheHits++;
                    return cachedPoint as T;
                }
                cacheKey = key;
            }

            var mathTransformFilter = MathTransformFilterDictionary[
                new Tuple<CoordinateSys, CoordinateSys>(fromCoordinateSystem, toCoordinateSystem)];

            var transformedGeometry = geometry is Point
                ? new Point(geometry.Coordinate.X, geometry.Coordinate.Y, geometry.Coordinate.Z)
                : geometry.Copy();

            transformedGeometry.Apply(mathTransformFilter);
            transformedGeometry.SRID = (int)toCoordinateSystem;

            if (_roundedCoordinateSystems.Contains(toCoordinateSystem))
            {
                var coords = transformedGeometry.Coordinates;
                for (int i = 0; i < coords.Length; i++)
                {
                    coords[i].X = Math.Round(coords[i].X, 0);
                    coords[i].Y = Math.Round(coords[i].Y, 0);
                }
            }

            if (cacheKey.HasValue && transformedGeometry is Point transformedPoint)
            {
                if (_transformPointCache.Count > MaxCacheSize)
                {
                    _transformPointCache.Clear();
                }

                _transformPointCache.TryAdd(cacheKey.Value, transformedPoint);
                NumberOfCachedItems++;
            }

            return transformedGeometry as T;
        }
    }

    extension(double degrees)
    {
        /// <summary>
        ///     Convert angle to radians
        /// </summary>
        /// <returns></returns>
        public double ToRadians()
        {
            return degrees * Math.PI / 180;
        }
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

        if (strCoordinateSystem.Equals("epsg:3035", StringComparison.OrdinalIgnoreCase))
        {
            coordinateSystem = CoordinateSys.ETRS89_LAEA_Europe;
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

    private readonly struct TransformCacheKey : IEquatable<TransformCacheKey>
    {            
        private const double Tolerance = 1e-5; // WGS 84 coordinates within 1.5 meters is considered equal

        public CoordinateSys From { get; }
        public CoordinateSys To { get; }
        public double X { get; }
        public double Y { get; }

        public TransformCacheKey(CoordinateSys from, CoordinateSys to, double x, double y)
        {
            From = from;
            To = to;
            X = x;
            Y = y;
        }

        public bool Equals(TransformCacheKey other) =>
            From == other.From &&
            To == other.To &&
            AreClose(X, other.X) &&
            AreClose(Y, other.Y);

        public override bool Equals(object obj) =>
            obj is TransformCacheKey other && Equals(other);

        public override int GetHashCode()
        {
            long xRounded = (long)Math.Round(X / Tolerance, MidpointRounding.AwayFromZero);
            long yRounded = (long)Math.Round(Y / Tolerance, MidpointRounding.AwayFromZero);
            return HashCode.Combine(From, To, xRounded, yRounded);
        }

        private static bool AreClose(double a, double b) =>
            Math.Abs(a - b) < Tolerance;
    }

}