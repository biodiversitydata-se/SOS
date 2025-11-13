using Swashbuckle.AspNetCore.SwaggerGen;
using NetTopologySuite.Geometries;
using System.Collections.Generic;
using Microsoft.OpenApi;
using System.Text.Json.Nodes;

namespace SOS.Lib.Swagger;
public class GeometrySchemaFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema is OpenApiSchema openApiSchema)
        {
            if (context.Type == typeof(Geometry))
            {
                openApiSchema.OneOf = new List<IOpenApiSchema>
                {
                    CreatePointSchema(),
                    CreateLineStringSchema(),
                    CreatePolygonSchema(),
                    CreateMultiPointSchema(),
                    CreateMultiLineStringSchema(),
                    CreateMultiPolygonSchema(),
                    CreateGeometryCollectionSchema()
                };

                openApiSchema.Properties?.Clear();
                openApiSchema.Type = null; //JsonSchemaType.Null;
            }
        }
    }

    private OpenApiSchema CreatePointSchema() =>
        new OpenApiSchema
        {
            Type = JsonSchemaType.Object,
            Required = new HashSet<string> { "type", "coordinates" },
            Properties = new Dictionary<string, IOpenApiSchema>
            {
                ["type"] = new OpenApiSchema
                {
                    Type = JsonSchemaType.String,
                    Enum = new List<JsonNode> { JsonNode.Parse("\"Point\"") },
                },
                ["coordinates"] = new OpenApiSchema
                {
                    Type = JsonSchemaType.Array,
                    Items = new OpenApiSchema { Type = JsonSchemaType.Number },
                    MinItems = 2
                }
            }
        };

    private OpenApiSchema CreateLineStringSchema() =>
        new OpenApiSchema
        {
            Type = JsonSchemaType.Object,
            Required = new HashSet<string> { "type", "coordinates" },
            Properties = new Dictionary<string, IOpenApiSchema>
            {
                ["type"] = new OpenApiSchema
                {
                    Type = JsonSchemaType.String,                    
                    Enum = new List<JsonNode> { JsonNode.Parse("\"LineString\"") },
                },
                ["coordinates"] = new OpenApiSchema
                {
                    Type = JsonSchemaType.Array,
                    Items = new OpenApiSchema
                    {
                        Type = JsonSchemaType.Array,
                        Items = new OpenApiSchema { Type = JsonSchemaType.Number },
                        MinItems = 2
                    }
                }
            }
        };

    private OpenApiSchema CreatePolygonSchema() =>
        new OpenApiSchema
        {
            Type = JsonSchemaType.Object,
            Required = new HashSet<string> { "type", "coordinates" },
            Properties = new Dictionary<string, IOpenApiSchema>
            {
                ["type"] = new OpenApiSchema
                {
                    Type = JsonSchemaType.String,
                    Enum = new List<JsonNode> { JsonNode.Parse("\"Polygon\"") },
                },
                ["coordinates"] = new OpenApiSchema
                {
                    Type = JsonSchemaType.Array,
                    Items = new OpenApiSchema
                    {
                        Type = JsonSchemaType.Array,
                        Items = new OpenApiSchema
                        {
                            Type = JsonSchemaType.Array,
                            Items = new OpenApiSchema { Type = JsonSchemaType.Number }
                        }
                    }
                }
            }
        };

    private OpenApiSchema CreateMultiPointSchema() =>
        new OpenApiSchema
        {
            Type = JsonSchemaType.Object,
            Required = new HashSet<string> { "type", "coordinates" },
            Properties = new Dictionary<string, IOpenApiSchema>
            {
                ["type"] = new OpenApiSchema
                {
                    Type = JsonSchemaType.String,
                    Enum = new List<JsonNode> { JsonNode.Parse("\"MultiPoint\"") },
                },
                ["coordinates"] = new OpenApiSchema
                {
                    Type = JsonSchemaType.Array,
                    Items = new OpenApiSchema
                    {
                        Type = JsonSchemaType.Array,
                        Items = new OpenApiSchema { Type = JsonSchemaType.Number }
                    }
                }
            }
        };

    private OpenApiSchema CreateMultiLineStringSchema() =>
        new OpenApiSchema
        {
            Type = JsonSchemaType.Object,
            Required = new HashSet<string> { "type", "coordinates" },
            Properties = new Dictionary<string, IOpenApiSchema>
            {
                ["type"] = new OpenApiSchema
                {
                    Type = JsonSchemaType.String,
                    Enum = new List<JsonNode> { JsonNode.Parse("\"MultiLineString\"") },
                },
                ["coordinates"] = new OpenApiSchema
                {
                    Type = JsonSchemaType.Array,
                    Items = new OpenApiSchema
                    {
                        Type = JsonSchemaType.Array,
                        Items = new OpenApiSchema
                        {
                            Type = JsonSchemaType.Array,
                            Items = new OpenApiSchema { Type = JsonSchemaType.Number }
                        }
                    }
                }
            }
        };

    private OpenApiSchema CreateMultiPolygonSchema() =>
        new OpenApiSchema
        {
            Type = JsonSchemaType.Object,
            Required = new HashSet<string> { "type", "coordinates" },
            Properties = new Dictionary<string, IOpenApiSchema>
            {
                ["type"] = new OpenApiSchema
                {
                    Type = JsonSchemaType.String,
                    Enum = new List<JsonNode> { JsonNode.Parse("\"MultiPolygon\"") },
                },
                ["coordinates"] = new OpenApiSchema
                {
                    Type = JsonSchemaType.Array,
                    Items = new OpenApiSchema
                    {
                        Type = JsonSchemaType.Array,
                        Items = new OpenApiSchema
                        {
                            Type = JsonSchemaType.Array,
                            Items = new OpenApiSchema
                            {
                                Type = JsonSchemaType.Array,
                                Items = new OpenApiSchema { Type = JsonSchemaType.Number }
                            }
                        }
                    }
                }
            }
        };

    private OpenApiSchema CreateGeometryCollectionSchema() =>
        new OpenApiSchema
        {
            Type = JsonSchemaType.Object,
            Required = new HashSet<string> { "type", "geometries" },
            Properties = new Dictionary<string, IOpenApiSchema>
            {
                ["type"] = new OpenApiSchema
                {
                    Type = JsonSchemaType.String,
                    Enum = new List<JsonNode> { JsonNode.Parse("\"GeometryCollection\"") },                    
                },
                ["geometries"] = new OpenApiSchema
                {
                    Type = JsonSchemaType.Array,                    
                    Items = new OpenApiSchema { DynamicRef = "#/components/schemas/Geometry" } //  Reference = new OpenApiReference { Id = "Geometry", Type = ReferenceType.Schema } } 
                }
            }
        };
}