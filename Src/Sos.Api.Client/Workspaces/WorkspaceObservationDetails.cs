namespace SOS.Api.Client.Workspaces;

/// <summary>
/// Observation properties that constrain a workspace search.
/// </summary>
public sealed class WorkspaceObservationDetails
{
    /// <summary>The presence filter.</summary>
    public WorkspacePresence Presence { get; set; } = WorkspacePresence.ObservedOnly;
    /// <summary>The spontaneous-occurrence filter.</summary>
    public WorkspaceSpontaneous Spontaneous { get; set; } = WorkspaceSpontaneous.SpontaneousAndUnspontaneous;
    /// <summary>The extended observation-type filter.</summary>
    public WorkspaceExtendedTypeFilter ExtendedTypeFilter { get; set; } = WorkspaceExtendedTypeFilter.DoNotShowMerged;
    /// <summary>Controls whether coordinate uncertainty is limited by <see cref="MaxAccuracy"/>.</summary>
    public bool FilterOnMaxAccuracy { get; set; }
    /// <summary>The maximum coordinate uncertainty, in metres.</summary>
    public int MaxAccuracy { get; set; } = 1000;
    /// <summary>Controls whether observations with uncertain identification are included.</summary>
    public bool IncludeUncertainIdentification { get; set; }
    /// <summary>Controls whether observations must have an invasive-species treatment.</summary>
    public bool HasInvasiveSpeciesTreatment { get; set; }
}
