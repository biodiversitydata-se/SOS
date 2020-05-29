using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using SOS.Lib.Models.Processed.Validation;
using SOS.Process.Database.Interfaces;
using SOS.Process.Repositories.Destination.Interfaces;

namespace SOS.Process.Repositories.Destination
{
    /// <summary>
    ///     Invalid observation repository
    /// </summary>
    public class InvalidObservationRepository : ProcessBaseRepository<InvalidObservation, ObjectId>,
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
            var indexModels = new List<CreateIndexModel<InvalidObservation>>
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