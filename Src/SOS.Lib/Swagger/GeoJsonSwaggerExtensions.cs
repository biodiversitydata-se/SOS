using GeoJSON.Text.Geometry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using NetTopologySuite.Geometries;
using Swashbuckle.AspNetCore.SwaggerGen;
using GeoJSON.Text.Feature;

namespace SOS.Lib.Swagger;

public static class GeoJsonSwaggerExtensions
{
    /// <summary>Describes both geometry libraries using shared GeoJSON schemas.</summary>
    public static void UseGeoJsonSchemas(this SwaggerGenOptions options)
    {
        options.MapType<Geometry>(() => new OpenApiSchemaReference("Geometry"));
        options.MapType<IGeometryObject>(() => new OpenApiSchemaReference("Geometry"));
        options.MapType<NetTopologySuite.Geometries.Point>(() => new OpenApiSchemaReference("Point"));
        options.MapType<GeoJSON.Text.Geometry.Point>(() => new OpenApiSchemaReference("Point"));
        options.MapType<FeatureCollection>(() => new OpenApiSchemaReference("FeatureCollection"));
        options.DocumentFilter<GeoJsonSchemaDocumentFilter>();
    }
}
