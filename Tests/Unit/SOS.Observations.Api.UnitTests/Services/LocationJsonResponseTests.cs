using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using SOS.Api.Contracts.Locations.Responses;
using SOS.Lib.Enums;
using SOS.Lib.Extensions.Dto;
using SOS.Lib.JsonConverters;
using SOS.Lib.Models.Processed.Observation;
using SOS.Observations.Api.Services.Locations;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Xunit;
using Location = SOS.Lib.Models.Processed.Observation.Location;

namespace SOS.Observations.Api.UnitTests.Services;

public class LocationJsonResponseTests
{
    private static readonly JsonSerializerOptions Options = CreateOptions();

    [Theory]
    [InlineData("POINT Z (18 59 120.5)")]
    [InlineData("LINESTRING Z (18 59 120, 19 60 130)")]
    [InlineData("POLYGON ((16 60, 19 60, 19 63, 16 63, 16 60), (17 61, 17 62, 18 62, 18 61, 17 61))")]
    [InlineData("MULTIPOINT Z ((18 59 120), (19 60 130))")]
    [InlineData("MULTILINESTRING ((18 59, 19 60), (20 61, 21 62))")]
    [InlineData("MULTIPOLYGON (((16 60, 19 60, 19 63, 16 60)))")]
    [InlineData("GEOMETRYCOLLECTION (POINT (18 59), LINESTRING (19 60, 20 61))")]
    public void Response_PreservesGeometryCoordinatesHolesAndAltitude(string wkt)
    {
        var location = new Location(LocationType.Polygon)
        {
            LocationId = "test:location",
            Point = new Point(new CoordinateZ(18, 59, 120.5)),
            PointWithBuffer = new WKTReader().Read(wkt),
            PointWithDisturbanceBuffer = new WKTReader().Read("POLYGON ((15 59, 20 59, 20 64, 15 59))")
        };

        AssertSameJson(location, Options);
        var clientOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            Converters = { new JsonStringEnumConverter() }
        };
        var result = JsonSerializer.Deserialize<LocationDto>(
            JsonSerializer.Serialize(new LocationJsonResponse(location), Options), clientOptions)!;
        Assert.Equal(120.5, result.Point.Coordinates.Altitude);
        Assert.Equal(18, result.Point.Coordinates.Longitude);
        Assert.Equal(59, result.Point.Coordinates.Latitude);
        Assert.NotNull(result.PointWithBuffer);
        Assert.NotNull(result.PointWithDisturbanceBuffer);
    }

    [Fact]
    public void Response_PreservesAllLocationFieldsAndFlattenedAttributes()
    {
        var location = new Location(LocationType.Point)
        {
            Continent = new VocabularyValue { Id = 2, Value = "Europe" },
            CoordinatePrecision = 0.01,
            CoordinateUncertaintyInMeters = 50,
            Country = new VocabularyValue { Id = 1, Value = "Sweden" },
            CountryRegion = new Area { FeatureId = "SE", Name = "Region" },
            CountryCode = "SE",
            County = new Area { FeatureId = "05", Name = "Östergötland" },
            Municipality = new Area { FeatureId = "0580", Name = "Linköping" },
            Parish = new Area { FeatureId = "P1", Name = "Parish" },
            Province = new Area { FeatureId = "PR1", Name = "Province" },
            DecimalLatitude = 59,
            DecimalLongitude = 18,
            Attributes = new LocationAttributes { ExternalId = "external:1", ProjectId = 42, IsPrivate = true },
            Locality = "Testlokal <ö>",
            LocationAccordingTo = "Source",
            LocationId = "location:1",
            LocationRemarks = "Remarks",
            Point = new Point(18, 59)
        };

        AssertSameJson(location, Options);
    }

    [Fact]
    public void Response_PreservesNullFieldsAndUsesTheSuppliedSerializerSettings()
    {
        var location = new Location { Attributes = null! };
        AssertSameJson(location, Options);
        AssertSameJson(location, CreateOptions(JsonIgnoreCondition.WhenWritingNull));
        var pascalCase = CreateOptions();
        pascalCase.PropertyNamingPolicy = null;
        AssertSameJson(location, pascalCase);
        Assert.Equal("null", JsonSerializer.Serialize(new LocationJsonResponse(null!), Options));
    }

    [Theory]
    [InlineData("POINT EMPTY")]
    [InlineData("LINESTRING EMPTY")]
    [InlineData("POLYGON EMPTY")]
    [InlineData("MULTIPOINT EMPTY")]
    [InlineData("MULTILINESTRING EMPTY")]
    [InlineData("MULTIPOLYGON EMPTY")]
    [InlineData("GEOMETRYCOLLECTION EMPTY")]
    [InlineData("GEOMETRYCOLLECTION (POINT (18 59), POINT EMPTY)")]
    [InlineData("MULTIPOINT (EMPTY, (18 59))")]
    [InlineData("MULTILINESTRING (EMPTY, (18 59, 19 60))")]
    [InlineData("MULTIPOLYGON (EMPTY, ((16 60, 19 60, 19 63, 16 60)))")]
    public void Response_PreservesExistingEmptyGeometryBehavior(string wkt)
    {
        var location = new Location { PointWithBuffer = new WKTReader().Read(wkt) };
        var existingError = Record.Exception(() => JsonSerializer.Serialize(location.ToLocationDto(), Options));
        if (existingError is not null)
        {
            Assert.Throws(existingError.GetType(), () => JsonSerializer.Serialize(new LocationJsonResponse(location), Options));
            return;
        }
        AssertSameJson(location, Options);
    }

    [Fact]
    public void Response_IncludesAdditionalDtoFieldsWithoutUpdatingTheWriter()
    {
        var options = CreateOptions();
        options.TypeInfoResolver = new DefaultJsonTypeInfoResolver
        {
            Modifiers =
            {
                typeInfo =>
                {
                    if (typeInfo.Type != typeof(LocationDto)) return;
                    var property = typeInfo.CreateJsonPropertyInfo(typeof(string), "additionalField");
                    property.Get = location => ((LocationDto)location).LocationId + ":extra";
                    typeInfo.Properties.Add(property);
                }
            }
        };

        AssertSameJson(new Location { LocationId = "location:1", Point = new Point(18, 59) }, options);
    }

    [Fact]
    public void Response_PreservesDtoPropertyNamesConditionsConvertersOrderingAndNumberHandling()
    {
        var options = CreateOptions();
        options.TypeInfoResolver = new DefaultJsonTypeInfoResolver
        {
            Modifiers =
            {
                typeInfo =>
                {
                    if (typeInfo.Type != typeof(LocationDto)) return;
                    var locality = typeInfo.Properties.Single(property => property.Name == "locality");
                    locality.Name = "renamedLocality";
                    locality.Order = -1;
                    locality.CustomConverter = new UppercaseStringConverter();
                    var remarks = typeInfo.Properties.Single(property => property.Name == "locationRemarks");
                    remarks.ShouldSerialize = (location, value) => ((LocationDto)location).ProjectId != 42;
                    var latitude = typeInfo.Properties.Single(property => property.Name == "decimalLatitude");
                    latitude.NumberHandling = JsonNumberHandling.WriteAsString;
                }
            }
        };
        var location = new Location
        {
            Locality = "Testlokal", LocationRemarks = "Not included", DecimalLatitude = 59,
            Attributes = new LocationAttributes { ProjectId = 42 }, Point = new Point(18, 59)
        };

        AssertSameJson(location, options);
        using var json = JsonDocument.Parse(JsonSerializer.Serialize(new LocationJsonResponse(location), options));
        Assert.Equal("renamedLocality", json.RootElement.EnumerateObject().First().Name);
        Assert.Equal("TESTLOKAL", json.RootElement.GetProperty("renamedLocality").GetString());
        Assert.False(json.RootElement.TryGetProperty("locationRemarks", out _));
        Assert.Equal("59", json.RootElement.GetProperty("decimalLatitude").GetString());
    }

    private static void AssertSameJson(Location location, JsonSerializerOptions options)
    {
        using var expected = JsonDocument.Parse(JsonSerializer.Serialize(location.ToLocationDto(), options));
        using var actual = JsonDocument.Parse(JsonSerializer.Serialize(new LocationJsonResponse(location), options));
        Assert.True(JsonElement.DeepEquals(expected.RootElement, actual.RootElement),
            $"Expected: {expected.RootElement}; actual: {actual.RootElement}");
    }

    private sealed class UppercaseStringConverter : JsonConverter<string>
    {
        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            throw new NotSupportedException();

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options) =>
            writer.WriteStringValue(value.ToUpperInvariant());
    }

    private static JsonSerializerOptions CreateOptions(JsonIgnoreCondition ignoreCondition = JsonIgnoreCondition.Never) =>
        new(JsonSerializerDefaults.Web)
        {
            DefaultIgnoreCondition = ignoreCondition,
            Converters = { new JsonStringEnumConverter(), new GeometryConverter(), new NetTopologySuite.IO.Converters.GeoJsonConverterFactory() }
        };
}
