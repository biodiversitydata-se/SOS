using System.Text.Json.Serialization;

namespace SOS.Api.Contracts.Workspaces;

/// <summary>
/// A taxon list used as a species-group selection.
/// </summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class WorkspaceSpeciesGroupSelection
{
    /// <summary>The taxon-list identifier.</summary>
    [JsonPropertyName("taxonListId")]
    public int TaxonListId { get; set; }
    /// <summary>Controls whether taxa in the list are excluded.</summary>
    [JsonPropertyName("exclude")]
    public bool Exclude { get; set; }
}
