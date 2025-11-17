using SOS.Shared.Api.Dtos.Filter;

namespace SOS.Shared.Api.Utilities.Objects.Interfaces;

public interface ISearchFilterUtility
{
    /// <summary>
    /// Initialize search filter base
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="filter"></param>
    /// <returns></returns>
    Task<T> InitializeSearchFilterAsync<T>(T? filter) where T : SearchFilterBaseDto;

    /// <summary>
    /// Initialize search filter internal
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    Task<SearchFilterInternalDto> InitializeSearchFilterAsync(SearchFilterInternalDto filter);
}
