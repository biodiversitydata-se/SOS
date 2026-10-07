using SOS.Api.Contracts.Aggregations.Requests;
using SOS.Api.Contracts.Common.Geo;

namespace SOS.Api.Client.Requests;

/// <summary>Options for aggregating observations by area.</summary>
public sealed class AreaAggregationOptions : SosRoleRequestOptions
{
    /// <summary>The area type used for aggregation.</summary>
    public AreaTypeAggregate AreaType { get; set; } = AreaTypeAggregate.Atlas10x10;
    /// <summary>The coordinate system used for returned geometries.</summary>
    public CoordinateSys? CoordinateSystem { get; set; } = CoordinateSys.WGS84;
    /// <summary>The aggregation precision threshold.</summary>
    public int? PrecisionThreshold { get; set; } = 100;
    /// <summary>Controls whether organism quantity is summed for each bucket.</summary>
    public bool? AggregateOrganismQuantity { get; set; }
    /// <summary>Controls whether the API validates the search filter.</summary>
    public bool? ValidateFilter { get; set; }
}
