using GeoJSON.Text.Geometry;
using Microsoft.OpenApi;
using Moq;
using SOS.Lib.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json;
using Xunit;

namespace SOS.Lib.UnitTests.Swagger;

public class GeoJsonSchemaDocumentFilterTests
{
    [Fact]
    public void FeatureCollectionReference_AddsCollectionAndItsGeometryDependency()
    {
        var document = CreateDocument("FeatureCollection");

        Assert.Equal(new[] { "FeatureCollection", "Geometry", "Result" }, document.Components.Schemas.Keys.Order());
        var feature = document.Components.Schemas["FeatureCollection"].Properties["features"].Items;
        Assert.Equal("Geometry", Assert.IsType<OpenApiSchemaReference>(feature.Properties["geometry"].AnyOf[0]).Reference.Id);
        Assert.Null(Assert.Single(feature.Properties["geometry"].AnyOf[1].Enum));
        Assert.Equal(JsonSchemaType.Object | JsonSchemaType.Null, feature.Properties["properties"].Type);
        Assert.Equal(4, document.Components.Schemas["FeatureCollection"].Properties["bbox"].MinItems);
        Assert.Equal(4, feature.Properties["bbox"].MinItems);
        Assert.Equal(7, document.Components.Schemas["Geometry"].OneOf.Count);
    }

    [Theory]
    [InlineData(18d, 59d, 18d, 59d)]
    [InlineData(18d, 59d, 120d, 18d, 59d, 120d)]
    public void PointReference_DescribesSerializedBoundingBoxAndAllowsForeignMembers(params double[] boundingBox)
    {
        var document = CreateDocument("Point");
        var schema = document.Components.Schemas["Point"];
        var point = new Point(new Position(59, 18, boundingBox.Length == 6 ? 120 : null)) { BoundingBoxes = boundingBox };
        using var payload = JsonDocument.Parse(JsonSerializer.Serialize(point));

        Assert.True(schema.AdditionalPropertiesAllowed);
        Assert.DoesNotContain("bbox", schema.Required);
        foreach (var property in payload.RootElement.EnumerateObject())
            Assert.Contains(property.Name, schema.Properties.Keys);
        Assert.Equal(JsonSchemaType.Array, schema.Properties["bbox"].Type);
        Assert.Equal(4, schema.Properties["bbox"].MinItems);
        Assert.Null(schema.Properties["bbox"].MaxItems);
        Assert.Equal(JsonSchemaType.Number, schema.Properties["bbox"].Items.Type);
        Assert.Equal(boundingBox.Length / 2, payload.RootElement.GetProperty("coordinates").GetArrayLength());
        Assert.Null(schema.Properties["coordinates"].MaxItems);

        using var writer = new StringWriter();
        document.SerializeAsV3(new OpenApiJsonWriter(writer));
        using var json = JsonDocument.Parse(writer.ToString());
        var serializedSchema = json.RootElement.GetProperty("components").GetProperty("schemas").GetProperty("Point");
        Assert.False(serializedSchema.TryGetProperty("additionalProperties", out var additionalProperties)
            && additionalProperties.ValueKind == JsonValueKind.False);
    }

    // Minimums are listed from the outer coordinate array down to each position (RFC 7946 section 3.1).
    [Theory]
    [InlineData("Point", 2)]
    [InlineData("LineString", 0, 2)]
    [InlineData("Polygon", 0, 4, 2)]
    [InlineData("MultiPoint", 0, 2)]
    [InlineData("MultiLineString", 0, 2, 2)]
    [InlineData("MultiPolygon", 0, 0, 4, 2)]
    public void CoordinateGeometries_HaveGeoJsonDimensionsAndMinimums(string geometryType, params int[] minimums)
    {
        var geometry = GetGeometryVariant(CreateDocument("Geometry"), geometryType);
        var coordinates = geometry.Properties["coordinates"];

        foreach (var minimum in minimums)
        {
            Assert.Equal(JsonSchemaType.Array, coordinates.Type);
            Assert.Equal(minimum == 0 ? (int?)null : minimum, coordinates.MinItems);
            Assert.Null(coordinates.MaxItems);
            coordinates = coordinates.Items;
        }

        Assert.Equal(JsonSchemaType.Number, coordinates.Type);
    }

