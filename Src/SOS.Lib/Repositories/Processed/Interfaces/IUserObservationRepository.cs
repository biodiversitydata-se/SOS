using SOS.Lib.Models.Processed.Observation;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SOS.Lib.Repositories.Processed.Interfaces
{
    /// <summary>
    /// </summary>
    public interface IUserObservationRepository : IProcessRepositoryBase<UserObservation, long>
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
    }
}