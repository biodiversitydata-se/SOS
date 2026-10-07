using Elastic.Clients.Elasticsearch;
using NetTopologySuite.Geometries;
using SOS.Api.Contracts.Common.Geo;

namespace SOS.Lib.Extensions;

public static class LatLonBoundingBoxExtensions
{
    private static readonly GeometryFactory GeometryFactory = new();

    /// <summary>Returns the same bounding box when both corners exist, otherwise null.</summary>
    public static LatLonBoundingBoxDto? GetCompleteBoundingBox(this LatLonBoundingBoxDto? boundingBox) =>
        boundingBox is { TopLeft: not null, BottomRight: not null } ? boundingBox : null;

    public static Envelope? ToEnvelope(this LatLonBoundingBoxDto? boundingBox)
    {
        return boundingBox.GetCompleteBoundingBox() is { } complete
            ? new Envelope(complete.TopLeft.Longitude, complete.BottomRight.Longitude,
                complete.BottomRight.Latitude, complete.TopLeft.Latitude)
            : null;
    }

    public static Polygon GetPolygon(this LatLonBoundingBoxDto boundingBox,
        CoordinateSys coordinateSystem = CoordinateSys.WGS84, bool counterClockwiseOrder = true)
    {
        var topLeft = boundingBox.TopLeft;
        var bottomRight = boundingBox.BottomRight;
        var coordinates = counterClockwiseOrder
            ? new[]
            {
                new Coordinate(topLeft.Longitude, topLeft.Latitude),
                new Coordinate(topLeft.Longitude, bottomRight.Latitude),
                new Coordinate(bottomRight.Longitude, bottomRight.Latitude),
                new Coordinate(bottomRight.Longitude, topLeft.Latitude),
                new Coordinate(topLeft.Longitude, topLeft.Latitude)
            }
            : new[]
            {
                new Coordinate(topLeft.Longitude, topLeft.Latitude),
                new Coordinate(bottomRight.Longitude, topLeft.Latitude),
                new Coordinate(bottomRight.Longitude, bottomRight.Latitude),
                new Coordinate(topLeft.Longitude, bottomRight.Latitude),
                new Coordinate(topLeft.Longitude, topLeft.Latitude)
            };

        return (Polygon)GeometryFactory.CreatePolygon(coordinates)
            .Transform(CoordinateSys.WGS84, coordinateSystem);
    }

    public static GeoBounds ToGeoBounds(this LatLonBoundingBoxDto boundingBox) =>
        new TopLeftBottomRightGeoBounds
        {
            BottomRight = boundingBox.BottomRight.ToGeoLocation(),
            TopLeft = boundingBox.TopLeft.ToGeoLocation()
        };
}
