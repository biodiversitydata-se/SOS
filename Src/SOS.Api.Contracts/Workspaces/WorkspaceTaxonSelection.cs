using System.Text.Json.Serialization;

namespace SOS.Api.Contracts.Workspaces;

/// <summary>
/// Taxon, taxon-list and category selections used by a workspace search.
/// </summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class WorkspaceTaxonSelection
{
    /// <summary>Explicitly selected taxa.</summary>
    [JsonPropertyName("taxa")]
    public ICollection<WorkspaceSelectedTaxon>? Taxa { get; set; }
    /// <summary>Taxon-list identifiers whose taxa are selected.</summary>
    [JsonPropertyName("taxonListIds")]
    public ICollection<int>? TaxonListIds { get; set; }
    /// <summary>Included or excluded species-group lists.</summary>
    [JsonPropertyName("speciesGroups")]
    public ICollection<WorkspaceSpeciesGroupSelection>? SpeciesGroups { get; set; }
    /// <summary>Controls whether child taxa are included when resolving red-list selections.</summary>
    [JsonPropertyName("includeChildTaxaForRedlists")]
    public bool IncludeChildTaxaForRedlists { get; set; }
    /// <summary>
    /// Taxon-category identifiers used to filter an explicitly selected taxon and its expanded children
    /// when <see cref="WorkspaceSelectedTaxon.IncludeChildren"/> is enabled.
    /// </summary>
    [JsonPropertyName("taxonCategoryIds")]
    public ICollection<int>? TaxonCategoryIds { get; set; }
}
