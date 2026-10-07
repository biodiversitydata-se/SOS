using SOS.Api.Contracts.Common.Geo;

namespace SOS.Api.Client.Requests;
/// <summary>Options for metric-grid aggregation.</summary>
public sealed class MetricGridAggregationInternalOptions : SosRoleRequestOptions
{
    /// <summary>The grid-cell side length, in metres.</summary>
    public int GridCellSizeInMeters { get; set; } = 10000;

    /// <summary>Controls whether the API validates the search filter.</summary>
    public bool ValidateSearchFilter { get; set; }

    /// <summary>Controls whether the sensitive-observation index is searched.</summary>
    public bool SensitiveObservations { get; set; }

    /// <summary>The metric coordinate reference system used to build the grid.</summary>
    public MetricCoordinateSys MetricCoordinateSys { get; set; } = MetricCoordinateSys.SWEREF99_TM;

    /// <summary>Creates the endpoint query string.</summary>
    public string ToQueryString()
    {
        return new SosQueryBuilder()
            .Add("gridCellSizeInMeters", GridCellSizeInMeters)
            .Add("validateSearchFilter", ValidateSearchFilter)
            .Add("sensitiveObservations", SensitiveObservations)
            .AddEnum("metricCoordinateSys", MetricCoordinateSys)
            .ToString();
    }
}


