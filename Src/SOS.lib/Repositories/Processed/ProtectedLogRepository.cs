using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Models.Log;
using SOS.Lib.Repositories.Processed.Interfaces;

namespace SOS.Lib.Repositories.Processed
{
    /// <summary>
    ///    Log protected observations 
    /// </summary>
    public class ProtectedLogRepository : MongoDbProcessedRepositoryBase<ProtectedLog, ObjectId>, IProtectedLogRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="logger"></param>
        public ProtectedLogRepository(
            IProcessClient client,
            ILogger<ProtectedLogRepository> logger
        ) : base(client, false, logger)
        {

        }

        /// <inheritdoc />
        public async Task CreateIndexAsync()
        {
            var indexModels = new []
            {
                new CreateIndexModel<ProtectedLog>(
                    Builders<ProtectedLog>.IndexKeys.Ascending(pl => pl.UserId)),
                new CreateIndexModel<ProtectedLog>(
                    Builders<ProtectedLog>.IndexKeys.Ascending(pl => pl.IssueDate))
            };

            Logger.LogDebug("Creating protected log indexes");
            await MongoCollection.Indexes.CreateManyAsync(indexModels);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<ProtectedLog>> SearchAsync(DateTime from, DateTime to)
        {
            var builder = Builders<ProtectedLog>.Filter;
            var fromFilter = builder.Gte(l => l.IssueDate, from);
            var toFilter = builder.Lte(l => l.IssueDate, to);

            return await MongoCollection.Find(fromFilter & toFilter).ToListAsync();
        }
    }
}