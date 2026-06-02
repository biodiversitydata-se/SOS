using NetTopologySuite.Geometries;

namespace SOS.Shared.Api.Dtos;

public class GeoJsonFeatureCollectionDto
{
    public string Type { get; set; } = "FeatureCollection";

    public IEnumerable<GeoJsonFeatureDto> Features { get; set; } = [];

    public IEnumerable<double>? Bbox { get; set; }
}

public class GeoJsonFeatureDto
{
    public string Type { get; set; } = "Feature";

    public Geometry Geometry { get; set; } = null!;

    public Dictionary<string, object> Properties { get; set; } = [];

    public IEnumerable<double>? Bbox { get; set; }

    public string? Id { get; set; }
}
