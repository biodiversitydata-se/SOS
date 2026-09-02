using GeoJSON.Text.Geometry;

namespace SOS.Api.Client.Workspaces;

/// <summary>
/// Search-related workspace settings. Persistence, sharing and user settings belong to the consuming application.
/// </summary>
public sealed class SosWorkspaceSearchDefinition
{
    /// <summary>Areas included in the search.</summary>
    public ICollection<WorkspaceAreaSelection>? Areas { get; set; }
    /// <summary>Controls whether observations outside Sweden are excluded.</summary>
    public bool? ExcludeObservationsOutsideSweden { get; set; }
    /// <summary>Selected SOS data-provider identifiers.</summary>
    public ICollection<int>? DataProviderIds { get; set; }
    /// <summary>Selected data-stewardship dataset identifiers.</summary>
    public ICollection<string>? DataStewardshipDatasetIdentifiers { get; set; }
    /// <summary>Selected project identifiers.</summary>
    public ICollection<int>? ProjectIds { get; set; }
    /// <summary>Selected activity identifiers.</summary>
    public ICollection<int>? ActivityIds { get; set; }
    /// <summary>
    /// The maximum bird-nest activity identifier. This filter affects only bird observations from
    /// Artportalen; other organism groups are unaffected.
    /// </summary>
    public int? BirdNestActivityLimit { get; set; }
    /// <summary>Geometries used to constrain the search.</summary>
    public ICollection<IGeometryObject>? Geometries { get; set; }
    /// <summary>Date-selection settings.</summary>
    public WorkspaceDateFilter DateFilter { get; set; } = new();
    /// <summary>Observation-detail settings.</summary>
    public WorkspaceObservationDetails ObservationDetails { get; set; } = new();
    /// <summary>Taxon-selection settings.</summary>
    public WorkspaceTaxonSelection? TaxonSelection { get; set; }
    /// <summary>Occurrence identifiers to exclude.</summary>
    public ICollection<string>? ExcludedOccurrenceIds { get; set; }
}
