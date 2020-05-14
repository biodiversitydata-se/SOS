using MongoDB.Driver;

namespace SOS.Import.MongoDb
{
    public class ResourceDbClient : MongoClient, Interfaces.IResourceDbClient
    {
        /// <summary>
        /// Name of database
        /// </summary>
        private readonly string _dataBaseName;

        /// <inheritdoc />
        public int BatchSize { get; }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="dataBaseName"></param>
        /// <param name="batchSize"></param>
        public ResourceDbClient(MongoClientSettings settings, string dataBaseName, int batchSize) : base(settings)
        {
            _dataBaseName = dataBaseName;
            BatchSize = batchSize;
        }

        public IMongoDatabase GetDatabase()
        {
            return base.GetDatabase(_dataBaseName);
        }
    }
}