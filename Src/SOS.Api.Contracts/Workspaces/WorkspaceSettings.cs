using System.Text.Json;
using System.Text.Json.Serialization;

namespace SOS.Api.Contracts.Workspaces;

/// <summary>Saved operation preferences. These settings do not constrain the observation search.</summary>
public sealed class WorkspaceSettings
{
    /// <summary>
    /// The preferred EPSG identifier for analysis calculations. Null leaves the choice to the
    /// application. Apply it to the analysis options after checking which systems the operation supports.
    /// </summary>
    [JsonPropertyName("analysisSrid")]
    public int? AnalysisSrid { get; set; }

    /// <summary>
    /// The preferred EPSG identifier for exports. Null leaves the choice to the application.
    /// Apply it to the export options after checking which systems the operation supports.
    /// </summary>
    [JsonPropertyName("exportSrid")]
    public int? ExportSrid { get; set; }

    /// <summary>Additional preferences preserved when reading and writing JSON.</summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}
