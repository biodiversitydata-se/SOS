using MongoDB.Driver;
using SOS.Observations.Api.Database.Interfaces;

namespace SOS.Observations.Api.Database
{
    /// <summary>
    ///     Process client.
    /// </summary>
    public class ProcessClient : MongoClient, IProcessClient
    {
        /// <summary>
        ///     Name of database
        /// </summary>
        private readonly string _databaseName;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="databaseName"></param>
        /// <param name="batchSize"></param>
        public ProcessClient(MongoClientSettings settings, string databaseName, int batchSize) : base(settings)
        {
            _databaseName = databaseName;
            BatchSize = batchSize;
        }

        /// <inheritdoc />
        public int BatchSize { get; }

        /// <inheritdoc />
        public IMongoDatabase GetDatabase()
        {
            return base.GetDatabase(_databaseName);
        }
    }
}