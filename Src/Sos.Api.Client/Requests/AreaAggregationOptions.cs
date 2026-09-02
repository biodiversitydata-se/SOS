using SOS.Api.Contracts.Analysis.Requests;

namespace SOS.Api.Client.Requests;

/// <summary>Options for aggregating observations by area.</summary>
public sealed class AreaAggregationOptions : SosRoleRequestOptions
{
    /// <summary>The area type used for aggregation.</summary>
    public AreaTypeAggregateDto AreaType { get; set; } = AreaTypeAggregateDto.Atlas10x10;
    /// <summary>The coordinate system used for returned geometries.</summary>
    public AnalysisCoordinateSystem? CoordinateSystem { get; set; } = AnalysisCoordinateSystem.WGS84;
    /// <summary>The aggregation precision threshold.</summary>
    public int? PrecisionThreshold { get; set; } = 100;
    /// <summary>Controls whether organism quantity is summed for each bucket.</summary>
    public bool? AggregateOrganismQuantity { get; set; }
    /// <summary>Controls whether the API validates the search filter.</summary>
    public bool? ValidateFilter { get; set; }
}
