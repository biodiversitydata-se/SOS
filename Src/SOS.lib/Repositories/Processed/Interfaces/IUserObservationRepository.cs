using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.Checklist;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Models.Search.Result;
using SOS.Lib.Models.Statistics;

namespace SOS.Lib.Repositories.Processed.Interfaces
{
    /// <summary>
    /// </summary>
    public interface IUserObservationRepository : IProcessRepositoryBase<UserObservation, long>
    {
        /// <summary>
        ///  Add many items
        /// </summary>
        /// <param name="userObservations"></param>
        /// <returns></returns>
        Task<int> AddManyAsync(IEnumerable<UserObservation> userObservations);

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

        Task<PagedResult<UserStatisticsItem>> PagedSpeciesCountSearchAsync(SpeciesCountUserStatisticsQuery query, int? skip, int? take);
        Task<List<UserStatisticsItem>> SpeciesCountSearchAsync(SpeciesCountUserStatisticsQuery filter);
        Task<List<UserStatisticsItem>> AreaSpeciesCountSearchAsync(SpeciesCountUserStatisticsQuery filter, List<int> userIds);
    }
}