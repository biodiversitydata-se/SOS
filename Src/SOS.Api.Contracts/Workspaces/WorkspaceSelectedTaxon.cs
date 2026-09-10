using System.Text.Json.Serialization;

namespace SOS.Api.Contracts.Workspaces;

/// <summary>
/// A taxon explicitly included in or excluded from a workspace.
/// </summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class WorkspaceSelectedTaxon
{
    /// <summary>The taxon identifier.</summary>
    [JsonPropertyName("taxonId")]
    public int TaxonId { get; set; }
    /// <summary>Controls whether the taxon is excluded.</summary>
    [JsonPropertyName("exclude")]
    public bool Exclude { get; set; }
    /// <summary>Controls whether child taxa are included in the selection.</summary>
    [JsonPropertyName("includeChildren")]
    public bool IncludeChildren { get; set; }
}
