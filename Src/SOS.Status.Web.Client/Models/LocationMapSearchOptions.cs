using SOS.Api.Contracts.Observations.Requests;
using System.Globalization;

namespace SOS.Status.Web.Client.Models;

/// <summary>Immutable settings for a Status map search, compared by value.</summary>
public sealed record LocationMapSearchOptions
{
    public int ObservationsLimit { get; init; } = 10000;
    public int MaxGridCells { get; init; } = 10000;
    public int LocationAggregationLimit { get; init; } = 1000;
    public AdaptiveSearchLocationsStrategy AdaptiveSearchStrategy { get; init; } = AdaptiveSearchLocationsStrategy.Auto;
    public bool Cluster { get; init; } = true;
    public double? ClusterRadiusMeters { get; init; } = null;
    public CreatePolygonsMode CreatePolygons { get; init; } = CreatePolygonsMode.No;
    public double ClusterRadiusPx { get; init; } = AdaptiveSearchLocationsDefaults.ClusterRadiusPx;
    public int MinLocationsPerCluster { get; init; } = 2;
    public int MinLocationCountBeforeClustering { get; init; } = AdaptiveSearchLocationsDefaults.MinLocationCountBeforeClustering;
    public bool ValidateSearchFilter { get; init; } = false;
    public string TranslationCultureCode { get; init; } = "sv-SE";
    public bool SensitiveObservations { get; init; } = false;
    public double? MetersPerPixel { get; init; } = null;
    public string? Bbox { get; init; } = null;
    public bool CreateSampleObservations { get; init; } = false;
    public bool UseCache { get; init; } = true;
    public int? CutoffYear { get; init; } = null;
    public bool IncludeFilterAreasInResult { get; init; } = false;
    public double DisablePixelClusteringAtMetersPerPixel { get; init; } = AdaptiveSearchLocationsDefaults.DisablePixelClusteringAtMetersPerPixel;
    public double ProximityClusterRadiusPx { get; init; } = AdaptiveSearchLocationsDefaults.ProximityClusterRadiusPx;

    public string ToQueryString()
    {
        var values = new (string Name, object? Value)[]
        {
            ("observationsLimit", ObservationsLimit),
            ("maxGridCells", MaxGridCells),
            ("locationAggregationLimit", LocationAggregationLimit),
            ("adaptiveSearchStrategy", AdaptiveSearchStrategy),
            ("cluster", Cluster),
            ("clusterRadiusMeters", ClusterRadiusMeters),
            ("createPolygons", CreatePolygons),
            ("clusterRadiusPx", ClusterRadiusPx),
            ("minLocationsPerCluster", MinLocationsPerCluster),
            ("minLocationCountBeforeClustering", MinLocationCountBeforeClustering),
            ("validateSearchFilter", ValidateSearchFilter),
            ("translationCultureCode", TranslationCultureCode),
            ("sensitiveObservations", SensitiveObservations),
            ("metersPerPixel", MetersPerPixel),
            ("bbox", string.IsNullOrWhiteSpace(Bbox) ? null : Bbox),
            ("createSampleObservations", CreateSampleObservations),
            ("useCache", UseCache),
            ("cutoffYear", CutoffYear),
            ("includeFilterAreasInResult", IncludeFilterAreasInResult),
            ("disablePixelClusteringAtMetersPerPixel", DisablePixelClusteringAtMetersPerPixel),
            ("proximityClusterRadiusPx", ProximityClusterRadiusPx),
        };
        return string.Join("&", values.Where(v => v.Value is not null && v.Value is not string { Length: 0 })
            .Select(v => $"{v.Name}={Uri.EscapeDataString(v.Value is bool flag ? (flag ? "true" : "false") : Convert.ToString(v.Value, CultureInfo.InvariantCulture)!)}"));
    }
}
