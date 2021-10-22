using MongoDB.Driver;
using SOS.Lib.Database.Interfaces;

namespace SOS.Lib.Database
{
    /// <summary>
    /// Mongo db base class
    /// </summary>
    public class MongoDbClient : MongoClient, IMongoDbClient
    {
        /// <summary>
        ///     Name of database
        /// </summary>
        private readonly string _databaseName;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="dataBaseName"></param>
        /// <param name="readBatchSize"></param>
        /// <param name="writeBatchSize"></param>
        public MongoDbClient(MongoClientSettings settings, string dataBaseName, int readBatchSize, int writeBatchSize) : base(settings)
        {
            _databaseName = dataBaseName;
            ReadBatchSize = readBatchSize;
            WriteBatchSize = writeBatchSize;
        }

        /// <inheritdoc />
        public int ReadBatchSize { get; }

        /// <inheritdoc />
        public int WriteBatchSize { get; }

        /// <inheritdoc />
        public string DatabaseName => _databaseName;

        /// <inheritdoc />
        public IMongoDatabase GetDatabase()
        {
            return base.GetDatabase(_databaseName);
        }
    }
}