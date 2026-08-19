namespace SOS.Api.Client.Workspaces;

/// <summary>
/// Taxon, taxon-list and category selections used by a workspace search.
/// </summary>
public sealed class WorkspaceTaxonSelection
{
    /// <summary>Explicitly selected taxa.</summary>
    public ICollection<WorkspaceSelectedTaxon>? Taxa { get; set; }
    /// <summary>Taxon-list identifiers whose taxa are selected.</summary>
    public ICollection<int>? TaxonListIds { get; set; }
    /// <summary>Included or excluded species-group lists.</summary>
    public ICollection<WorkspaceSpeciesGroupSelection>? SpeciesGroups { get; set; }
    /// <summary>Controls whether child taxa are included when resolving red-list selections.</summary>
    public bool IncludeChildTaxaForRedlists { get; set; }
    /// <summary>
    /// Taxon-category identifiers used to filter an explicitly selected taxon and its expanded children
    /// when <see cref="WorkspaceSelectedTaxon.IncludeChildren"/> is enabled.
    /// </summary>
    public ICollection<int>? TaxonCategoryIds { get; set; }
}
