using FluentAssertions;
using NetTopologySuite.Geometries;
using SOS.Api.Contracts.Areas.Requests;
using SOS.Api.Contracts.Areas.Responses;
using SOS.Api.Contracts.Aggregations.Requests;
using SOS.Api.Contracts.Common.Geo;
using SOS.Api.Contracts.Observations.Requests;
using SOS.Lib.Extensions.Dto;
using SOS.Lib.JsonConverters;
using SOS.Lib.Models.Search.Input;
using System.Text.Json;
using System.Globalization;
using System.Text.Json.Serialization.Metadata;
using Xunit;

namespace SOS.Lib.UnitTests.JsonConverters;

public class GeographicsFilterInputJsonConverterTests
{
    private static readonly JsonSerializerOptions ServerOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new GeographicsFilterInputJsonConverter(), new GeometryConverter() }
    };

    [Fact]
    public void Read_UsesConfiguredNamingPolicyForOrdinaryGeographicFields()
    {
        var options = new JsonSerializerOptions(ServerOptions) { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };
        const string json = """
            {"geographics":{"max_distance_from_point":250,"consider_observation_accuracy":true,
                "location_name_filter":"test*","geometries":[{"type":"Point","coordinates":[16,62]}]}}
            """;

        var filter = JsonSerializer.Deserialize<SearchFilterDto>(json, options)!;

        var input = filter.Geographics.Should().BeOfType<GeographicsFilterInputModel>().Subject;
        input.MaxDistanceFromPoint.Should().Be(250);
        input.ConsiderObservationAccuracy.Should().BeTrue();
        input.LocationNameFilter.Should().Be("test*");
        input.Geometries.Should().BeNull();
        input.ServerGeometries.Should().ContainSingle();
    }

    [Theory]
    [InlineData("""{"type":"Point","coordinates":[16,62]}""", OgcGeometryType.Point)]
    [InlineData("""{"Type":"Point","Coordinates":[16,62],"ConsiderObservationAccuracy":false}""", OgcGeometryType.Point)]
    [InlineData("""{"Type":"Point","coordinates":[16,62]}""", OgcGeometryType.Point)]
    [InlineData("""{"type":"Point","Coordinates":[16,62]}""", OgcGeometryType.Point)]
    [InlineData("""{"type":"Polygon","coordinates":[[[16,62],[17,62],[17,63],[16,62]]]}""", OgcGeometryType.Polygon)]
    [InlineData("""{"Type":"Polygon","Coordinates":[[[16,62],[17,62],[17,63],[16,62]]]}""", OgcGeometryType.Polygon)]
    [InlineData("""{"type":"MultiPolygon","coordinates":[[[[16,62],[17,62],[17,63],[16,62]]]]}""", OgcGeometryType.MultiPolygon)]
    [InlineData("""{"Type":"MultiPolygon","Coordinates":[[[[16,62],[17,62],[17,63],[16,62]]]]}""", OgcGeometryType.MultiPolygon)]
    [InlineData("""{"type":"LineString","coordinates":[[16,62],[17,63]]}""", OgcGeometryType.LineString)]
    [InlineData("""{"type":"MultiLineString","coordinates":[[[16,62],[17,63]]]}""", OgcGeometryType.MultiLineString)]
    public void Read_CreatesOnlyServerGeometry(string geometryJson, OgcGeometryType type)
    {
        var json = "{\"geographics\":{\"geometries\":[" + geometryJson +
            "],\"maxDistanceFromPoint\":100,\"considerDisturbanceRadius\":true,\"locationIds\":[\"a\"]}}";

        var filter = JsonSerializer.Deserialize<SearchFilterDto>(json, ServerOptions)!;

        var geographics = filter.Geographics.Should().BeOfType<GeographicsFilterInputModel>().Subject;
        geographics.Geometries.Should().BeNull();
        geographics.ServerGeometries.Should().ContainSingle().Which.OgcGeometryType.Should().Be(type);
        geographics.GetSearchGeometries().Should().BeSameAs(geographics.ServerGeometries);
        geographics.MaxDistanceFromPoint.Should().Be(100);
        geographics.ConsiderDisturbanceRadius.Should().BeTrue();
        geographics.LocationIds.Should().Equal("a");
    }

    [Theory]
    [InlineData("""{"type":"Polygon","coordinates":[[[16,62],[17,63]]]}""")]
    [InlineData("""{"Type":"Polygon","Coordinates":[[[16,62],[17,63]]]}""")]
    [InlineData("""{"type":"Polygon","coordinates":[[[16,62],[17,62],[17,63],[16,63]]]}""")]
    [InlineData("""{"Type":"Polygon","Coordinates":[[[16,62],[17,62],[17,63],[16,63]]]}""")]
    public void Read_InvalidPolygon_PreservesBadRequestPath(string geometryJson)
    {
        var json = "{\"geographics\":{\"geometries\":[" + geometryJson + "]}}";

        var action = () => JsonSerializer.Deserialize<SearchFilterDto>(json, ServerOptions);

        action.Should().Throw<JsonException>()
            .Which.Path.Should().Be("$.geographics.geometries[0]");
    }

    [Theory]
    [InlineData(typeof(SearchFilterDto))]
    [InlineData(typeof(SearchFilterInternalDto))]
    [InlineData(typeof(SearchFilterBaseDto))]
    [InlineData(typeof(SearchFilterInternalBaseDto))]
    [InlineData(typeof(SearchFilterAggregationDto))]
    [InlineData(typeof(SearchFilterAggregationInternalDto))]
    public void Read_DegeneratePolygonInSearchFilter_PreservesMessageAndPath(Type filterType)
    {
        const string json = """
            {"Geographics":{"Geometries":[{"Type":"polygon","Coordinates":[[[16,62],[16,62]]]}]}}
            """;
        var action = () => JsonSerializer.Deserialize(json, filterType, ServerOptions);

        var error = action.Should().Throw<JsonException>().Which;
        error.Message.Should().StartWith("Invalid geometry.");
        error.Path.Should().Be("$.geographics.geometries[0]");
    }

    [Theory]
    [InlineData("null")]
    [InlineData("{}")]
    [InlineData("""{"type":{},"coordinates":[16,62]}""")]
    [InlineData("""{"type":"Point","coordinates":[16]}""")]
    [InlineData("""{"type":"Polygon","coordinates":[[[16,62,10],[17,62,10],[17,63,10],[16,62,20]]]}""")]
    [InlineData("""{"type":"Polygon","coordinates":[[[16,62],[17,62],[17,63],[16,62]],[[16.1,62.1],[16.2,62.1],[16.2,62.2],[16.1,62.2]]]}""")]
    [InlineData("""{"type":"MultiPolygon","coordinates":[[[[16,62,10],[17,62,10],[17,63,10],[16,62,20]]]]}""")]
    public void Read_InvalidSecondGeometryPreservesMessageAndIndex(string geometryJson)
    {
        var json = "{\"geographics\":{\"geometries\":[{\"type\":\"Point\",\"coordinates\":[16,62]}," + geometryJson + "]}}";
        var action = () => JsonSerializer.Deserialize<SearchFilterDto>(json, ServerOptions);

        var error = action.Should().Throw<JsonException>().Which;
        error.Message.Should().StartWith("Invalid geometry.");
        error.Path.Should().Be("$.geographics.geometries[1]");
    }

    [Theory]
    [InlineData("null", true)]
    [InlineData("[]", false)]
    public void Read_PreservesNullAndEmptyGeometries(string geometries, bool expectNull)
    {
        var json = "{\"geographics\":{\"geometries\":" + geometries + "}}";
        var filter = JsonSerializer.Deserialize<SearchFilterDto>(json, ServerOptions)!;
        var input = filter.Geographics.Should().BeOfType<GeographicsFilterInputModel>().Subject;

        if (expectNull) input.ServerGeometries.Should().BeNull();
        else input.ServerGeometries.Should().BeEmpty();
        input.Geometries.Should().BeNull();
    }

    [Fact]
    public void QueuedFilter_MaterializesGeoJsonGeometriesForSerialization()
    {
        const string json = """
            {"geographics":{"geometries":[{"type":"Point","coordinates":[16,62]}]}}
            """;
        var filter = JsonSerializer.Deserialize<SearchFilterDto>(json, ServerOptions)!;

        filter.MaterializeGeoJsonGeometriesForSerialization();

        filter.Geographics.Should().BeOfType<GeographicsFilterDto>();
        filter.Geographics!.Geometries.Should().ContainSingle();
        var queuedJson = Newtonsoft.Json.JsonConvert.SerializeObject(filter);
        queuedJson.Should().Contain("Coordinates");
        queuedJson.Should().NotContain("ServerGeometries");
        JsonSerializer.Serialize(filter, ServerOptions).Should().Contain("coordinates");
    }

    [Fact]
    public void ServerGeometry_IsNotSerializedBySystemTextJson()
    {
        var input = new GeographicsFilterInputModel
        {
            ServerGeometries = [new GeometryFactory().CreatePoint(new Coordinate(16, 62))]
        };

        var json = JsonSerializer.Serialize(input);

        json.Should().NotContain("ServerGeometries");
        json.Should().NotContain("serverGeometries");
    }

    [Fact]
    public void GeographicFilterMappings_PreserveEveryContractProperty()
    {
        // Adding a property must also update this sample and the input/export mappings.
        string[] coveredProperties =
        [
            nameof(GeographicsFilterDto.Areas), nameof(GeographicsFilterDto.BoundingBox),
            nameof(GeographicsFilterDto.ConsiderDisturbanceRadius), nameof(GeographicsFilterDto.ConsiderObservationAccuracy),
            nameof(GeographicsFilterDto.ConsiderAuthorizationBuffer), nameof(GeographicsFilterDto.IncludeObservationsOutsideSweden),
            nameof(GeographicsFilterDto.MaxDistanceFromPoint), nameof(GeographicsFilterDto.Geometries),
            nameof(GeographicsFilterDto.LocationIds), nameof(GeographicsFilterDto.LocationNameFilter), nameof(GeographicsFilterDto.MaxAccuracy)
        ];
        typeof(GeographicsFilterDto).GetProperties().Where(property => property.CanWrite)
            .Select(property => property.Name).Should().BeEquivalentTo(coveredProperties,
                "every geographic DTO property must be exercised through HTTP input, preparation and export serialization");
        var source = new GeographicsFilterDto
        {
            Areas = [new AreaFilterDto { AreaType = AreaTypeDto.Municipality, FeatureId = "0180" }],
            BoundingBox = new LatLonBoundingBoxDto
            {
                TopLeft = new LatLonCoordinateDto { Latitude = 63, Longitude = 15 },
                BottomRight = new LatLonCoordinateDto { Latitude = 60, Longitude = 18 }
            },
            ConsiderDisturbanceRadius = true,
            ConsiderObservationAccuracy = true,
            ConsiderAuthorizationBuffer = false,
            IncludeObservationsOutsideSweden = true,
            MaxDistanceFromPoint = 250,
            Geometries = [new GeoJSON.Text.Geometry.Point(new GeoJSON.Text.Geometry.Position(62, 16))],
            LocationIds = ["location-1"],
            LocationNameFilter = "test*",
            MaxAccuracy = 500
        };
        foreach (var property in typeof(GeographicsFilterDto).GetProperties().Where(property => property.CanWrite))
        {
            var defaultValue = property.PropertyType.IsValueType ? Activator.CreateInstance(property.PropertyType) : null;
            property.GetValue(source).Should().NotBe(defaultValue,
                $"{property.Name} must have a non-default sample value to detect a missing mapping");
        }

        var converted = GeographicsFilterInputModel.FromDto(source);
        converted.Geometries.Should().BeNull();
        converted.GetSearchGeometries().Should().BeSameAs(converted.ServerGeometries);
        converted.ToSerializableGeographicsFilterDto().Should().BeEquivalentTo(source);
        GeographicsFilterInputModel.FromDto(converted).Should().BeSameAs(converted);

        var json = JsonSerializer.Serialize(source, ServerOptions);
        var target = JsonSerializer.Deserialize<GeographicsFilterDto>(json, ServerOptions);

        var input = target.Should().BeOfType<GeographicsFilterInputModel>().Subject;
        input.Geometries.Should().BeNull();
        input.ToSerializableGeographicsFilterDto().Should().BeEquivalentTo(source);
        JsonSerializer.Serialize(target, ServerOptions).Should().Be(json);

        var queuedFilter = new SearchFilterDto { Geographics = input };
        queuedFilter.MaterializeGeoJsonGeometriesForSerialization();
        queuedFilter.Geographics.Should().BeEquivalentTo(source);
    }

    [Fact]
    public void Write_ReusesSerializerMetadataWithoutChangingInputOptions()
    {
        var metadataRequests = 0;
        var resolver = new DefaultJsonTypeInfoResolver();
        resolver.Modifiers.Add(info =>
        {
            if (info.Type == typeof(GeographicsFilterDto)) metadataRequests++;
        });
        var options = new JsonSerializerOptions(ServerOptions) { TypeInfoResolver = resolver };
        GeographicsFilterDto input = new GeographicsFilterInputModel
        {
            ServerGeometries = [new GeometryFactory().CreatePoint(new Coordinate(16, 62))]
        };
        var firstJson = JsonSerializer.Serialize(input, options);
        var firstMetadataRequests = metadataRequests;

        for (var index = 0; index < 5; index++)
            JsonSerializer.Serialize(input, options).Should().Be(firstJson);

        metadataRequests.Should().Be(firstMetadataRequests);
        options.Converters.Should().Contain(converter => converter is GeographicsFilterInputJsonConverter);
        JsonSerializer.Deserialize<GeographicsFilterDto>(firstJson, options).Should().BeOfType<GeographicsFilterInputModel>();
    }

    [Fact]
    public void DtoDeserialization_CreatesGeoJsonObjects()
    {
        const string json = """
            {"geographics":{"geometries":[{"type":"Point","coordinates":[16,62]}]}}
            """;

        var filter = JsonSerializer.Deserialize<SearchFilterDto>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web))!;

        filter.Geographics.Should().BeOfType<GeographicsFilterDto>();
        filter.Geographics!.Geometries.Should().ContainSingle();
    }

}
