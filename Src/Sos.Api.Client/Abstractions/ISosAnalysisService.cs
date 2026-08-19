using SOS.Api.Client.Requests;
using SOS.Api.Contracts.Aggregations.Requests;

namespace SOS.Api.Client.Abstractions;

/// <summary>
/// High-level analysis workflows composed from one or more SOS Analysis API calls.
/// </summary>
public interface ISosAnalysisService
{
    /// <summary>Queues an AOO/EOO analysis by combining calculation options with order metadata.</summary>
    Task<string> OrderAooEooAsync(
        SearchFilterAggregationInternalDto filter,
        AooEooOptions calculationOptions,
        SosOrderMetadata order,
        CancellationToken cancellationToken = default);

    /// <summary>Queues an Article 17 analysis by combining calculation options with order metadata.</summary>
    Task<string> OrderArticle17Async(
        SearchFilterAggregationInternalDto filter,
        Article17Options calculationOptions,
        SosOrderMetadata order,
        CancellationToken cancellationToken = default);
}
