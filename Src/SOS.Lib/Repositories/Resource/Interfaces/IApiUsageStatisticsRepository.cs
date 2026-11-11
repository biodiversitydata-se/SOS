using MongoDB.Bson;
using SOS.Lib.Models.Shared;
using MongoDB.Driver;
using SOS.Lib.Repositories.Interfaces;
using System;
using System.Threading.Tasks;

namespace SOS.Lib.Repositories.Resource.Interfaces;

public interface IApiUsageStatisticsRepository : IRepositoryBase<ApiUsageStatistics, ObjectId>
{
    /// <summary>Stream statistics within inclusive harvest dates. Null bounds are open.</summary>
    Task<IAsyncCursor<ApiUsageStatistics>> GetByDateRangeAsync(DateTime? fromDate, DateTime? toDate);
    Task<DateTime?> GetLatestHarvestDate();
    Task CreateIndexAsync();
    Task VerifyCollection();
}
