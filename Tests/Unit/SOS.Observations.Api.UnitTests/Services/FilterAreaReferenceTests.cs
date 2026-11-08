using Microsoft.AspNetCore.Http;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using SOS.Api.Contracts.Common.Geo;
using SOS.Api.Contracts.Areas.Requests;
using SOS.Api.Contracts.Areas.Responses;
using SOS.Api.Contracts.Observations.Requests;
using SOS.Api.Contracts.Observations.Responses;
using SOS.Lib.Enums;
using SOS.Lib.Models.Gis;
using SOS.Lib.Models.Search.Input;
using SOS.Lib.Models.Search.Result;
using SOS.Observations.Api.Services.AdaptiveSearchLocations;
using System.Text.Json;
using Xunit;

namespace SOS.Observations.Api.UnitTests.Services;

public class FilterAreaReferenceTests
{
    [Fact]
    public void ReferencesIncludeFetchArgumentsAndOneGroupForAllOwnGeometries()
    {
        var filter = new GeographicsFilterInputModel
        {
            Areas = [new() { AreaType = AreaTypeDto.County, FeatureId = "a b?å" }, new() { AreaType = AreaTypeDto.County, FeatureId = "a b?å" }],
            ServerGeometries = [new WKTReader().Read("POLYGON ((15 59, 15 60, 16 60, 15 59))"), new Point(15, 59)]
        };
        var references = FilterAreaReferenceFactory.Create(filter, "/api");
        Assert.Equal(2, references.Count);
        Assert.Equal("/api/areas/County/a%20b%3F%C3%A5/geometry", references[0].GeometryUrl);
        Assert.Equal(AreaTypeDto.County, references[0].AreaType);
        Assert.Equal("a b?å", references[0].FeatureId);
        Assert.Equal("FilterArea.OwnGeometries", references[1].FeatureType);
        Assert.Null(references[1].GeometryUrl);
        Assert.Null(references[1].AreaType);
        Assert.Null(references[1].FeatureId);
        using var json = JsonDocument.Parse(JsonSerializer.Serialize(references, new JsonSerializerOptions(JsonSerializerDefaults.Web)));
        Assert.False(json.RootElement[1].TryGetProperty("geometryIndex", out _));
        Assert.Null(filter.Geometries); // No conversion to a duplicate GeoJSON coordinate tree is needed.
    }

    [Fact]
    public void SearchCircleReferencesFirstPointAndOriginalRadius()
    {
        var reference = Assert.Single(FilterAreaReferenceFactory.Create(new GeographicsFilterInputModel
        {
            ServerGeometries = [new Point(15, 59), new Point(16, 60)],
            MaxDistanceFromPoint = 123.5
        }, PathString.Empty));
        Assert.Equal("FilterArea.Polygon", reference.FeatureType);
        Assert.Equal(123.5, reference.MaxDistanceFromPoint);
        Assert.Null(reference.GeometryUrl);
    }

    [Fact]
    public void NoGeographicFilterProducesEmptyReferences()
    {
        Assert.Empty(FilterAreaReferenceFactory.Create(null, PathString.Empty));
        Assert.Empty(FilterAreaReferenceFactory.Create(new GeographicsFilterDto(), PathString.Empty));
        Assert.Empty(FilterAreaReferenceFactory.Create(new GeographicsFilterInputModel { ServerGeometries = [] }, PathString.Empty));
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task LocationsAndGridReturnSameReferencesWithAndWithoutInlineGeometry(bool includeGeometry, bool grid)
    {
        var references = FilterAreaReferenceFactory.Create(new GeographicsFilterDto
        {
            Areas = [new AreaFilterDto { AreaType = AreaTypeDto.County, FeatureId = "1" }]
        }, PathString.Empty);
        var geometry = new WKTReader().Read("POLYGON ((15 59, 15 60, 16 60, 15 59))");
        List<Feature>? geographicAreas = includeGeometry
            ? [new Feature(geometry, new AttributesTable { { "FeatureType", "FilterArea.Area" } })]
            : null;
        var context = new DefaultHttpContext();
        using var stream = new MemoryStream();
        context.Response.Body = stream;
        if (grid)
        {
            await AdaptiveSearchLocationsGeoJsonWriter.WriteGeogridFeatureCollectionAsync(
                new GeoGridTileResult { GridCellTiles = [] }, null, context.Response, geographicAreas,
                TestContext.Current.CancellationToken, references);
        }
        else
        {
            await AdaptiveSearchLocationsGeoJsonWriter.WriteFeatureCollectionAsync([], null, context.Response,
                geographicAreas: geographicAreas,
                cancellationToken: TestContext.Current.CancellationToken, filterAreaReferences: references);
        }

        var body = System.Text.Encoding.UTF8.GetString(stream.ToArray());

        using var document = JsonDocument.Parse(body);
        var root = document.RootElement;
        var reference = Assert.Single(root.GetProperty("filterAreaReferences").EnumerateArray());
        Assert.Equal("County", reference.GetProperty("areaType").GetString());
        Assert.Equal("1", reference.GetProperty("featureId").GetString());
        Assert.Equal("/areas/County/1/geometry", reference.GetProperty("geometryUrl").GetString());
        Assert.False(reference.TryGetProperty("geometry", out _));
        Assert.Equal(includeGeometry ? 1 : 0, root.GetProperty("features").GetArrayLength());
    }

    [Fact]
    public async Task GeogridIsWrittenWithTheSameCellsAndBboxAsTheGeoGridTileResultGeoJson()
    {
        var bbox = LatLonBoundingBoxDto.Create(14, 60, 16, 58);
        var gridResult = new GeoGridTileResult
        {
            Zoom = 8,
            GridCellTileCount = 2,
            GridCellTiles = [GridCellTile.Create("8/138/72", 5, 2), GridCellTile.Create("8/139/73", 1, 1)]
        };
        var area = new Feature(new WKTReader().Read("POLYGON ((15 59, 15 60, 16 60, 15 59))"),
            new AttributesTable { { "FeatureType", "FilterArea.Area" } });
        var context = new DefaultHttpContext();
        using var stream = new MemoryStream();
        context.Response.Body = stream;

        await AdaptiveSearchLocationsGeoJsonWriter.WriteGeogridFeatureCollectionAsync(
            gridResult, bbox, context.Response, [area], TestContext.Current.CancellationToken);

        using var actual = JsonDocument.Parse(stream.ToArray());
        using var expected = JsonDocument.Parse(gridResult.GetFeatureCollectionGeoJson(CoordinateSys.WGS84, bbox));
        Assert.True(JsonElement.DeepEquals(expected.RootElement.GetProperty("bbox"), actual.RootElement.GetProperty("bbox")));
        var expectedCells = expected.RootElement.GetProperty("features").EnumerateArray().ToList();
        var actualFeatures = actual.RootElement.GetProperty("features").EnumerateArray().ToList();
        Assert.Equal(expectedCells.Count + 1, actualFeatures.Count);
        for (var i = 0; i < expectedCells.Count; i++)
        {
            Assert.True(JsonElement.DeepEquals(expectedCells[i].GetProperty("geometry"), actualFeatures[i].GetProperty("geometry")));
            Assert.True(JsonElement.DeepEquals(expectedCells[i].GetProperty("properties"), actualFeatures[i].GetProperty("properties")));
        }

        Assert.Equal("FilterArea.Area", actualFeatures[^1].GetProperty("properties").GetProperty("FeatureType").GetString());
    }
}
