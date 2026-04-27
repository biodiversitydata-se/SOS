using SOS.Lib.Models.Processed.DataStewardship.Dataset;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Models.Search.Result;

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

    /// <summary>
    /// Retrieves a list of datasets that satisfy the specified filter criteria.
    /// </summary>
    /// <remarks>This method is asynchronous and should be awaited. An exception is thrown if the filter is
    /// invalid or if there are issues accessing the dataset source.</remarks>
    /// <param name="filter">A filter expression that defines the criteria for selecting datasets. This parameter cannot be null or empty.</param>
    /// <param name="skip">The number of datasets to skip before starting to collect the result set. This parameter is used for pagination.</param>
    /// <param name="take">The number of datasets to take from the result set. This parameter is used for pagination.</param>
    /// <param name="excludeFields">An optional collection of field names to exclude from the returned dataset objects. If specified, these fields
    /// will be omitted from each dataset in the result.</param>
    /// <param name="sortOrders">An optional collection of sorting criteria that determines the order of the returned datasets. Each criterion
    /// specifies a field and the sort direction.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of datasets that match the
    /// filter criteria.</returns>
    Task<PagedResult<Dataset>> GetDatasetsByFilterAsync(string filter, int skip, int take, IEnumerable<string> excludeFields = null, IEnumerable<SortOrderFilter> sortOrders = null);

    Task<List<Dataset>> GetDatasetsByIds(IEnumerable<string> ids, IEnumerable<string> excludeFields = null, IEnumerable<SortOrderFilter> sortOrders = null);

    Task WaitForIndexCreation(long expectedRecordsCount, TimeSpan? timeout = null);
}