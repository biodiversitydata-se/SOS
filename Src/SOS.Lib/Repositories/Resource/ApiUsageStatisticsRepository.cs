using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Helpers;
using SOS.Lib.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SOS.Lib.Repositories.Resource;

/// <summary>
///     Report repository
/// </summary>
public class ApiUsageStatisticsRepository : RepositoryBase<ApiUsageStatistics, ObjectId>, Interfaces.IApiUsageStatisticsRepository
{

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="processClient"></param>
    /// <param name="logger"></param>
    public ApiUsageStatisticsRepository(
        IProcessClient processClient,
        ILogger<ApiUsageStatisticsRepository> logger) : base(processClient, logger)
    {

    }

    public Task<IAsyncCursor<ApiUsageStatistics>> GetByDateRangeAsync(DateTime? fromDate, DateTime? toDate)
    {
        var range = new ApiUsageStatisticsDateRange(fromDate, toDate);
        var filter = Builders<ApiUsageStatistics>.Filter.Empty;
        if (range.FromDate.HasValue)
        {
            filter &= Builders<ApiUsageStatistics>.Filter.Gte(row => row.Date,
                DateTime.SpecifyKind(range.FromDate.Value, DateTimeKind.Local));
        }
        // Include the entire last day, up to the next harvest midnight.
        // MaxValue includes every representable date and needs no upper bound.
        if (range.ToDate.HasValue && range.ToDate.Value < DateTime.MaxValue.Date)
        {
            filter &= Builders<ApiUsageStatistics>.Filter.Lt(row => row.Date,
                DateTime.SpecifyKind(range.ToDate.Value.AddDays(1), DateTimeKind.Local));
        }

        return MongoCollection.FindAsync(filter, new FindOptions<ApiUsageStatistics>
        {
            BatchSize = BatchSizeRead,
            AllowPartialResults = true,
            CursorType = CursorType.NonTailable
        });
    }

    public async Task<DateTime?> GetLatestHarvestDate()
    {
        var collectionExists = await CheckIfCollectionExistsAsync();
        if (!collectionExists)
        {
            return null;
        }

        var list = MongoCollection
            .Find(Builders<ApiUsageStatistics>.Filter.Empty)
            .SortByDescending(e => e.Date)
            .Limit(1)
            .ToList();

        if (list.Count == 1)
        {
            return list.First().Date.ToLocalTime();
        }

        return null;
    }

    public async Task VerifyCollection()
    {
        var collectionExists = await CheckIfCollectionExistsAsync();
        if (!collectionExists)
        {
            await Database.CreateCollectionAsync(CollectionName);
            await CreateIndexAsync();
        }
    }

    public async Task CreateIndexAsync()
    {
        var indexModels = new List<CreateIndexModel<ApiUsageStatistics>>
        {
            new CreateIndexModel<ApiUsageStatistics>(
                Builders<ApiUsageStatistics>.IndexKeys.Ascending(io => io.Date))
        };

        Logger.LogDebug("Creating ApiUsageStatistics indexes");
        await MongoCollection.Indexes.CreateManyAsync(indexModels);
    }
}
