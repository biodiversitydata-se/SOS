using NetTopologySuite.Geometries;
using GeoJSON.Text.Geometry;

namespace SOS.Lib.Extensions.Dto;

/// <summary>Maps programmatically created GeoJSON filter geometries without a JSON round trip.</summary>
internal static class RequestGeometryExtensions
{
    private static readonly GeometryFactory Factory = new(new PrecisionModel(), 4326);

    internal static Geometry? ToSearchGeometry(this IGeometryObject? source) => source switch
    {
        null => null,
        GeoJSON.Text.Geometry.Point point => Factory.CreatePoint(ToCoordinate(point.Coordinates)),
        GeoJSON.Text.Geometry.LineString line => Factory.CreateLineString(ToCoordinates(line)),
        GeoJSON.Text.Geometry.Polygon polygon => ToPolygon(polygon),
        GeoJSON.Text.Geometry.MultiPoint points => Factory.CreateMultiPoint(points.Coordinates
            .Select(point => Factory.CreatePoint(ToCoordinate(point.Coordinates))).ToArray()),
        GeoJSON.Text.Geometry.MultiLineString lines => Factory.CreateMultiLineString(lines.Coordinates
            .Select(line => Factory.CreateLineString(ToCoordinates(line))).ToArray()),
        GeoJSON.Text.Geometry.MultiPolygon polygons => Factory.CreateMultiPolygon(polygons.Coordinates.Select(ToPolygon).ToArray()),
        GeoJSON.Text.Geometry.GeometryCollection collection => Factory.CreateGeometryCollection(collection.Geometries
            .Select(geometry => geometry.ToSearchGeometry() ?? throw new ArgumentException("Missing geometry.")).ToArray()),
        _ => throw new ArgumentException("Unsupported filter geometry.", nameof(source))
    };

    private static NetTopologySuite.Geometries.Polygon ToPolygon(GeoJSON.Text.Geometry.Polygon polygon)
    {
        var rings = polygon.Coordinates.Select(line => Factory.CreateLinearRing(ToCoordinates(line))).ToArray();
        return rings.Length == 0 ? Factory.CreatePolygon() : Factory.CreatePolygon(rings[0], rings[1..]);
    }

    private static Coordinate[] ToCoordinates(GeoJSON.Text.Geometry.LineString line) =>
        line.Coordinates.Select(ToCoordinate).ToArray();

    private static Coordinate ToCoordinate(IPosition position)
    {
        ArgumentNullException.ThrowIfNull(position);
        if (!double.IsFinite(position.Longitude) || !double.IsFinite(position.Latitude) ||
            position.Altitude is double altitude && !double.IsFinite(altitude))
            throw new ArgumentException("Geometry coordinates must be finite.", nameof(position));

        // Searches remain two-dimensional, as with the API reader and previous GeoJsonReader.
        return new Coordinate(position.Longitude, position.Latitude);
    }
}
