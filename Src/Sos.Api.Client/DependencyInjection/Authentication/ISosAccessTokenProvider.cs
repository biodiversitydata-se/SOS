namespace SOS.Api.Client.DependencyInjection.Authentication;

/// <summary>
/// Supplies an access token for outgoing requests.
/// Implementations must be singleton-safe and resolve the current token for each call.
/// </summary>
public interface ISosAccessTokenProvider
{
    /// <summary>
    /// Retrieves the current access token, or null if unavailable.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>The access token, or null if no token is available.</returns>
    ValueTask<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default);
}
