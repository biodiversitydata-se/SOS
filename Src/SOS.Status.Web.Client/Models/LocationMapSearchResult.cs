using SOS.Api.Contracts.Observations.Requests;

namespace SOS.Status.Web.Client.Models;

/// <summary>GeoJSON and diagnostics returned by a Status map search.</summary>
public sealed record LocationMapSearchResult
{
    public string GeoJson { get; init; } = string.Empty;
    public long? ObservationsTotalCount { get; init; }
    public long? EstimatedLocationCount { get; init; }
    public long? FeatureCount { get; init; }
    public long? LocationsBeforeClustering { get; init; }
    public long ResponseSizeBytes { get; init; }
    public string? CacheStatus { get; init; }
    public string? ResultType { get; init; }
    public string? Timings { get; init; }
    /// <summary>Summary values may be approximate, including statusCounts and potentially isAllObservationsSensitive. Null for older servers.</summary>
    public bool? LocationSummaryApproximated { get; init; }
    /// <summary>Applies only to presentation.status and presentation.cutoffYearStatus. Null for older servers.</summary>
    public bool? LocationPresentationStatusApproximated { get; init; }
    public AdaptiveSearchLocationsStrategy? AdaptiveSearchStrategy { get; init; }
}
