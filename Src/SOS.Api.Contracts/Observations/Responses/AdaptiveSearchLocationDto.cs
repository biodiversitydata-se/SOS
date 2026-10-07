using GeoJSON.Text.Geometry;

namespace SOS.Api.Contracts.Observations.Responses;

/// <summary>Observations grouped by location, with geometry and counts.</summary>
public class AdaptiveSearchLocationDto<TObservation, TLocation>
{
    /// <summary>The shared location information.</summary>
    public TLocation Location { get; set; }
    /// <summary>The geometry representing the location.</summary>
    public IGeometryObject Geometry { get; set; }
    /// <summary>The number of observations.</summary>
    public int ObservationCount { get; set; }
    /// <summary>The number of distinct taxa.</summary>
    public int TaxaCount { get; set; }
    /// <summary>The number of locations represented, normally one.</summary>
    public int LocationCount { get; set; } = 1;
    /// <summary>Observation counts by status, when available.</summary>
    public ObservationStatusCounts? StatusCounts { get; set; }
    /// <summary>The observations at this location.</summary>
    public IEnumerable<TObservation> Observations { get; set; } = null!;
}
