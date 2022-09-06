namespace SOS.UserStatistics.Api.Repositories.Interfaces;

public interface IUserStatisticsObservationRepository : IUserObservationRepository
{
    Task<PagedResult<UserStatisticsItem>> PagedSpeciesCountSearchAsync(SpeciesCountUserStatisticsQuery query, int? skip, int? take);
    Task<List<UserStatisticsItem>> SpeciesCountSearchAsync(SpeciesCountUserStatisticsQuery filter, List<int> userIds = null);
    Task<List<UserStatisticsItem>> AreaSpeciesCountSearchAsync(SpeciesCountUserStatisticsQuery filter, List<int> userIds);
    Task<List<UserStatisticsItem>> AreaSpeciesCountSearchCompositeAsync(SpeciesCountUserStatisticsQuery filter, List<int> userIds);
}
