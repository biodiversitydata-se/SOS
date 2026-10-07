using SOS.Api.Contracts.Aggregations.Requests;
using SOS.Api.Contracts.Observations.Requests;

namespace SOS.Shared.Api.Utilities.Objects.Interfaces;

public interface ISearchFilterUtility
{
    Task<SearchFilterBaseDto> InitializeSearchFilterAsync(
        SearchFilterBaseDto? filter);

    Task<SearchFilterDto> InitializeSearchFilterAsync(
        SearchFilterDto? filter);

    Task<SearchFilterInternalBaseDto> InitializeSearchFilterAsync(
        SearchFilterInternalBaseDto? filter);

    Task<SearchFilterInternalDto> InitializeSearchFilterAsync(
        SearchFilterInternalDto? filter);

    Task<SearchFilterAggregationInternalDto> InitializeSearchFilterAsync(
        SearchFilterAggregationInternalDto? filter);

    /// <summary>
    /// Initialize search filter base
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="filter"></param>
    /// <returns></returns>
    Task<T> InitializeSearchFilterAsync<T>(T? filter) where T : SearchFilterBaseDto;

}
