using Microsoft.AspNetCore.Http;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using SOS.Api.Contracts.Areas.Requests;
using SOS.Api.Contracts.Areas.Responses;
using SOS.Api.Contracts.Observations.Requests;
using SOS.Api.Contracts.Observations.Responses;
using SOS.Lib.Models.Search.Input;
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
            ServerGeometries = [new Point(15, 59), new Point(16, 60)], MaxDistanceFromPoint = 123.5
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
        string body;
        if (grid)
        {
            var collection = new FeatureCollection();
            if (includeGeometry) collection.Add(new Feature(geometry, new AttributesTable { { "FeatureType", "FilterArea.Area" } }));
            body = AdaptiveSearchLocationsGeoJsonWriter.AddFilterAreaReferences(new GeoJsonWriter().Write(collection), references);
        }
        else
        {
            var context = new DefaultHttpContext();
            using var stream = new MemoryStream();
            context.Response.Body = stream;
            await AdaptiveSearchLocationsGeoJsonWriter.WriteFeatureCollectionAsync([], null, context.Response,
                geographicAreas: includeGeometry ? [new Feature(geometry, new AttributesTable { { "FeatureType", "FilterArea.Area" } })] : null,
                cancellationToken: TestContext.Current.CancellationToken, filterAreaReferences: references);
            body = System.Text.Encoding.UTF8.GetString(stream.ToArray());
        }

        using var document = JsonDocument.Parse(body);
        var root = document.RootElement;
        var reference = Assert.Single(root.GetProperty("filterAreaReferences").EnumerateArray());
        Assert.Equal("County", reference.GetProperty("areaType").GetString());
        Assert.Equal("1", reference.GetProperty("featureId").GetString());
        Assert.Equal("/areas/County/1/geometry", reference.GetProperty("geometryUrl").GetString());
        Assert.False(reference.TryGetProperty("geometry", out _));
        Assert.Equal(includeGeometry ? 1 : 0, root.GetProperty("features").GetArrayLength());
    }
}
