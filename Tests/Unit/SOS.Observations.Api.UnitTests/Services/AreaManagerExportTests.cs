using SOS.Api.Contracts.Common.Geo;
using System.IO.Compression;
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NetTopologySuite.Geometries;
using SOS.Api.Contracts.Areas.Responses;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.Gis;
using SOS.Lib.Models.Shared;
using SOS.Observations.Api.Managers;
using Xunit;

namespace SOS.Observations.Api.UnitTests.Services;

public class AreaManagerExportTests
{
    [Theory]
    [InlineData("Point")]
    [InlineData("Polygon")]
    [InlineData("MultiPolygon")]
    public async Task JsonExport_KeepsPascalCaseAreaAndGeometryProperties(string geometryType)
    {
        var area = new SOS.Lib.Models.Shared.Area(AreaType.Municipality, "123")
        {
            Name = "Test area",
            BoundingBox = new LatLonBoundingBoxDto
            {
                TopLeft = new LatLonCoordinateDto(60, 15),
                BottomRight = new LatLonCoordinateDto(59, 16)
            }
        };
        var factory = new GeometryFactory();
        var polygon = factory.CreatePolygon([
            new Coordinate(15, 60),
            new Coordinate(16, 60),
            new Coordinate(16, 59),
            new Coordinate(15, 60)
        ]);
        Geometry geometry = geometryType switch
        {
            "Point" => factory.CreatePoint(new Coordinate(15, 60)),
            "Polygon" => polygon,
            _ => factory.CreateMultiPolygon([polygon])
        };

        var cache = new Mock<IAreaCache>();
        cache.Setup(x => x.GetAsync(area.Id)).ReturnsAsync(area);
        cache.Setup(x => x.GetGeometryAsync(AreaType.Municipality, area.FeatureId)).ReturnsAsync(geometry);
        var manager = new AreaManager(cache.Object, NullLogger<AreaManager>.Instance);

        var zipBytes = await manager.GetZippedAreaAsync(AreaTypeDto.Municipality, area.FeatureId, AreaExportFormat.Json);

        Assert.NotNull(zipBytes);
        using var archive = new ZipArchive(new MemoryStream(zipBytes), ZipArchiveMode.Read);
        using var entry = archive.Entries.Single().Open();
        using var json = await JsonDocument.ParseAsync(entry);
        var root = json.RootElement;
        Assert.Equal("Municipality", root.GetProperty("AreaType").GetString());
        Assert.Equal(1, root.GetProperty("AreaTypeId").GetInt32());
        Assert.Equal("123", root.GetProperty("FeatureId").GetString());
        Assert.Equal("Test area", root.GetProperty("Name").GetString());
        Assert.Equal("Municipality-123", root.GetProperty("Key").GetString());
        Assert.Equal(60, root.GetProperty("BoundingBox").GetProperty("TopLeft").GetProperty("Latitude").GetDouble());

        var exportedGeometry = root.GetProperty("Geometry");
        Assert.Equal(geometryType, exportedGeometry.GetProperty("Type").GetString());
        var coordinates = exportedGeometry.GetProperty("Coordinates");
        Assert.Equal(JsonValueKind.Array, coordinates.ValueKind);
        var firstPosition = geometryType switch
        {
            "Point" => coordinates,
            "Polygon" => coordinates[0][0],
            _ => coordinates[0][0][0]
        };
        Assert.Equal(15, firstPosition[0].GetDouble());
        Assert.Equal(60, firstPosition[1].GetDouble());
        Assert.Equal(new[] { "Coordinates", "Type" }, exportedGeometry.EnumerateObject().Select(property => property.Name));
    }
}
