
namespace SOS.Api.Client.Requests;
/// <summary>Options for taxon aggregation.</summary>
public sealed class TaxonSumAggregationInternalOptions
{
    /// <summary>The number of buckets to skip.</summary>
    public int? Skip { get; set; }

    /// <summary>The field by which to sort the buckets.</summary>
    public string? SortBy { get; set; }

    /// <summary>The order in which to sort the buckets.</summary>
    public string? SortOrder { get; set; }

    /// <summary>The maximum number of buckets to return.</summary>
    public int? Take { get; set; }

    /// <summary>Creates the endpoint query string.</summary>
    public string ToQueryString()
    {
        return new SosQueryBuilder()
            .Add("skip", Skip)
            .Add("take", Take)
            .Add("sortBy", SortBy)
            .Add("sortOrder", SortOrder)
            .ToString();
    }
}


