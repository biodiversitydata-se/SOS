using SOS.Lib.Models.Processed.DataStewardship.Dataset;
using SOS.Lib.Models.Search.Filters;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SOS.Lib.Repositories.Processed.Interfaces;

/// <summary>
/// </summary>
public interface IDatasetRepository : IProcessRepositoryBase<Dataset, string>
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

    Task<List<Dataset>> GetDatasetsByIds(IEnumerable<string> ids, IEnumerable<string> excludeFields = null, IEnumerable<SortOrderFilter> sortOrders = null);

    Task WaitForIndexCreation(long expectedRecordsCount, TimeSpan? timeout = null);
}