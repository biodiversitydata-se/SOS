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
    ///     Invalid event repository
    /// </summary>
    public class InvalidEventRepository : MongoDbProcessedRepositoryBase<InvalidEvent, ObjectId>,
        IInvalidEventRepository
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="logger"></param>
        public InvalidEventRepository(
            IProcessClient client,
            ILogger<InvalidEventRepository> logger
        ) : base(client, true, logger)
        {
        }

        /// <inheritdoc />
        public async Task CreateIndexAsync()
        {
            var indexModels = new []
            {
                new CreateIndexModel<InvalidEvent>(
                    Builders<InvalidEvent>.IndexKeys.Ascending(io => io.DatasetName)),
                new CreateIndexModel<InvalidEvent>(
                    Builders<InvalidEvent>.IndexKeys.Ascending(io => io.EventID))
            };

            Logger.LogDebug("Creating InvalidEvent indexes");
            await MongoCollection.Indexes.CreateManyAsync(indexModels);
        }
    }
}