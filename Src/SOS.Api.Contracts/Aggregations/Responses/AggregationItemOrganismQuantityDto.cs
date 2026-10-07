namespace SOS.Api.Contracts.Aggregations.Responses;

/// <summary>
/// An aggregation key with observation and organism counts.
/// </summary>
public class AggregationItemOrganismQuantityDto
{
    /// <summary>The value identifying the aggregation bucket.</summary>
    public string? AggregationKey { get; set; }
    /// <summary>The number of observations in the bucket.</summary>
    public int DocCount { get; set; }
    /// <summary>The sum of reported organism quantities in the bucket.</summary>
    public int OrganismQuantity { get; set; }
}
