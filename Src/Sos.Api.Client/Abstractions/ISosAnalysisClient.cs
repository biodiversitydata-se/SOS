using GeoJSON.Text.Feature;
using SOS.Api.Client.Requests;
using SOS.Api.Contracts.Aggregations.Requests;
using SOS.Api.Contracts.Analysis.Responses;

namespace SOS.Api.Client.Abstractions;

/// <summary>Client for SOS Analysis API endpoints.</summary>
public interface ISosAnalysisClient
{
    /// <summary>Aggregates observations by a requested field.</summary>
    Task<PagedAggregationResultDto<UserAggregationResponseDto>> AggregateAsync(
        SearchFilterAggregationInternalDto filter,
        FieldAggregationOptions options,
        CancellationToken cancellationToken = default);

    /// <summary>Aggregates observations into the requested area type.</summary>
    Task<FeatureCollection> AggregateAreasAsync(
        SearchFilterAggregationInternalDto filter,
        AreaAggregationOptions options,
        CancellationToken cancellationToken = default);

    /// <summary>Calculates area of occupancy and extent of occurrence immediately.</summary>
    Task<FeatureCollection> CalculateAooEooAsync(
        SearchFilterAggregationInternalDto filter,
        AooEooOptions options,
        CancellationToken cancellationToken = default);

    /// <summary>Queues an area-of-occupancy and extent-of-occurrence calculation.</summary>
    Task<string> OrderAooEooAsync(
        SearchFilterAggregationInternalDto filter,
        AooEooOrderOptions options,
        CancellationToken cancellationToken = default);

    /// <summary>Calculates Article 17 analysis immediately.</summary>
    Task<FeatureCollection> CalculateArticle17Async(
        SearchFilterAggregationInternalDto filter,
        Article17Options options,
        CancellationToken cancellationToken = default);

    /// <summary>Queues an Article 17 analysis.</summary>
    Task<string> OrderArticle17Async(
        SearchFilterAggregationInternalDto filter,
        Article17OrderOptions options,
        CancellationToken cancellationToken = default);
}
