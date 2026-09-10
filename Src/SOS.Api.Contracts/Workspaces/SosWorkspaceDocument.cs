using System.Text.Json;
using System.Text.Json.Serialization;

namespace SOS.Api.Contracts.Workspaces;

/// <summary>
/// A portable, versioned workspace document. Storage identifiers, ownership and sharing are managed
/// by the application or user-data service that stores the document.
/// </summary>
public sealed class SosWorkspaceDocument
{
    /// <summary>The workspace document schema supported by this version of the contracts.</summary>
    public const int CurrentSchemaVersion = 1;

    /// <summary>
    /// The document schema version, independent of the NuGet package version. Consumers must check
    /// support before interpreting the search; the SOS client does this when converting a document.
    /// </summary>
    [JsonRequired]
    [JsonPropertyName("schemaVersion")]
    public int SchemaVersion { get; set; } = CurrentSchemaVersion;

    /// <summary>The workspace's display name.</summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>The saved search selections, including explicitly selected taxa.</summary>
    [JsonRequired]
    [JsonPropertyName("search")]
    public SosWorkspaceSearchDefinition Search { get; set; } = new();

    /// <summary>Saved preferences for analysis and export operations.</summary>
    [JsonPropertyName("settings")]
    public WorkspaceSettings? Settings { get; set; }

    /// <summary>
    /// Application-specific state keyed by a stable application identifier, for example
    /// "fynddata". Each application owns its entry and preserves entries belonging to other apps.
    /// This may contain inactive geometries, map presentation or file references; it is not a filter.
    /// </summary>
    [JsonPropertyName("applicationState")]
    public Dictionary<string, JsonElement>? ApplicationState { get; set; }

    /// <summary>Additional document metadata preserved when reading and writing JSON.</summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}
