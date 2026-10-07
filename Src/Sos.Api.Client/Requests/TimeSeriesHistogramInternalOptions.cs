using SOS.Api.Contracts.Aggregations.Requests;

namespace SOS.Api.Client.Requests;
/// <summary>Options for time-series aggregation.</summary>
public sealed class TimeSeriesHistogramInternalOptions : SosRoleRequestOptions
{
    /// <summary>The time interval used for histogram buckets.</summary>
    public TimeSeriesTypeDto TimeSeriesType { get; set; } = TimeSeriesTypeDto.Year;

    /// <summary>Controls whether the API validates the search filter.</summary>
    public bool ValidateSearchFilter { get; set; }

    /// <summary>Controls whether the sensitive-observation index is searched.</summary>
    public bool SensitiveObservations { get; set; }

    /// <summary>Creates the endpoint query string.</summary>
    public string ToQueryString()
    {
        return new SosQueryBuilder()
            .AddEnum("timeSeriesType", TimeSeriesType)
            .Add("validateSearchFilter", ValidateSearchFilter)
            .Add("sensitiveObservations", SensitiveObservations)
            .ToString();
    }
}