    [Theory]
    [InlineData("Point")]
    [InlineData("LineString")]
    [InlineData("Polygon")]
    [InlineData("MultiPoint")]
    [InlineData("MultiLineString")]
    [InlineData("MultiPolygon")]
    [InlineData("GeometryCollection")]
    public void GeometryVariants_AllowOptionalBoundingBoxAndForeignMembers(string geometryType)
    {
        var geometry = GetGeometryVariant(CreateDocument("Geometry"), geometryType);

        Assert.True(geometry.AdditionalPropertiesAllowed);
        Assert.DoesNotContain("bbox", geometry.Required);
        Assert.Equal(JsonSchemaType.Array, geometry.Properties["bbox"].Type);
        Assert.Equal(4, geometry.Properties["bbox"].MinItems);
        Assert.Equal(JsonSchemaType.Number, geometry.Properties["bbox"].Items.Type);
    }

    [Fact]
    public void LineString_AllowsEmptyCoordinatesWithoutAllowingASinglePosition()
    {
        var geometry = GetGeometryVariant(CreateDocument("Geometry"), "LineString");
        var coordinates = geometry.Properties["coordinates"];
        var alternatives = coordinates.AnyOf;

        Assert.Equal(JsonSchemaType.Array, coordinates.Type);
        Assert.Null(coordinates.MinItems);
        Assert.Equal(JsonSchemaType.Array, coordinates.Items.Type);
        Assert.Equal(2, alternatives.Count);
        Assert.Equal(2, alternatives[0].MinItems);
        Assert.Equal(0, alternatives[1].MaxItems);

        var emptyLine = JsonSerializer.Deserialize<IGeometryObject>("""{"type":"LineString","coordinates":[]}""");
        using var payload = JsonDocument.Parse(JsonSerializer.Serialize(emptyLine));
        Assert.Equal(0, payload.RootElement.GetProperty("coordinates").GetArrayLength());
    }

    [Theory]
    [InlineData("Polygon")]
    [InlineData("MultiPoint")]
    [InlineData("MultiLineString")]
    [InlineData("MultiPolygon")]
    [InlineData("GeometryCollection")]
    public void Collections_ContinueToAllowEmptyArrays(string geometryType)
    {
        var geometry = GetGeometryVariant(CreateDocument("Geometry"), geometryType);
        var property = geometryType == "GeometryCollection" ? "geometries" : "coordinates";

        Assert.Equal(JsonSchemaType.Array, geometry.Properties[property].Type);
        Assert.Null(geometry.Properties[property].MinItems);
    }

    [Fact]
    public void DocumentWithoutGeoJsonReferences_DoesNotAddUnusedComponents()
    {
        var document = new OpenApiDocument
        {
            Components = new OpenApiComponents { Schemas = new Dictionary<string, IOpenApiSchema>() }
        };
        var context = new DocumentFilterContext([], Mock.Of<ISchemaGenerator>(), new SchemaRepository());

        new GeoJsonSchemaDocumentFilter().Apply(document, context);

        Assert.Empty(document.Components.Schemas);
    }

    private static OpenApiDocument CreateDocument(string referenceId)
    {
        var document = new OpenApiDocument
        {
            Components = new OpenApiComponents
            {
                Schemas = new Dictionary<string, IOpenApiSchema>
                {
                    ["Result"] = new OpenApiSchema
                    {
                        Properties = new Dictionary<string, IOpenApiSchema>
                        {
                            ["geometry"] = new OpenApiSchemaReference(referenceId)
                        }
                    }
                }
            }
        };
        var context = new DocumentFilterContext([], Mock.Of<ISchemaGenerator>(), new SchemaRepository());
        new GeoJsonSchemaDocumentFilter().Apply(document, context);
        return document;
    }

    private static IOpenApiSchema GetGeometryVariant(OpenApiDocument document, string geometryType) =>
        document.Components.Schemas["Geometry"].OneOf.Single(
            geometry => geometry.Properties["type"].Enum.Single()!.GetValue<string>() == geometryType);
}
