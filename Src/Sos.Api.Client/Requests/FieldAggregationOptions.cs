namespace SOS.Api.Client.Requests;

/// <summary>Options for aggregating observations by a field.</summary>
public sealed class FieldAggregationOptions : SosRoleRequestOptions
{
    /// <summary>The property path used to form aggregation buckets.</summary>
    public required string AggregationField { get; set; }
    /// <summary>Controls whether the API validates the search filter.</summary>
    public bool? ValidateFilter { get; set; }
    /// <summary>Controls whether organism quantity is summed for each bucket.</summary>
    public bool? AggregateOrganismQuantity { get; set; }
    /// <summary>The aggregation precision threshold.</summary>
    public int? PrecisionThreshold { get; set; }
    /// <summary>The continuation key returned by the preceding page.</summary>
    public string? AfterKey { get; set; }
    /// <summary>The maximum number of buckets to return.</summary>
    public int? Take { get; set; } = 10;
}
