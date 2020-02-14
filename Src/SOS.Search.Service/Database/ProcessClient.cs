using MongoDB.Driver;
using SOS.Search.Service.Database.Interfaces;

namespace SOS.Search.Service.Database
{
    /// <summary>
    /// Process client.
    /// </summary>
    public class ProcessClient : MongoClient, IProcessClient
    {
        /// <inheritdoc />
        public int BatchSize { get; }

        /// <summary>
        /// Name of database
        /// </summary>
        private readonly string _databaseName;

        /// <summary>
        /// Constructor
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
        public IMongoDatabase GetDatabase()
        {
            return base.GetDatabase(_databaseName);
        }
    }
}
