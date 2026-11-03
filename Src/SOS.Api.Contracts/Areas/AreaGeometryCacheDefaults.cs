namespace SOS.Api.Contracts.Areas;

/// <summary>Shared HTTP cache policy for area geometry responses.</summary>
public static class AreaGeometryCacheDefaults
{
    /// <summary>Freshness lifetime in seconds: one hour.</summary>
    public const int MaxAgeSeconds = 3600;

    /// <summary>HTTP cache policy used by both the geometry endpoint and its web proxy.</summary>
    public static string CacheControl { get; } = FormattableString.Invariant($"public, max-age={MaxAgeSeconds}");
}
