using GeoJSON.Text.Geometry;
using NetTopologySuite.IO;
using System.Text.Json;

namespace SOS.Lib.Extensions.Dto;

/// <summary>Converts NetTopologySuite geometries to GeoJSON response geometries.</summary>
public static class ResponseGeometryExtensions
{
    private static readonly GeoJsonWriter GeometryWriter = new();

    /// <summary>Converts a response point directly, preserving coordinate order and altitude.</summary>
    public static Point? ToGeoJsonPoint(this NetTopologySuite.Geometries.Point? source) =>
        source is null ? null : new Point(new Position(source.Y, source.X, double.IsNaN(source.Z) ? null : source.Z));

    public static IGeometryObject ToGeoJsonGeometry(this NetTopologySuite.Geometries.Geometry source)
    {
        // Retain the existing conversion behavior for empty geometries, whose support
        // depends on the concrete GeoJSON type.
        if (source.IsEmpty)
            return JsonSerializer.Deserialize<IGeometryObject>(GeometryWriter.Write(source))
                ?? throw new JsonException("Could not convert the server geometry to the API contract geometry.");

        return source switch
        {
            NetTopologySuite.Geometries.Point point => point.ToGeoJsonPoint()!,
            NetTopologySuite.Geometries.LineString line => ToLineString(line),
            NetTopologySuite.Geometries.Polygon polygon => ToPolygon(polygon),
            NetTopologySuite.Geometries.MultiPoint points => new MultiPoint(points.Geometries
                .Cast<NetTopologySuite.Geometries.Point>().Select(point => point.ToGeoJsonPoint()!)),
            NetTopologySuite.Geometries.MultiLineString lines => new MultiLineString(lines.Geometries
                .Cast<NetTopologySuite.Geometries.LineString>().Select(ToLineString)),
            NetTopologySuite.Geometries.MultiPolygon polygons => new MultiPolygon(polygons.Geometries
                .Cast<NetTopologySuite.Geometries.Polygon>().Select(ToPolygon)),
            NetTopologySuite.Geometries.GeometryCollection collection => new GeometryCollection(collection.Geometries.Select(ToGeoJsonGeometry)),
            _ => throw new ArgumentException($"Unsupported response geometry: {source.GeometryType}.", nameof(source))
        };
    }

    private static Polygon ToPolygon(NetTopologySuite.Geometries.Polygon polygon) =>
        new([ToLineString(polygon.ExteriorRing), .. polygon.Holes.Select(ToLineString)]);

    private static LineString ToLineString(NetTopologySuite.Geometries.LineString line) =>
        new(line.Coordinates.Select(ToPosition));

    private static Position ToPosition(NetTopologySuite.Geometries.Coordinate coordinate) =>
        new(coordinate.Y, coordinate.X, double.IsNaN(coordinate.Z) ? null : coordinate.Z);
}
