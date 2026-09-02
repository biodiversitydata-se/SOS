namespace SOS.Api.Contracts.Observations.Responses;

/// <summary>The response body and metadata returned by adaptive search.</summary>
public sealed class AdaptiveSearchResponse
{
    /// <summary>The JSON or GeoJSON response body.</summary>
    public required string Content { get; init; }

    /// <summary>The response content type.</summary>
    public string? ContentType { get; init; }

    /// <summary>The response size in bytes.</summary>
    public long ResponseSizeBytes { get; init; }

    /// <summary>The total number of matching observations reported for GeoJSON responses.</summary>
    public long? ObservationsTotalCount { get; init; }

    /// <summary>The number of returned observations or grid cells reported for GeoJSON responses.</summary>
    public long? ResultCount { get; init; }

    /// <summary>The result representation reported for GeoJSON responses.</summary>
    public string? ResultType { get; init; }

    /// <summary>The grid zoom level reported for GeoJSON grid responses.</summary>
    public int? Zoom { get; init; }

    /// <summary>The observation count provided by the legacy compatibility header.</summary>
    public long? ObservationsCount { get; init; }
}
