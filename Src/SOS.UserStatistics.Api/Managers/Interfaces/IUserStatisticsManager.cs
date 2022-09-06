namespace SOS.UserStatistics.Api.Managers.Interfaces;

public interface IUserStatisticsManager
{
    Task<PagedResult<UserStatisticsItem>> PagedSpeciesCountSearchAsync(SpeciesCountUserStatisticsQuery query,
        int? skip,
        int? take,
        bool useCache = true);
    Task<PagedResult<UserStatisticsItem>> SpeciesCountSearchAsync(SpeciesCountUserStatisticsQuery query,
        int? skip,
        int? take,
        bool useCache = true);

    Task<PagedResult<UserStatisticsItem>> ProcessedObservationPagedSpeciesCountSearchAsync(SpeciesCountUserStatisticsQuery query,
        int? skip,
        int? take,
        bool useCache = true);

    void ClearCache();
}
