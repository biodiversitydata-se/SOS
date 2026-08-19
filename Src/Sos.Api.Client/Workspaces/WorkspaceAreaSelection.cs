namespace SOS.Api.Client.Workspaces;

/// <summary>
/// Identifies an area selected in a workspace.
/// </summary>
public sealed class WorkspaceAreaSelection
{
    /// <summary>The SOS area-type identifier.</summary>
    public string AreaType { get; set; } = string.Empty;
    /// <summary>The area's feature identifier.</summary>
    public string FeatureId { get; set; } = string.Empty;
}
