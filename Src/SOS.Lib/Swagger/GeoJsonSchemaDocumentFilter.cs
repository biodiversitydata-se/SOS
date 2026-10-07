using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json.Nodes;

namespace SOS.Lib.Swagger;

/// <summary>Creates the GeoJSON components referenced by request and response models.</summary>
public sealed class GeoJsonSchemaDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument document, DocumentFilterContext context)
    {
        var references = new GeoJsonReferences();
        new OpenApiWalker(references).Walk(document);
        if (!references.Geometry && !references.Point && !references.FeatureCollection) return;

        document.Components ??= new OpenApiComponents();
        document.Components.Schemas ??= new Dictionary<string, IOpenApiSchema>();
        if (references.FeatureCollection)
        {
            document.Components.Schemas["FeatureCollection"] = CreateFeatureCollectionSchema(document);
        }

        if (references.Geometry || references.FeatureCollection)
        {
            document.Components.Schemas["Geometry"] = new OpenApiSchema
            {
                // Properties are defined in the variants, so the union must allow them.
                AdditionalPropertiesAllowed = true,
                OneOf =
                [
                    CreateCoordinateGeometry("Point", 1),
                    CreateCoordinateGeometry("LineString", 2, 2),
                    CreateCoordinateGeometry("Polygon", 3, 4),
                    CreateCoordinateGeometry("MultiPoint", 2),
                    CreateCoordinateGeometry("MultiLineString", 3, 2),
                    CreateCoordinateGeometry("MultiPolygon", 4, 4),
                    new OpenApiSchema
                    {
                        Type = JsonSchemaType.Object,
                        Required = new HashSet<string> { "type", "geometries" },
                        Properties = new Dictionary<string, IOpenApiSchema>
                        {
                            ["type"] = CreateType("GeometryCollection"),
                            ["bbox"] = CreateBoundingBoxSchema(),
                            ["geometries"] = new OpenApiSchema
                            {
                                Type = JsonSchemaType.Array,
                                Items = new OpenApiSchemaReference("Geometry", document)
                            }
                        }
                    }
                ]
            };
        }

        if (references.Point)
        {
            document.Components.Schemas["Point"] = CreateCoordinateGeometry("Point", 1);
        }
    }

    // Describe the GeoJSON wire format without the library's CRS and type-enum schemas.
    private static OpenApiSchema CreateFeatureCollectionSchema(OpenApiDocument document) => new()
    {
        Type = JsonSchemaType.Object,
        Required = new HashSet<string> { "type", "features" },
        Properties = new Dictionary<string, IOpenApiSchema>
        {
            ["type"] = CreateType("FeatureCollection"),
            ["features"] = new OpenApiSchema { Type = JsonSchemaType.Array, Items = CreateFeatureSchema(document) },
            ["bbox"] = CreateBoundingBoxSchema()
        }
    };

    private static OpenApiSchema CreateFeatureSchema(OpenApiDocument document) => new()
    {
        Type = JsonSchemaType.Object,
        Required = new HashSet<string> { "type", "geometry", "properties" },
        Properties = new Dictionary<string, IOpenApiSchema>
        {
            ["type"] = CreateType("Feature"),
            ["geometry"] = new OpenApiSchema
            {
                AnyOf = [new OpenApiSchemaReference("Geometry", document), new OpenApiSchema { Enum = [null] }]
            },
            ["properties"] = new OpenApiSchema { Type = JsonSchemaType.Object | JsonSchemaType.Null, AdditionalPropertiesAllowed = true },
            ["id"] = new OpenApiSchema { Type = JsonSchemaType.String },
            ["bbox"] = CreateBoundingBoxSchema()
        }
    };

    private static OpenApiSchema CreateBoundingBoxSchema() => new()
    {
        Type = JsonSchemaType.Array,
        MinItems = 4,
        Items = new OpenApiSchema { Type = JsonSchemaType.Number, Format = "double" }
    };

    private static OpenApiSchema CreateCoordinateGeometry(string type, int arrayDepth, int? minPositions = null)
    {
        var coordinates = new OpenApiSchema { Type = JsonSchemaType.Number };
        for (var depth = 0; depth < arrayDepth; depth++)
        {
            coordinates = new OpenApiSchema
            {
                Type = JsonSchemaType.Array,
                Items = coordinates,
                MinItems = depth switch { 0 => 2, 1 => minPositions, _ => null }
            };
        }

        // RFC 7946 section 3.1 permits empty coordinate arrays to represent null geometries.
        if (type == "LineString")
        {
            coordinates.MinItems = null;
            coordinates.AnyOf = [new OpenApiSchema { MinItems = 2 }, new OpenApiSchema { MaxItems = 0 }];
        }

        return new OpenApiSchema
        {
            Type = JsonSchemaType.Object,
            Required = new HashSet<string> { "type", "coordinates" },
            Properties = new Dictionary<string, IOpenApiSchema>
            {
                ["type"] = CreateType(type),
                ["coordinates"] = coordinates,
                ["bbox"] = CreateBoundingBoxSchema()
            }
        };
    }

    private static OpenApiSchema CreateType(string type) => new()
    {
        Type = JsonSchemaType.String,
        Enum = [JsonValue.Create(type)]
    };

    private sealed class GeoJsonReferences : OpenApiVisitorBase
    {
        public bool Geometry { get; private set; }
        public bool Point { get; private set; }
        public bool FeatureCollection { get; private set; }

        public override void Visit(IOpenApiReferenceHolder referenceHolder)
        {
            if (referenceHolder is not OpenApiSchemaReference reference) return;
            Geometry |= reference.Reference?.Id == "Geometry";
            Point |= reference.Reference?.Id == "Point";
            FeatureCollection |= reference.Reference?.Id == "FeatureCollection";
        }
    }
}
