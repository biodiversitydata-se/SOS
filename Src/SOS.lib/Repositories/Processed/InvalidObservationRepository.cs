using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Models.Processed.Validation;
using SOS.Lib.Repositories.Processed.Interfaces;

namespace SOS.Lib.Repositories.Processed
{
    /// <summary>
    ///     Invalid observation repository
    /// </summary>
    public class InvalidObservationRepository : MongoDbProcessedRepositoryBase<InvalidObservation, ObjectId>,
        IInvalidObservationRepository
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="logger"></param>
        public InvalidObservationRepository(
            IProcessClient client,
            ILogger<InvalidObservationRepository> logger
        ) : base(client, true, logger)
        {
        }

        /// <inheritdoc />
        public async Task CreateIndexAsync()
        {
            var indexModels = new []
            {
                new CreateIndexModel<InvalidObservation>(
                    Builders<InvalidObservation>.IndexKeys.Ascending(io => io.DatasetName)),
                new CreateIndexModel<InvalidObservation>(
                    Builders<InvalidObservation>.IndexKeys.Ascending(io => io.OccurrenceID))
            };

            Logger.LogDebug("Creating Area indexes");
            await MongoCollection.Indexes.CreateManyAsync(indexModels);
        }
    }
}