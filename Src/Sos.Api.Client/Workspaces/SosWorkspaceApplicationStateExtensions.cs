using System.Text.Json;
using SOS.Api.Contracts.Workspaces;

namespace SOS.Api.Client.Workspaces;

/// <summary>Typed access to state owned by an application; this state never affects SOS search filters.</summary>
public static class SosWorkspaceApplicationStateExtensions
{
    /// <summary>Reads an independent typed value, or default when the application's entry is missing or null. Invalid state throws JsonException.</summary>
    public static T? GetApplicationState<T>(this SosWorkspaceDocument document, string applicationId,
        JsonSerializerOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentException.ThrowIfNullOrWhiteSpace(applicationId);
        return document.ApplicationState?.TryGetValue(applicationId, out var state) == true
            ? state.Deserialize<T>(options) : default;
    }

    /// <summary>Stores a snapshot of the value under this application's key, preserving other applications' entries.</summary>
    public static void SetApplicationState<T>(this SosWorkspaceDocument document, string applicationId, T value,
        JsonSerializerOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentException.ThrowIfNullOrWhiteSpace(applicationId);
        var state = JsonSerializer.SerializeToElement(value, options);
        document.ApplicationState ??= new();
        document.ApplicationState[applicationId] = state;
    }
}
