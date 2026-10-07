using SOS.Api.Contracts.Common.Geo;
using SOS.Api.Contracts.Observations.Requests;

namespace SOS.Api.Client.Requests;
/// <summary>Options for metric-grid aggregation returned as GeoJSON.</summary>
public sealed class MetricGridAggregationInternalGeoJsonOptions : SosRoleRequestOptions
{
    /// <summary>The grid-cell side length, in metres.</summary>
    public int GridCellSizeInMeters { get; set; } = 10000;

    /// <summary>Controls whether the API validates the search filter.</summary>
    public bool ValidateSearchFilter { get; set; }

    /// <summary>Controls whether the sensitive-observation index is searched.</summary>
    public bool SensitiveObservations { get; set; }

    /// <summary>The metric coordinate reference system used to build the grid.</summary>
    public MetricCoordinateSys MetricCoordinateSys { get; set; } = MetricCoordinateSys.SWEREF99_TM;

    /// <summary>The GeoJSON response format.</summary>
    public OutputFormatDto OutputFormat { get; set; } = OutputFormatDto.GeoJson;

    /// <summary>Creates the endpoint query string.</summary>
    public string ToQueryString()
    {
        if (OutputFormat is not (OutputFormatDto.GeoJson or OutputFormatDto.GeoJsonFlat))
        {
            throw new InvalidOperationException("Metric grid GeoJSON output format must be GeoJson or GeoJsonFlat.");
        }

        return new SosQueryBuilder()
            .Add("gridCellSizeInMeters", GridCellSizeInMeters)
            .Add("validateSearchFilter", ValidateSearchFilter)
            .Add("sensitiveObservations", SensitiveObservations)
            .AddEnum("metricCoordinateSys", MetricCoordinateSys)
            .AddEnum("outputFormat", OutputFormat)
            .ToString();
    }
}


