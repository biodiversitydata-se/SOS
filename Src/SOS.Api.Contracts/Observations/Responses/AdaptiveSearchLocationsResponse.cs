namespace SOS.Api.Contracts.Observations.Responses;
/// <summary>GeoJSON response body and diagnostic headers returned by adaptive location searches.</summary>
public sealed class AdaptiveSearchLocationsResponse
{
    /// <summary>The GeoJSON feature collection returned by the search.</summary>
    public required string GeoJson { get; init; }

    /// <summary>The response body size in bytes.</summary>
    public long ResponseSizeBytes { get; init; }

    /// <summary>The total number of matching observations.</summary>
    public long? ObservationsTotalCount { get; init; }

    /// <summary>The estimated number of matching locations.</summary>
    public long? EstimatedLocationCount { get; init; }

    /// <summary>The number of returned GeoJSON features.</summary>
    public long? FeatureCount { get; init; }

    /// <summary>The number of locations before clustering.</summary>
    public long? LocationsBeforeClustering { get; init; }

    /// <summary>The zoom level used for the result.</summary>
    public int? Zoom { get; init; }

    /// <summary>The cache status reported for the request.</summary>
    public string? CacheStatus { get; init; }

    /// <summary>The result representation, such as observations, locations or grid cells.</summary>
    public string? ResultType { get; init; }

    /// <summary>Diagnostic timing information reported for the search.</summary>
    public string? Timings { get; init; }

    /// <summary>The mode used to aggregate observations into representative locations.</summary>
    public string? LocationAggregationMode { get; init; }

    /// <summary>Whether representative location summaries are approximate.</summary>
    public bool? LocationSummaryApproximated { get; init; }
}


