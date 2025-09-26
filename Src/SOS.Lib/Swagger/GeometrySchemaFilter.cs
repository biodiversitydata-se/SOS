using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using NetTopologySuite.Geometries;
using System.Collections.Generic;

namespace SOS.Lib.Swagger;
public class GeometrySchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type == typeof(Geometry))
        {
            schema.OneOf = new List<OpenApiSchema>
            {
                CreatePointSchema(),
                CreateLineStringSchema(),
                CreatePolygonSchema(),
                CreateMultiPointSchema(),
                CreateMultiLineStringSchema(),
                CreateMultiPolygonSchema(),
                CreateGeometryCollectionSchema()
            };

            schema.Properties.Clear();
            schema.Type = null;
        }
    }

    private OpenApiSchema CreatePointSchema() =>
        new OpenApiSchema
        {
            Type = "object",
            Required = new HashSet<string> { "type", "coordinates" },
            Properties = new Dictionary<string, OpenApiSchema>
            {
                ["type"] = new OpenApiSchema
                {
                    Type = "string",
                    Enum = new List<IOpenApiAny> { new OpenApiString("Point") }
                },
                ["coordinates"] = new OpenApiSchema
                {
                    Type = "array",
                    Items = new OpenApiSchema { Type = "number" },
                    MinItems = 2
                }
            }
        };

    private OpenApiSchema CreateLineStringSchema() =>
        new OpenApiSchema
        {
            Type = "object",
            Required = new HashSet<string> { "type", "coordinates" },
            Properties = new Dictionary<string, OpenApiSchema>
            {
                ["type"] = new OpenApiSchema
                {
                    Type = "string",
                    Enum = new List<IOpenApiAny> { new OpenApiString("LineString") }
                },
                ["coordinates"] = new OpenApiSchema
                {
                    Type = "array",
                    Items = new OpenApiSchema
                    {
                        Type = "array",
                        Items = new OpenApiSchema { Type = "number" },
                        MinItems = 2
                    }
                }
            }
        };

    private OpenApiSchema CreatePolygonSchema() =>
        new OpenApiSchema
        {
            Type = "object",
            Required = new HashSet<string> { "type", "coordinates" },
            Properties = new Dictionary<string, OpenApiSchema>
            {
                ["type"] = new OpenApiSchema
                {
                    Type = "string",
                    Enum = new List<IOpenApiAny> { new OpenApiString("Polygon") }
                },
                ["coordinates"] = new OpenApiSchema
                {
                    Type = "array",
                    Items = new OpenApiSchema
                    {
                        Type = "array",
                        Items = new OpenApiSchema
                        {
                            Type = "array",
                            Items = new OpenApiSchema { Type = "number" }
                        }
                    }
                }
            }
        };

    private OpenApiSchema CreateMultiPointSchema() =>
        new OpenApiSchema
        {
            Type = "object",
            Required = new HashSet<string> { "type", "coordinates" },
            Properties = new Dictionary<string, OpenApiSchema>
            {
                ["type"] = new OpenApiSchema
                {
                    Type = "string",
                    Enum = new List<IOpenApiAny> { new OpenApiString("MultiPoint") }
                },
                ["coordinates"] = new OpenApiSchema
                {
                    Type = "array",
                    Items = new OpenApiSchema
                    {
                        Type = "array",
                        Items = new OpenApiSchema { Type = "number" }
                    }
                }
            }
        };

    private OpenApiSchema CreateMultiLineStringSchema() =>
        new OpenApiSchema
        {
            Type = "object",
            Required = new HashSet<string> { "type", "coordinates" },
            Properties = new Dictionary<string, OpenApiSchema>
            {
                ["type"] = new OpenApiSchema
                {
                    Type = "string",
                    Enum = new List<IOpenApiAny> { new OpenApiString("MultiLineString") }
                },
                ["coordinates"] = new OpenApiSchema
                {
                    Type = "array",
                    Items = new OpenApiSchema
                    {
                        Type = "array",
                        Items = new OpenApiSchema
                        {
                            Type = "array",
                            Items = new OpenApiSchema { Type = "number" }
                        }
                    }
                }
            }
        };

    private OpenApiSchema CreateMultiPolygonSchema() =>
        new OpenApiSchema
        {
            Type = "object",
            Required = new HashSet<string> { "type", "coordinates" },
            Properties = new Dictionary<string, OpenApiSchema>
            {
                ["type"] = new OpenApiSchema
                {
                    Type = "string",
                    Enum = new List<IOpenApiAny> { new OpenApiString("MultiPolygon") }
                },
                ["coordinates"] = new OpenApiSchema
                {
                    Type = "array",
                    Items = new OpenApiSchema
                    {
                        Type = "array",
                        Items = new OpenApiSchema
                        {
                            Type = "array",
                            Items = new OpenApiSchema
                            {
                                Type = "array",
                                Items = new OpenApiSchema { Type = "number" }
                            }
                        }
                    }
                }
            }
        };

    private OpenApiSchema CreateGeometryCollectionSchema() =>
        new OpenApiSchema
        {
            Type = "object",
            Required = new HashSet<string> { "type", "geometries" },
            Properties = new Dictionary<string, OpenApiSchema>
            {
                ["type"] = new OpenApiSchema
                {
                    Type = "string",
                    Enum = new List<IOpenApiAny> { new OpenApiString("GeometryCollection") }
                },
                ["geometries"] = new OpenApiSchema
                {
                    Type = "array",
                    Items = new OpenApiSchema { Reference = new OpenApiReference { Id = "Geometry", Type = ReferenceType.Schema } }
                }
            }
        };
}