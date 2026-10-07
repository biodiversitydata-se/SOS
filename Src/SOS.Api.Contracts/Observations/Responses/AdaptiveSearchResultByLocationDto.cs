using SOS.Api.Contracts.Common.Geo;
using SOS.Api.Contracts.Areas.Responses;
using SOS.Api.Contracts.Aggregations.Responses;

namespace SOS.Api.Contracts.Observations.Responses;

/// <summary>An adaptive search result containing grid cells or observations grouped by location.</summary>
public class AdaptiveSearchResultByLocationDto<TObservation, TLocation>
{
    /// <summary>The representation selected for the result.</summary>
    public AdaptiveSearchResultTypeDto ResultType { get; set; }

    /// <summary>
    /// The bounding box of the result, or null when no observations matched the search (no extent could
    /// be calculated).
    /// </summary>
    public LatLonBoundingBoxDto? BoundingBox { get; set; }
    /// <summary>The total number of observations matching the search.</summary>
    public int TotalObservationsCount { get; set; }
    /// <summary>The number of observations returned in the location result.</summary>
    public int ObservationsCount { get; set; }
    /// <summary>The number of grid cells or locations returned.</summary>
    public int ResultCount { get; set; }
    /// <summary>The grid result, when grid aggregation was selected.</summary>
    public GeoGridResultDto? GeoGridResult { get; set; }
    /// <summary>Observations grouped by location, when location results were selected.</summary>
    public IEnumerable<AdaptiveSearchLocationDto<TObservation, TLocation>>? LocationResult { get; set; }
    /// <summary>The geographic areas used to filter the search, when requested.</summary>
    public IEnumerable<AreaWithGeometryDto>? FilterAreas { get; set; }
}
