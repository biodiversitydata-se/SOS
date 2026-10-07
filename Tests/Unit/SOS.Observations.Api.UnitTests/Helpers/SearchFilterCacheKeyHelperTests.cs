using GeoJSON.Text.Geometry;
using NetTopologySuite.IO;
using SOS.Api.Contracts.Aggregations.Requests;
using SOS.Api.Contracts.Areas.Requests;
using SOS.Api.Contracts.Common.Geo;
using SOS.Api.Contracts.Observations.Requests;
using SOS.Lib.JsonConverters;
using SOS.Lib.Models.Search.Input;
using SOS.Observations.Api.Helpers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Xunit;

namespace SOS.Observations.Api.UnitTests.Helpers;

public class SearchFilterCacheKeyHelperTests
{
    private static readonly JsonSerializerOptions CacheOptions = new()
    {
        Converters = { new JsonStringEnumConverter(), new NetTopologySuite.IO.Converters.GeoJsonConverterFactory() }
    };

    [Theory]
    [InlineData(typeof(SearchFilterAggregationDto))]
    [InlineData(typeof(SearchFilterAggregationInternalDto))]
    [InlineData(typeof(SearchFilterInternalDto))]
    public void Serialize_HttpFiltersIncludeServerGeometries(Type filterType)
    {
        const string json = """
            {"geographics":{"geometries":[{"type":"Polygon","coordinates":[[[16,62],[17,62],[17,63],[16,62]]]}]}}
            """;
        var inputOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            Converters = { new GeographicsFilterInputJsonConverter(), new GeometryConverter() }
        };
        var filter = Assert.IsAssignableFrom<SearchFilterBaseDto>(JsonSerializer.Deserialize(json, filterType, inputOptions));
        var input = Assert.IsType<GeographicsFilterInputModel>(filter.Geographics);
        var geometry = Assert.Single(input.ServerGeometries!);

        var cacheJson = JsonNode.Parse(SearchFilterCacheKeyHelper.Serialize(filter, CacheOptions))!;

