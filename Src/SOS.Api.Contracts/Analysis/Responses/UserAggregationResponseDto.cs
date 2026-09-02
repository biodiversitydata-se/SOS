namespace SOS.Api.Contracts.Analysis.Responses;

/// <summary>
/// Aggregated values for one field value.
/// </summary>
public sealed class UserAggregationResponseDto
{
    /// <summary>The value of the field used to form this bucket.</summary>
    public object? AggregationField { get; set; }

    /// <summary>The number of observations in the bucket.</summary>
    public int Count { get; set; }

    /// <summary>The number of unique taxa in the bucket.</summary>
    public int UniqueTaxon { get; set; }

    /// <summary>The summed organism quantity in the bucket.</summary>
    public int OrganismQuantity { get; set; }
}
