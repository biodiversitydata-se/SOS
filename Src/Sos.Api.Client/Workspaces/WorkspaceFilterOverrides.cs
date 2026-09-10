using SOS.Api.Contracts.Common.Geo;
using SOS.Api.Contracts.Observations.Requests;

namespace SOS.Api.Client.Workspaces;

/// <summary>
/// Per-request values that are not persisted in a workspace.
/// </summary>
public sealed class WorkspaceFilterOverrides
{
    /// <summary>A bounding box that additionally constrains the workspace.</summary>
    public LatLonBoundingBoxDto? BoundingBox { get; set; }
    /// <summary>A centre point for a radius search.</summary>
    public LatLonCoordinateDto? Point { get; set; }
    /// <summary>The radius around <see cref="Point"/>, in metres.</summary>
    public int? RadiusInMeters { get; set; }
    /// <summary>The radius used when <see cref="Point"/> is set without an explicit radius.</summary>
    public int DefaultRadiusInMeters { get; set; } = 1000;
    /// <summary>Output-field settings to place on the generated search filter.</summary>
    public OutputFilterExtendedDto? Output { get; set; }

    /// <summary>
    /// Intersects the computed workspace date interval, for example when producing time-series batches.
    /// Empty intersections and reversed bounds throw a workspace validation exception.
    /// Recurring periods require both bounds within one calendar year and a single contiguous intersection.
    /// </summary>
    public DateTime? DateFrom { get; set; }
    /// <summary>The optional inclusive upper date boundary.</summary>
    public DateTime? DateTo { get; set; }

    /// <summary>
    /// Replaces the resolved workspace taxon selection for a single request.
    /// This is useful when expanding one workspace into one request per taxon.
    /// </summary>
    public IReadOnlyCollection<int>? TaxonIds { get; set; }
}
