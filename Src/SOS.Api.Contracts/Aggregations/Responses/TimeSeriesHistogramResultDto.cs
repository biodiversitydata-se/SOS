using SOS.Api.Contracts.Aggregations.Requests;
using SOS.Api.Contracts.Observations.Responses;

namespace SOS.Api.Contracts.Aggregations.Responses;

/// <summary>
/// Time series histogram result.
/// </summary>
public class TimeSeriesHistogramResultDto
{
    /// <summary>
    /// Time series type.
    /// </summary>
    public TimeSeriesTypeDto Type { get; set; }

    /// <summary>
    /// Time period.
    /// </summary>
    public int Period { get; set; }

    /// <summary>
    /// Number of observations.
    /// </summary>
    public int Observations { get; set; }

    /// <summary>
    /// Organism quantity.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Number of taxa.
    /// </summary>
    public int Taxa { get; set; }
}

