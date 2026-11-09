namespace SOS.Api.Contracts.Areas.Responses;

/// <summary>A geometry GET response and its HTTP cache validator, as returned by the API client.</summary>
public sealed class AreaGeometryResponse
{
    /// <summary>The GeoJSON Feature body, or null when the server returned 304 Not Modified.</summary>
    public string? GeoJson { get; init; }

    /// <summary>The ETag to retain alongside the body and supply as If-None-Match when revalidating.</summary>
    public string? ETag { get; init; }

    /// <summary>True for 304 Not Modified; reuse the previously cached geometry.</summary>
    public bool NotModified { get; init; }
}
