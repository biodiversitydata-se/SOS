namespace SOS.Api.Contracts.Aggregations.Responses;

/// <summary>
/// An aggregation key and its observation count.
/// </summary>
public class AggregationItemDto
{
    /// <summary>The value identifying the aggregation bucket.</summary>
    public string? AggregationKey { get; set; }
    /// <summary>The number of observations in the bucket.</summary>
    public int DocCount { get; set; }
}