        Assert.Equal("Polygon", cacheJson["Geographics"]!["Geometries"]![0]!["type"]!.GetValue<string>());
        Assert.Null(input.Geometries);
        Assert.Same(geometry, Assert.Single(input.ServerGeometries!));
        Assert.DoesNotContain("ServerGeometries", cacheJson.ToJsonString());
    }

    [Theory]
    [InlineData("POINT (16 62)", "POINT (17 62)")]
    [InlineData("POINT Z (16 62 10)", "POINT Z (16 62 20)")]
    [InlineData("LINESTRING (16 62, 17 63)", "LINESTRING (16 62, 16 63, 17 63)")]
    [InlineData("POLYGON ((16 62, 17 62, 17 63, 16 63, 16 62))", "POLYGON ((16 62, 17 62, 17 63, 16 62))")]
    [InlineData("POLYGON ((16 62, 17 62, 17 63, 16 63, 16 62))",
        "POLYGON ((16 62, 17 62, 17 63, 16 63, 16 62), (16.2 62.2, 16.2 62.8, 16.8 62.8, 16.8 62.2, 16.2 62.2))")]
    [InlineData("MULTIPOLYGON (((16 62, 17 62, 17 63, 16 63, 16 62)))", "MULTIPOLYGON (((16 62, 17 62, 17 63, 16 62)))")]
    public void Serialize_DifferentServerGeometriesWithSameBoundingBoxProduceDifferentKeys(string firstWkt, string secondWkt)
    {
        var filter = new SearchFilterAggregationDto
        {
            Geographics = new GeographicsFilterInputModel
            {
                BoundingBox = new LatLonBoundingBoxDto
                {
                    TopLeft = new LatLonCoordinateDto { Longitude = 16, Latitude = 63 },
                    BottomRight = new LatLonCoordinateDto { Longitude = 17, Latitude = 62 }
                },
                ServerGeometries = [new WKTReader().Read(firstWkt)]
            }
        };
        var firstJson = SearchFilterCacheKeyHelper.Serialize(filter, CacheOptions);
        Assert.Equal(firstJson, SearchFilterCacheKeyHelper.Serialize(filter, CacheOptions));

        ((GeographicsFilterInputModel)filter.Geographics).ServerGeometries = [new WKTReader().Read(secondWkt)];

        Assert.NotEqual(firstJson, SearchFilterCacheKeyHelper.Serialize(filter, CacheOptions));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Serialize_PreservesOtherFieldsAndDoesNotModifyTheInput(bool camelCase)
    {
        var options = new JsonSerializerOptions(CacheOptions)
        {
            PropertyNamingPolicy = camelCase ? JsonNamingPolicy.CamelCase : null
        };
        var geometry = new WKTReader().Read("POINT (16 62)");
        var input = new GeographicsFilterInputModel
        {
            BoundingBox = new LatLonBoundingBoxDto
            {
                TopLeft = new LatLonCoordinateDto { Longitude = 16, Latitude = 63 },
                BottomRight = new LatLonCoordinateDto { Longitude = 17, Latitude = 62 }
            },
            ServerGeometries = [geometry],
            LocationIds = ["a"], MaxDistanceFromPoint = 100, ConsiderObservationAccuracy = true
        };
        var filter = new SearchFilterInternalDto
        {
            Geographics = input, Taxon = new TaxonFilterDto { Ids = [1, 2] }, ReportedByMe = true
        };
        var dtoFilter = new SearchFilterInternalDto
        {
            Geographics = input.ToSerializableGeographicsFilterDto(), Taxon = filter.Taxon, ReportedByMe = true
        };

        var actual = JsonNode.Parse(SearchFilterCacheKeyHelper.Serialize(filter, options))!;
        Assert.True(JsonNode.DeepEquals(JsonSerializer.SerializeToNode(dtoFilter, options), actual));

        var jsonWithoutBbox = SearchFilterCacheKeyHelper.Serialize(filter, options, ignoreBoundingBox: true);
        var geographicsProperty = camelCase ? "geographics" : "Geographics";
        var bboxProperty = camelCase ? "boundingBox" : "BoundingBox";
        Assert.Null(JsonNode.Parse(jsonWithoutBbox)![geographicsProperty]![bboxProperty]);
        Assert.NotEqual(actual.ToJsonString(options), jsonWithoutBbox);
        Assert.NotNull(input.BoundingBox);
        Assert.Null(input.Geometries);
        Assert.Same(geometry, Assert.Single(input.ServerGeometries!));
        Assert.Equal(100, input.MaxDistanceFromPoint);
    }

    [Fact]
    public void Serialize_DistinguishesNullEmptyAndPopulatedGeometries()
    {
        var input = new GeographicsFilterInputModel();
        var filter = new SearchFilterAggregationDto { Geographics = input };
        var nullJson = SearchFilterCacheKeyHelper.Serialize(filter, CacheOptions);
        input.ServerGeometries = [];
        var emptyJson = SearchFilterCacheKeyHelper.Serialize(filter, CacheOptions);
        input.ServerGeometries.Add(new WKTReader().Read("POINT (16 62)"));
        var populatedJson = SearchFilterCacheKeyHelper.Serialize(filter, CacheOptions);

        Assert.NotEqual(nullJson, emptyJson);
        Assert.NotEqual(emptyJson, populatedJson);
        Assert.NotEqual(nullJson, populatedJson);
    }

    [Fact]
    public void Serialize_OrdinaryDtoKeepsItsGeometryJson()
    {
        var filter = new SearchFilterAggregationDto
        {
            Geographics = new GeographicsFilterDto
            {
                Geometries = [new Point(new Position(62, 16))]
            }
        };

        Assert.Equal(JsonSerializer.Serialize(filter, CacheOptions), SearchFilterCacheKeyHelper.Serialize(filter, CacheOptions));
    }

    [Theory]
    [InlineData("POINT (16 62)")]
    [InlineData("POINT Z (16 62 10)")]
    [InlineData("LINESTRING (16 62, 17 63)")]
    [InlineData("MULTILINESTRING ((16 62, 17 63), (18 64, 19 65))")]
    [InlineData("MULTIPOINT ((16 62), (17 63))")]
    [InlineData("POLYGON ((16 62, 17 62, 17 63, 16 62), (16.1 62.1, 16.2 62.1, 16.2 62.2, 16.1 62.1))")]
    [InlineData("MULTIPOLYGON (((16 62, 17 62, 17 63, 16 62)))")]
    [InlineData("GEOMETRYCOLLECTION (POINT (16 62), LINESTRING (16 62, 17 63))")]
    public void Serialize_ServerGeometryPreservesThePreviousCacheKeyJson(string wkt)
    {
        var input = new GeographicsFilterInputModel
        {
            ServerGeometries = [new WKTReader().Read(wkt)],
            BoundingBox = new LatLonBoundingBoxDto
            {
                TopLeft = new LatLonCoordinateDto { Longitude = 16, Latitude = 63 },
                BottomRight = new LatLonCoordinateDto { Longitude = 17, Latitude = 62 }
            },
            Areas = [new AreaFilterDto { FeatureId = "0180" }],
            ConsiderDisturbanceRadius = true, ConsiderObservationAccuracy = true,
            ConsiderAuthorizationBuffer = false, IncludeObservationsOutsideSweden = true,
            MaxDistanceFromPoint = 250, LocationIds = ["a"], LocationNameFilter = "test*", MaxAccuracy = 500
        };
        var filter = new SearchFilterInternalDto { Geographics = input, ReportedByMe = true };

        foreach (var camelCase in new[] { false, true })
        foreach (var ignoreBoundingBox in new[] { false, true })
        {
            var options = new JsonSerializerOptions(CacheOptions)
            {
                PropertyNamingPolicy = camelCase ? JsonNamingPolicy.CamelCase : null
            };
            string PropertyName(string name) => options.PropertyNamingPolicy?.ConvertName(name) ?? name;
            // Reproduce the old JSON-tree implementation to assert byte-for-byte cache-key compatibility.
            var expected = JsonSerializer.SerializeToNode(filter, options)!;
            var geographics = expected[PropertyName(nameof(filter.Geographics))]!;
            geographics[PropertyName(nameof(input.Geometries))] = JsonSerializer.SerializeToNode(input.ServerGeometries, options);
            if (ignoreBoundingBox) geographics[PropertyName(nameof(input.BoundingBox))] = null;

            Assert.Equal(expected.ToJsonString(options), SearchFilterCacheKeyHelper.Serialize(filter, options, ignoreBoundingBox));
        }
        Assert.NotNull(input.BoundingBox);
        Assert.Null(input.Geometries);
    }

    [Fact]
    public void Serialize_SharedOptionsSupportConcurrentRequestsWithoutChangingTheFilter()
    {
        var options = SearchFilterCacheKeyHelper.SerializerOptions;
        var input = new GeographicsFilterInputModel
        {
            ServerGeometries = [new WKTReader().Read("POINT Z (16 62 10)")],
            BoundingBox = new LatLonBoundingBoxDto()
        };
        var filter = new SearchFilterInternalDto { Geographics = input };
        var expectedWithBbox = SearchFilterCacheKeyHelper.Serialize(filter, options);
        var expectedWithoutBbox = SearchFilterCacheKeyHelper.Serialize(filter, options, ignoreBoundingBox: true);

        Parallel.For(0, 64, index =>
        {
            var ignoreBbox = index % 2 == 0;
            Assert.Equal(ignoreBbox ? expectedWithoutBbox : expectedWithBbox,
                SearchFilterCacheKeyHelper.Serialize(filter, options, ignoreBbox));
        });

        Assert.NotNull(input.BoundingBox);
        Assert.Null(input.Geometries);
        Assert.Single(input.ServerGeometries!);
    }

    [Theory]
    [InlineData("""{"type":"Point","coordinates":[16,62,10]}""")]
    [InlineData("""{"type":"Polygon","coordinates":[[[16,62,10],[17,62,20],[17,63,30],[16,62,10]],[[16.1,62.1,40],[16.2,62.1,50],[16.2,62.2,60],[16.1,62.1,40]]]}""")]
    [InlineData("""{"type":"MultiPolygon","coordinates":[[[[16,62],[17,62],[17,63],[16,62]]]]}""")]
    [InlineData("""{"type":"GeometryCollection","geometries":[{"type":"Point","coordinates":[16,62,10]}]}""")]
    public void Serialize_OrdinaryDtoCanIgnoreBoundingBoxWithoutLosingGeometry(string geometryJson)
    {
        var filter = new SearchFilterDto
        {
            Geographics = new GeographicsFilterDto
            {
                BoundingBox = new LatLonBoundingBoxDto(),
                Geometries = [JsonSerializer.Deserialize<IGeometryObject>(geometryJson)!]
            }
        };
        var expected = JsonSerializer.SerializeToNode(filter, CacheOptions)!;
        expected["Geographics"]!["BoundingBox"] = null;

        Assert.Equal(expected.ToJsonString(CacheOptions), SearchFilterCacheKeyHelper.Serialize(filter, CacheOptions, ignoreBoundingBox: true));
        Assert.NotNull(filter.Geographics.BoundingBox);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CreateKey_PreservesThePreviousHash(bool ignoreBoundingBox)
    {
        const string queryString = "?cultureCode=sv-SE&test=åäö:";
        var filter = new SearchFilterInternalDto
        {
            Geographics = new GeographicsFilterInputModel
            {
                ServerGeometries = [new WKTReader().Read("POINT Z (16 62 10)")],
                BoundingBox = new LatLonBoundingBoxDto()
            }
        };
        var json = SearchFilterCacheKeyHelper.Serialize(filter, SearchFilterCacheKeyHelper.SerializerOptions, ignoreBoundingBox);
        var expected = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(queryString + json)));

        Assert.Equal(expected, SearchFilterCacheKeyHelper.CreateKey(queryString, filter, ignoreBoundingBox));
        Assert.NotEqual(expected, SearchFilterCacheKeyHelper.CreateKey(queryString + "&skip=1", filter, ignoreBoundingBox));
    }

    [Fact]
    public void CreateKey_NullRequestHashesOnlyTheQueryString()
    {
        const string queryString = "?skip=10";
        var expected = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(queryString)));

        Assert.Equal(expected, SearchFilterCacheKeyHelper.CreateKey(queryString, null));
    }
}
