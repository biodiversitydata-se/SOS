namespace SOS.Api.Client.Workspaces;

/// <summary>
/// Determines which Artportalen sighting-type search groups are included.
/// </summary>
public enum WorkspaceExtendedTypeFilter
{
    /// <summary>Exclude merged observations.</summary>
    DoNotShowMerged = 0,
    /// <summary>Exclude merged observations but include their replacement children.</summary>
    DoNotShowMergedIncludeReplacementChildren = 1
}
