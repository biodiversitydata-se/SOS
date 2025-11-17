using MongoDB.Bson;
using SOS.Lib.Models.Processed.Validation;
using System.Threading.Tasks;

namespace SOS.Lib.Repositories.Processed.Interfaces;

/// <summary>
///     Invalid observation repository
/// </summary>
public interface IInvalidObservationRepository : IMongoDbProcessedRepositoryBase<InvalidObservation, ObjectId>
{
    /// <summary>
    ///     Create index
    /// </summary>
    /// <returns></returns>
    Task CreateIndexAsync();
}