using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Lib.Models.Processed.DataStewardship.Event;

namespace SOS.Lib.Repositories.Processed.Interfaces
{
    /// <summary>
    /// </summary>
    public interface IObservationEventRepository : IProcessRepositoryBase<ObservationEvent, string>
    {
        /// <summary>
        ///  Add many items
        /// </summary>
        /// <param name="datasets"></param>
        /// <returns></returns>
        Task<int> AddManyAsync(IEnumerable<ObservationEvent> datasets);

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

        Task<List<ObservationEvent>> GetEventsByIds(IEnumerable<string> ids);

        Task<bool> DeleteAllDocumentsAsync();
    }
}