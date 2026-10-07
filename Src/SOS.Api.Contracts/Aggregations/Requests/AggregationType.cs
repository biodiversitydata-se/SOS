namespace SOS.Api.Contracts.Aggregations.Requests;

/// <summary>
/// Type of aggregation
/// </summary>
public enum AggregationType
{
    /// <summary>Count observations per week.</summary>
    SightingsPerWeek = 0,
    /// <summary>Count observations per year.</summary>
    SightingsPerYear = 1,
    /// <summary>Sum organism quantities per week.</summary>
    QuantityPerWeek = 2,
    /// <summary>Sum organism quantities per year.</summary>
    QuantityPerYear = 3,
    /// <summary>Group observations by species.</summary>
    SpeciesSightingsList = 4,
    /// <summary>Group observations by species and include the number of taxa.</summary>
    SpeciesSightingsListTaxonCount = 5,
    /// <summary>Special case where every month is divided in 4 weeks, total 48 weeks a year</summary>
    SightingsPerWeek48 = 6
}
