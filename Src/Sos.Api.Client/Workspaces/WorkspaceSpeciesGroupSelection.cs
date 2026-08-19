namespace SOS.Api.Client.Workspaces;

/// <summary>
/// A taxon list used as a species-group selection.
/// </summary>
public sealed class WorkspaceSpeciesGroupSelection
{
    /// <summary>The taxon-list identifier.</summary>
    public int TaxonListId { get; set; }
    /// <summary>Controls whether taxa in the list are excluded.</summary>
    public bool Exclude { get; set; }
}
