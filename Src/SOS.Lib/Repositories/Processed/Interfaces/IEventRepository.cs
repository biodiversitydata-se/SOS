using SOS.Lib.Models.Processed.DataStewardship.Event;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Models.Search.Result;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SOS.Lib.Repositories.Processed.Interfaces;

/// <summary>
/// </summary>
public interface IEventRepository : IProcessRepositoryBase<Event, string>
{
    /// <summary>
    /// Clear the collection
    /// </summary>
    /// <returns></returns>
    Task<bool> ClearCollectionAsync();

    /// <summary>
    /// Name of index 
    /// </summary>
    string IndexName { get; }

    /// <summary>
    /// Unique index name
    /// </summary>
    string UniqueIndexName { get; }

    /// <summary>
    /// Verify that collection exists
    /// </summary>
    /// <returns></returns>
    Task<bool> VerifyCollectionAsync();

    Task<List<Event>> GetEventsByIds(IEnumerable<string> ids, IEnumerable<SortOrderFilter> sortOrders = null);
    Task<List<AggregationItemList<TKey, TValue>>> GetAllAggregationItemsListAsync<TKey, TValue>(EventSearchFilter filter, string aggregationFieldKey, string aggregationFieldList);
    Task<List<AggregationItem>> GetAllAggregationItemsAsync(EventSearchFilter filter, string aggregationField);
    Task<PagedResult<dynamic>> GetChunkAsync(EventSearchFilter filter, int skip, int take, bool getAllFields = false);
    Task WaitForIndexCreation(long expectedRecordsCount, TimeSpan? timeout = null);
}