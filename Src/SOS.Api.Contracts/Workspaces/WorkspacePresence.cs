using System.Text.Json.Serialization;

namespace SOS.Api.Contracts.Workspaces;

/// <summary>
/// Presence values included in a workspace search.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<WorkspacePresence>))]
public enum WorkspacePresence
{
    /// <summary>Use the default observed-only selection.</summary>
    None = 0,
    /// <summary>Include observed occurrences only.</summary>
    ObservedOnly = 1,
    /// <summary>Include observed and not-recovered occurrences.</summary>
    ObservedAndNotRecovered = 2,
    /// <summary>Include observed and not-found occurrences.</summary>
    ObservedAndNotFound = 3,
    /// <summary>Include only not-recovered and not-found occurrences.</summary>
    NotRecoveredAndNotFoundOnly = 4,
    /// <summary>Include only not-recovered occurrences.</summary>
    OnlyNotRecovered = 5,
    /// <summary>Include only not-found occurrences.</summary>
    OnlyNotFound = 6
}
