namespace SOS.Status.Web.Client.Dtos.SosObsApi;

public class GeoJson2DCoordinates
{
    public double X { get; }
    public double Y { get; }

    public GeoJson2DCoordinates(double x, double y)
    {
        X = x;
        Y = y;
    }
}

public enum GeoJsonObjectType
{
    Point,
    Polygon,
    MultiPolygon
}

// ---- Abstract clas for geometries ----
public abstract class GeoJsonGeometry
{
    public GeoJsonObjectType Type { get; }

    protected GeoJsonGeometry(GeoJsonObjectType type)
    {
        Type = type;
    }
}

// ---- Point ----
public class GeoJsonPoint : GeoJsonGeometry
{
    public GeoJson2DCoordinates Coordinates { get; }

    public GeoJsonPoint(GeoJson2DCoordinates coordinates) : base(GeoJsonObjectType.Point)
    {
        Coordinates = coordinates;
    }
}

// ---- Linear ring ----
public class GeoJsonLinearRingCoordinates
{
    public IReadOnlyList<GeoJson2DCoordinates> Positions { get; }

    public GeoJsonLinearRingCoordinates(IEnumerable<GeoJson2DCoordinates> positions)
    {
        Positions = positions.ToList();
    }
}

// ---- Polygon ----
public class GeoJsonPolygonCoordinates
{
    public GeoJsonLinearRingCoordinates Exterior { get; }
    public IReadOnlyList<GeoJsonLinearRingCoordinates> Holes { get; }

    public GeoJsonPolygonCoordinates(
        GeoJsonLinearRingCoordinates exterior,
        IEnumerable<GeoJsonLinearRingCoordinates>? holes = null)
    {
        Exterior = exterior;
        Holes = holes?.ToList() ?? new List<GeoJsonLinearRingCoordinates>();
    }
}

public class GeoJsonPolygon : GeoJsonGeometry
{
    public GeoJsonPolygonCoordinates Coordinates { get; }

    public GeoJsonPolygon(GeoJsonPolygonCoordinates coordinates)
        : base(GeoJsonObjectType.Polygon)
    {
        Coordinates = coordinates;
    }
}

// ---- MultiPolygon ----
public class GeoJsonMultiPolygonCoordinates
{
    public IReadOnlyList<GeoJsonPolygonCoordinates> Polygons { get; }

    public GeoJsonMultiPolygonCoordinates(IEnumerable<GeoJsonPolygonCoordinates> polygons)
    {
        Polygons = polygons.ToList();
    }
}

public class GeoJsonMultiPolygon : GeoJsonGeometry
{
    public GeoJsonMultiPolygonCoordinates Coordinates { get; }

    public GeoJsonMultiPolygon(GeoJsonMultiPolygonCoordinates coordinates)
        : base(GeoJsonObjectType.MultiPolygon)
    {
        Coordinates = coordinates;
    }
}