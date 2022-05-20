using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.CheckList;
using SOS.Lib.Models.Search;

namespace SOS.Lib.Repositories.Processed.Interfaces
{
    /// <summary>
    /// </summary>
    public interface IProcessedCheckListRepository : IProcessRepositoryBase<CheckList, string>
    {
        /// <summary>
        ///  Add many items
        /// </summary>
        /// <param name="checkLists"></param>
        /// <returns></returns>
        Task<int> AddManyAsync(IEnumerable<CheckList> checkLists);

        /// <summary>
        /// Clear the collection
        /// </summary>
        /// <returns></returns>
        Task<bool> ClearCollectionAsync();

        /// <summary>
        /// Turn of indexing
        /// </summary>
        /// <returns></returns>
        Task<bool> DisableIndexingAsync();

        /// <summary>
        /// Turn on indexing
        /// </summary>
        /// <returns></returns>
        Task EnableIndexingAsync();

        /// <summary>
        /// Get a check list by it's id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="internalCall"></param>
        /// <returns></returns>
        Task<CheckList> GetAsync(string id, bool internalCall);

        /// <summary>
        ///     Get chunk of objects from repository
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <param name="sortBy"></param>
        /// <param name="sortOrder"></param>
        /// <returns></returns>
        Task<PagedResult<CheckList>> GetChunkAsync(SearchFilter filter, int skip, int take, string sortBy,
            SearchSortOrder sortOrder);

        /// <summary>
        /// Count matching documents used in trend calculation
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        Task<long> GetTrendCountAsync(CheckListSearchFilter filter);

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
    }
}