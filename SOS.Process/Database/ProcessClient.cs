using MongoDB.Driver;

namespace SOS.Process.Database
{
    public class ProcessClient : MongoClient, Interfaces.IProcessClient
    {
        /// <inheritdoc />
        public int BatchSize { get; }

        /// <summary>
        /// Name of database
        /// </summary>
        private readonly string _dataBaseName;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="dataBaseName"></param>
        /// <param name="batchSize"></param>
        public ProcessClient(MongoClientSettings settings, string dataBaseName, int batchSize) : base(settings)
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
