namespace SOS.Api.Client;

/// <summary>
/// Configures HTTP resilience behavior for SOS API clients.
/// </summary>
public sealed class SosResilienceOptions
{
    /// <summary>
    /// Maximum total time for a request including all retry attempts and delays.
    /// </summary>
    public TimeSpan TotalRequestTimeout { get; set; } = TimeSpan.FromSeconds(90);

    /// <summary>
    /// Timeout for a single attempt before it is considered failed.
    /// </summary>
    public TimeSpan AttemptTimeout { get; set; } = TimeSpan.FromSeconds(60);

    /// <summary>
    /// Maximum number of retry attempts after the initial attempt fails.
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 2;

    /// <summary>
    /// Initial delay between retry attempts. Grows exponentially with jitter.
    /// </summary>
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(2);

    /// <summary>
    /// Maximum delay between retry attempts regardless of exponential backoff.
    /// </summary>
    public TimeSpan RetryMaxDelay { get; set; } = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Fraction of failed requests within the sampling window that triggers the circuit breaker.
    /// Value between 0.0 and 1.0.
    /// </summary>
    public double CircuitBreakerFailureRatio { get; set; } = 0.5;

    /// <summary>
    /// Minimum number of requests within the sampling window before the circuit breaker can open.
    /// </summary>
    public int CircuitBreakerMinimumThroughput { get; set; } = 10;

    /// <summary>
    /// Duration the circuit breaker stays open before allowing a test request through.
    /// </summary>
    public TimeSpan CircuitBreakerBreakDuration { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Time window over which failures are measured to determine whether to open the circuit.
    /// </summary>
    public TimeSpan CircuitBreakerSamplingDuration { get; set; } = TimeSpan.FromSeconds(120);
}
