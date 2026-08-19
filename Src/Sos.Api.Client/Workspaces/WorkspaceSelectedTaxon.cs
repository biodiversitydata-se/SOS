namespace SOS.Api.Client.Workspaces;

/// <summary>
/// A taxon explicitly included in or excluded from a workspace.
/// </summary>
public sealed class WorkspaceSelectedTaxon
{
    /// <summary>The taxon identifier.</summary>
    public int TaxonId { get; set; }
    /// <summary>Controls whether the taxon is excluded.</summary>
    public bool Exclude { get; set; }
    /// <summary>Controls whether child taxa are included in the selection.</summary>
    public bool IncludeChildren { get; set; }
}
