namespace SOS.Api.Contracts.Aggregations.Requests;

/// <summary>The ordering of aggregation results.</summary>
public enum AggregationSortOrder
{
    /// <summary>Order by observation count, highest first.</summary>
    CountDescending = 0,
    /// <summary>Order by observation count, lowest first.</summary>
    CountAscending = 1,
    /// <summary>Order by aggregation key, ascending.</summary>
    KeyAscending = 2,
    /// <summary>Order by aggregation key, descending.</summary>
    KeyDescending = 3,
    /// <summary>Order by organism quantity, lowest first.</summary>
    OrganismQuantityAscending = 4,
    /// <summary>Order by organism quantity, highest first.</summary>
    OrganismQuantityDescending = 5
}
