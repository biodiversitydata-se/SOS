using System.Threading.Tasks;
using MongoDB.Bson;
using SOS.Lib.Models.Processed.Validation;

namespace SOS.Lib.Repositories.Processed.Interfaces
{
    /// <summary>
    ///     Processed data class
    /// </summary>
    public interface IInvalidObservationRepository : IMongoDbProcessedRepositoryBase<InvalidObservation, ObjectId>
    {
        /// <summary>
        ///     Create index
        /// </summary>
        /// <returns></returns>
        Task CreateIndexAsync();
    }
}