using System.Text.Json.Serialization;

namespace SOS.Api.Contracts.Workspaces;

/// <summary>
/// Observation properties that constrain a workspace search.
/// </summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class WorkspaceObservationDetails
{
    /// <summary>The presence filter.</summary>
    [JsonPropertyName("presence")]
    public WorkspacePresence Presence { get; set; } = WorkspacePresence.ObservedOnly;
    /// <summary>The spontaneous-occurrence filter.</summary>
    [JsonPropertyName("spontaneous")]
    public WorkspaceSpontaneous Spontaneous { get; set; } = WorkspaceSpontaneous.SpontaneousAndUnspontaneous;
    /// <summary>The extended observation-type filter.</summary>
    [JsonPropertyName("extendedTypeFilter")]
    public WorkspaceExtendedTypeFilter ExtendedTypeFilter { get; set; } = WorkspaceExtendedTypeFilter.DoNotShowMerged;
    /// <summary>Controls whether coordinate uncertainty is limited by <see cref="MaxAccuracy"/>.</summary>
    [JsonPropertyName("filterOnMaxAccuracy")]
    public bool FilterOnMaxAccuracy { get; set; }
    /// <summary>The maximum coordinate uncertainty, in metres.</summary>
    [JsonPropertyName("maxAccuracy")]
    public int MaxAccuracy { get; set; } = 1000;
    /// <summary>Controls whether observations with uncertain identification are included.</summary>
    [JsonPropertyName("includeUncertainIdentification")]
    public bool IncludeUncertainIdentification { get; set; }
    /// <summary>Controls whether observations must have an invasive-species treatment.</summary>
    [JsonPropertyName("hasInvasiveSpeciesTreatment")]
    public bool HasInvasiveSpeciesTreatment { get; set; }
}
