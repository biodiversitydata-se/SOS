namespace SOS.Observations.Api.IntegrationTests.Models;

/// <summary>
/// Represents a deserialized GeoJSON FeatureCollection response where each feature's
/// properties are typed as <typeparamref name="TProperties"/>.
/// </summary>
public class GeoJsonResponse<TProperties>
{
    public string? Type { get; set; }
    public string? Crs { get; set; }
    public double[]? Bbox { get; set; }
    public IEnumerable<GeoJsonResponseFeature<TProperties>> Features { get; set; } = [];
}
