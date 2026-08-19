namespace SOS.Api.Contracts.Observations.Responses;
public sealed class AdaptiveSearchLocationsResponse
{
    public required string GeoJson { get; init; }

    public long ResponseSizeBytes { get; init; }

    public long? ObservationsTotalCount { get; init; }

    public long? EstimatedLocationCount { get; init; }

    public long? FeatureCount { get; init; }

    public long? LocationsBeforeClustering { get; init; }

    public int? Zoom { get; init; }

    public string? CacheStatus { get; init; }

    public string? ResultType { get; init; }

    public string? Timings { get; init; }

    public string? LocationAggregationMode { get; init; }

    public bool? LocationSummaryApproximated { get; init; }
}


