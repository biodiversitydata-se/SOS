using System.Threading.Tasks;
using MongoDB.Bson;
using SOS.Lib.Models.Processed.Validation;

namespace SOS.Lib.Repositories.Processed.Interfaces
{
    /// <summary>
    ///     Invalid event repository
    /// </summary>
    public interface IInvalidEventRepository : IMongoDbProcessedRepositoryBase<InvalidEvent, ObjectId>
    {
        /// <summary>
        ///     Create index
        /// </summary>
        /// <returns></returns>
        Task CreateIndexAsync();
    }
}