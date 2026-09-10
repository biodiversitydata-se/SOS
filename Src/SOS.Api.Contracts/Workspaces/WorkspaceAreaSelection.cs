using System.Text.Json.Serialization;

namespace SOS.Api.Contracts.Workspaces;

/// <summary>
/// Identifies an area selected in a workspace.
/// </summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class WorkspaceAreaSelection
{
    /// <summary>The SOS area-type identifier.</summary>
    [JsonPropertyName("areaType")]
    public string AreaType { get; set; } = string.Empty;
    /// <summary>The area's feature identifier.</summary>
    [JsonPropertyName("featureId")]
    public string FeatureId { get; set; } = string.Empty;
}
