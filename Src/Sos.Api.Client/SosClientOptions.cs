namespace SOS.Api.Client;

/// <summary>
/// SOS client options.
/// </summary>
public sealed class SosClientOptions
{
    /// <summary>
    /// Log level used for internal search request parameters and bodies.
    /// Set to <see cref="Microsoft.Extensions.Logging.LogLevel.None"/> to disable request logging.
    /// </summary>
    public Microsoft.Extensions.Logging.LogLevel RequestLoggingLevel { get; set; } =
        Microsoft.Extensions.Logging.LogLevel.None;

    /// <summary>
    /// Maximum number of characters included from an internal search request body in a log entry.
    /// </summary>
    public int MaxLoggedRequestBodyLength { get; set; } = 1024;

    /// <summary>
    /// Name of the authentication token retrieved from HttpContext.
    /// Used by AddSosWebClients to relay the current user's access token.
    /// </summary>
    public string AccessTokenName { get; set; } = "access_token";

    /// <summary>
    /// HttpClient timeout used when standard resilience is disabled.
    /// When resilience is enabled, timeouts are managed by Polly instead.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(90);

    /// <summary>
    /// Timeout for long-running observation aggregation, analysis and export requests.
    /// Analysis and export order POST requests are not retried.
    /// </summary>
    public TimeSpan LongRunningTimeout { get; set; } = TimeSpan.FromSeconds(240);

    /// <summary>The maximum number of area-geometry exports performed concurrently.</summary>
    public int AreaGeometryMaxConcurrency { get; set; } = 8;

    /// <summary>The sliding expiration used for cached area geometries.</summary>
    public TimeSpan AreaGeometryCacheSlidingExpiration { get; set; } = TimeSpan.FromMinutes(15);

    /// <summary>
    /// Identifier for the consuming application, sent as the X-Authorization-Application-Identifier header.
    /// </summary>
    public string ApplicationIdentifier { get; set; } = string.Empty;

    /// <summary>
    /// Name of the requesting system, sent as the X-Requesting-System header.
    /// </summary>
    public string SystemName { get; set; } = string.Empty;

    /// <summary>
    /// Base URL for the SOS Observations API.
    /// </summary>
    public required Uri ObservationsApiBaseAddress { get; set; }

    /// <summary>
    /// Base URL for the SOS Analysis API.
    /// </summary>
    public Uri? AnalysisApiBaseAddress { get; set; }

    /// <summary>
    /// Configures SOS standard HTTP resilience (retry, timeout, circuit breaker).
    /// Only applied when useResilience is true (default).
    /// </summary>
    public SosResilienceOptions Resilience { get; set; } = new();
}
