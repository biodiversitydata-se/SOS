using System.Text.Json.Serialization;

namespace SOS.Api.Contracts.Workspaces;

/// <summary>
/// Spontaneous-occurrence values included in a workspace search.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<WorkspaceSpontaneous>))]
public enum WorkspaceSpontaneous
{
    /// <summary>No explicit spontaneous-occurrence filter.</summary>
    None = 0,
    /// <summary>Include spontaneous and non-spontaneous occurrences.</summary>
    SpontaneousAndUnspontaneous = 1,
    /// <summary>Include spontaneous occurrences only.</summary>
    SpontaneousOnly = 2,
    /// <summary>Include non-spontaneous occurrences only.</summary>
    UnspontaneousOnly = 3
}
