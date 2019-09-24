using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using SOS.Core.Models.Observations;
using SOS.Core.Models.Versioning;

namespace SOS.Core.Repositories
{
    public interface IVersionedObservationRepository<T> where T : class, IObservationKey
    {
        IMongoCollection<VersionedObservation<T>> Collection { get; }
        Task InsertDocumentAsync(T doc);
        Task InsertDocumentsAsync(List<T> speciesObservations);
        Task<VersionedObservation<T>> GetDocumentAsync(int dataProviderId, string catalogNumber);
        Task<VersionedObservation<T>> GetDocumentAsync(ObjectId objectId);
        Task DeleteDocumentAsync(int dataProviderId, string catalogNumber);
        Task<List<ObservationVersionIdentifier>> GetAllObservationVersionIdentifiers();
        IEnumerable<ObservationVersionIdentifier> GetAllObservationVersionIdentifiersEnumerable();
        Task<T> RestoreDocumentAsync(
            int dataProviderId,
            string catalogNumber,
            int version);
    }
}