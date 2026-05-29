using NetTopologySuite.Geometries;
using System.Text.Json;

namespace SOS.Observations.Api.IntegrationTests.Models;

/// <summary>
/// Represents a single GeoJSON Feature with typed properties.
/// The <see cref="Geometry"/> is kept as a <see cref="JsonElement"/> to avoid
/// conflicts with the NetTopologySuite GeoJSON converter registered in the serializer options.
/// </summary>
public class GeoJsonResponseFeature<TProperties>
{
    public string? Type { get; set; }
    public string? Id { get; set; }
    public Geometry Geometry { get; set; }
    public TProperties Properties { get; set; } = default!;
}
