using System.Text.Json.Serialization;
using GeoJSON.Text.Geometry;

namespace SOS.Api.Contracts.Workspaces;

/// <summary>
/// Reusable search selections stored in a workspace and converted to SOS filters by the client.
/// </summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class SosWorkspaceSearchDefinition
{
    /// <summary>Areas included in the search.</summary>
    [JsonPropertyName("areas")]
    public ICollection<WorkspaceAreaSelection>? Areas { get; set; }
    /// <summary>Controls whether observations outside Sweden are excluded.</summary>
    [JsonPropertyName("excludeObservationsOutsideSweden")]
    public bool? ExcludeObservationsOutsideSweden { get; set; }
    /// <summary>Selected SOS data-provider identifiers. Null or empty selects no providers.</summary>
    [JsonPropertyName("dataProviderIds")]
    public ICollection<int>? DataProviderIds { get; set; }
    /// <summary>Selected data-stewardship dataset identifiers.</summary>
    [JsonPropertyName("dataStewardshipDatasetIdentifiers")]
    public ICollection<string>? DataStewardshipDatasetIdentifiers { get; set; }
    /// <summary>Selected project identifiers.</summary>
    [JsonPropertyName("projectIds")]
    public ICollection<int>? ProjectIds { get; set; }
    /// <summary>Selected activity identifiers.</summary>
    [JsonPropertyName("activityIds")]
    public ICollection<int>? ActivityIds { get; set; }
    /// <summary>
    /// The maximum bird-nest activity identifier. This filter affects only bird observations from
    /// Artportalen; other organism groups are unaffected.
    /// </summary>
    [JsonPropertyName("birdNestActivityLimit")]
    public int? BirdNestActivityLimit { get; set; }
    /// <summary>Active search geometries in WGS84 GeoJSON. Store inactive features in application state.</summary>
    [JsonPropertyName("geometries")]
    public ICollection<IGeometryObject>? Geometries { get; set; }
    /// <summary>Date-selection settings.</summary>
    [JsonPropertyName("dateFilter")]
    public WorkspaceDateFilter DateFilter { get; set; } = new();
    /// <summary>Observation-detail settings.</summary>
    [JsonPropertyName("observationDetails")]
    public WorkspaceObservationDetails ObservationDetails { get; set; } = new();
    /// <summary>Taxon-selection settings. Null means no taxon restriction; an empty selection matches no taxa.</summary>
    [JsonPropertyName("taxonSelection")]
    public WorkspaceTaxonSelection? TaxonSelection { get; set; }
    /// <summary>Occurrence identifiers to exclude.</summary>
    [JsonPropertyName("excludedOccurrenceIds")]
    public ICollection<string>? ExcludedOccurrenceIds { get; set; }
}
