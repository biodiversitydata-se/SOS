using SOS.Api.Contracts.Aggregations.Responses;
using SOS.Api.Contracts.Areas.Responses;
using SOS.Api.Contracts.Common.Geo;

namespace SOS.Api.Contracts.Observations.Responses;

/// <summary>A typed JSON result from adaptive search.</summary>
/// <typeparam name="T">The observation representation.</typeparam>
public sealed class AdaptiveSearchResultDto<T>
{
    /// <summary>The result representation selected by the API.</summary>
    public AdaptiveSearchResultTypeDto ResultType { get; set; }

    /// <summary>The bounding box of the result.</summary>
    public LatLonBoundingBoxDto? BoundingBox { get; set; }

    /// <summary>The total number of matching observations.</summary>
    public int TotalObservationsCount { get; set; }

    /// <summary>The number of returned observations or grid cells.</summary>
    public int ResultCount { get; set; }

    /// <summary>The grid result when <see cref="ResultType"/> is <see cref="AdaptiveSearchResultTypeDto.GeoGrid"/>.</summary>
    public GeoGridResultDto? GeoGridResult { get; set; }

    /// <summary>The observations when <see cref="ResultType"/> is <see cref="AdaptiveSearchResultTypeDto.Observations"/>.</summary>
    public IEnumerable<T>? ObservationsResult { get; set; }

    /// <summary>The filter areas included on request.</summary>
    public IEnumerable<AreaWithGeometryDto>? FilterAreas { get; set; }
}
