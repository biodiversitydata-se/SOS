namespace SOS.Api.Client.Abstractions;

/// <summary>
/// Supplies the selected authorization role for outgoing requests.
/// Implementations must be singleton-safe and resolve the current role for each call.
/// </summary>
public interface ISosRoleIdProvider
{
    /// <summary>The selected role identifier, or <see langword="null"/> when no role is selected.</summary>
    int? GetCurrentRoleId();
}
