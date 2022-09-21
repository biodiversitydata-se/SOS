namespace SOS.UserStatistics.Api.Repositories.Interfaces;

public interface IUserStatisticsProcessedObservationRepository : IProcessedObservationCoreRepository
{
    Task<PagedResult<UserStatisticsItem>> PagedSpeciesCountSearchAsync(SpeciesCountUserStatisticsQuery query, int? skip, int? take);
    Task<List<UserStatisticsItem>> AreaSpeciesCountSearchAsync(SpeciesCountUserStatisticsQuery filter, List<int> userIds);
}
