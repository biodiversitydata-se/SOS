namespace SOS.Api.Contracts.Analysis.Responses;

/// <summary>
/// A page of aggregation records and the key used to retrieve the next page.
/// </summary>
/// <typeparam name="T">The aggregation record type.</typeparam>
public sealed class PagedAggregationResultDto<T>
{
    /// <summary>The continuation key for the next page, or <see langword="null"/> for the last page.</summary>
    public object? AfterKey { get; set; }

    /// <summary>The aggregation records in the current page.</summary>
    public IEnumerable<T>? Records { get; set; }
}
